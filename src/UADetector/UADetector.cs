using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using UADetector.Models.Constants;
using UADetector.Models.Enums;
using UADetector.Parsers;
using UADetector.Parsers.Devices;
using UADetector.Results;

namespace UADetector;

public sealed class UADetector : IUADetector
{
    private readonly UADetectorOptions _uaDetectorOptions;
    private readonly OsParser _osParser;
    private readonly BrowserParser _browserParser;
    private readonly ClientParser _clientParser;
    private readonly BotParser _botParser;

    private static readonly Regex ContainsLetterRegex = new("[a-zA-Z]", RegexOptions.Compiled);
    private static readonly Regex AndroidVrFragment =
        ParserExtensions.BuildUserAgentRegex("Android( [.0-9]+)?; Mobile VR;| VR ");

    private static readonly Regex ChromeRegex = ParserExtensions.BuildUserAgentRegex("Chrome/[.0-9]*");
    private static readonly Regex MobileRegex = ParserExtensions.BuildUserAgentRegex("(?:Mobile|eliboM)");
    private static readonly Regex PadRegex = ParserExtensions.BuildUserAgentRegex("Pad/APad");
    private static readonly Regex OperaTabletRegex = ParserExtensions.BuildUserAgentRegex("Opera Tablet");
    private static readonly Regex OperaTvStoreRegex = ParserExtensions.BuildUserAgentRegex("Opera TV Store| OMI/");
    private static readonly Regex TouchEnabledRegex = ParserExtensions.BuildUserAgentRegex("Touch");
    private static readonly Regex TvFragmentRegex = ParserExtensions.BuildUserAgentRegex(@"\(TV;");

    private static readonly Regex AndroidTabletFragmentRegex =
        ParserExtensions.BuildUserAgentRegex(@"Android( [.0-9]+)?; Tablet;|Tablet(?! PC)|.*\-tablet$");

    private static readonly Regex AndroidMobileFragmentRegex =
        ParserExtensions.BuildUserAgentRegex(@"Android( [.0-9]+)?; Mobile;|.*\-mobile$");

    private static readonly Regex PuffinSecureBrowserDesktopRegex =
        ParserExtensions.BuildUserAgentRegex(@"Puffin/(?:\d+[.\d]+)[LMW]D");

    private static readonly Regex PuffinWebBrowserSmartphoneRegex =
        ParserExtensions.BuildUserAgentRegex(@"Puffin/(?:\d+[.\d]+)[AIFLW]P");

    private static readonly Regex PuffinWebBrowserTabletRegex =
        ParserExtensions.BuildUserAgentRegex(@"Puffin/(?:\d+[.\d]+)[AILW]T");

    private static readonly Regex AndroidRegex =
        ParserExtensions.BuildUserAgentRegex(@"Andr0id|(?:Android(?: UHD)?|Google) TV|\(lite\) TV|BRAVIA| TV$");

    private static readonly Regex DesktopFragment =
        ParserExtensions.BuildUserAgentRegex("Desktop(?: (x(?:32|64)|WOW64))?;");

    private static readonly FrozenSet<string> AppleOsNames = new[]
    {
        OsNames.IPadOs, OsNames.TvOs, OsNames.WatchOs, OsNames.IOs, OsNames.Mac
    }.ToFrozenSet();

    private static readonly FrozenSet<string> TvBrowsers = new[]
    {
        BrowserNames.Kylo, BrowserNames.EspialTvBrowser, BrowserNames.LujoTvBrowser, BrowserNames.LogicUiTvBrowser,
        BrowserNames.OpenTvBrowser, BrowserNames.SeraphicSraf, BrowserNames.OperaDevices, BrowserNames.CrowBrowser,
        BrowserNames.VewdBrowser, BrowserNames.QuickSearchTv, BrowserNames.QjyTvBrowser, BrowserNames.TvBro,
    }.ToFrozenSet();

    private static readonly FrozenSet<string> TvClients = new[] { "TiviMate" }.ToFrozenSet();

    private readonly IEnumerable<BaseDeviceParser> _deviceParsers =
    [
        new MobileParser(),
        new NotebookParser(),
        new ConsoleParser(),
        new HbbTvParser(),
        new ShellTvParser(),
        new CarBrowserParser(),
        new CameraParser(),
        new PortableMediaPlayerParser()
    ];

    public UADetector(UADetectorOptions? uaDetectorOptions = null)
    {
        _uaDetectorOptions = uaDetectorOptions ?? new UADetectorOptions();
        _osParser = new OsParser(_uaDetectorOptions.VersionTruncation);
        _browserParser = new BrowserParser(_uaDetectorOptions.VersionTruncation);
        _clientParser = new ClientParser(_uaDetectorOptions.VersionTruncation);
        _botParser = new BotParser();
    }

    private static bool IsWindows8OrLater(OsInfo os)
    {
        return os.Name == OsNames.Windows && !string.IsNullOrEmpty(os.Version) &&
               ParserExtensions.TryCompareVersions(os.Version, "8", out var comparisonResult) && comparisonResult >= 0;
    }

    private bool IsDesktop(OsInfo os, BrowserInfo browser)
    {
        return !_browserParser.IsMobileOnlyBrowser(browser.Code) && !string.IsNullOrEmpty(os.Family) &&
               _osParser.IsDesktopOs(os.Family);
    }

    private bool TryParseDevice(
        string userAgent,
        ClientHints clientHints,
        OsInfo? os,
        BrowserInfo? browser,
        ClientInfo? client,
        [NotNullWhen(true)] out DeviceInfo? result
    )
    {
        DeviceType? deviceType = null;
        string? model = null;
        string? brand = null;

        foreach (var parser in _deviceParsers)
        {
            if (parser.TryParse(userAgent, clientHints, out var deviceInfo))
            {
                model = deviceInfo.Model;
                break;
            }
        }

        // If the user agent does not specify a model, use the one from client hints.
        if (string.IsNullOrEmpty(model))
        {
            model = clientHints.Model;
        }

        if (string.IsNullOrEmpty(brand))
        {
            VendorFragmentParser.TryParseBrand(userAgent, out brand);
        }

        // Prevent misidentification of spoofed user agent as legitimate Apple.
        if (brand == BrandNames.Apple && !AppleOsNames.Contains(brand))
        {
            deviceType = null;
            brand = null;
            model = null;
        }

        // Assume all devices running iOS or macOS are manufactured by Apple.
        if (string.IsNullOrEmpty(brand) && os is not null && AppleOsNames.Contains(os.Name))
        {
            brand = BrandNames.Apple;
        }

        // User agents containing the fragment 'VR' are assumed to represent wearables.
        if (deviceType is null && AndroidVrFragment.IsMatch(userAgent))
        {
            deviceType = DeviceType.Wearable;
        }

        // Chrome on Android uses the 'Mobile' keyword in the user agent to indicate device type.
        // If 'Mobile' is present, the device is considered a smartphone; otherwise, it's a tablet.
        //
        // Note: The browser family is not checked, as some mobile apps may use Chrome without a detected browser.
        // Instead, the user agent is directly checked for the presence of 'Chrome'.
        if (deviceType is null && os?.Family == OsFamilies.Android && ChromeRegex.IsMatch(userAgent))
        {
            deviceType = MobileRegex.IsMatch(userAgent) ? DeviceType.Smartphone : DeviceType.Tablet;
        }

        // User agents containing the fragment 'Pad' or 'APad' are assumed to represent tablets.
        if (deviceType == DeviceType.Smartphone && PadRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Tablet;
        }

        // User agents containing the fragments 'Android; Tablet;' or 'Opera Tablet' are assumed to represent tablets.
        if (deviceType is null &&
            (AndroidTabletFragmentRegex.IsMatch(userAgent) || OperaTabletRegex.IsMatch(userAgent)))
        {
            deviceType = DeviceType.Tablet;
        }

        // User agents containing the fragment 'Android; Mobile;' are assumed to represent smartphones.
        if (deviceType is null && AndroidMobileFragmentRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Smartphone;
        }

        // Android versions up to 3.0 were designed exclusively for smartphones.
        // However, due to the late release of 3.0 (which was tablet-only), many tablets were running Android 2.x.
        // Starting with Android 4.0, the platform was unified, supporting both smartphones and tablets.
        // 
        // Therefore, it is expected that:
        // - Devices running Android versions earlier than 2.0 are smartphones.
        // - Devices running Android 3.x are tablets.
        // - Devices running Android 2.x and 4.x+ have an unknown device type.
        if (deviceType is null && os?.Name == OsNames.Android && !string.IsNullOrEmpty(os.Version))
        {
            if (ParserExtensions.TryCompareVersions(os.Version, "2.0", out var comparisonResult) &&
                comparisonResult == -1)
            {
                deviceType = DeviceType.Smartphone;
            }
            else if (ParserExtensions.TryCompareVersions(os.Version, "3.0", out comparisonResult) &&
                     comparisonResult >= 0 &&
                     ParserExtensions.TryCompareVersions(os.Version, "4.0", out comparisonResult) &&
                     comparisonResult == -1)
            {
                deviceType = DeviceType.Tablet;
            }
        }

        // Android feature phones are likely to be smartphones.
        if (deviceType == DeviceType.FeaturePhone && os?.Family == OsFamilies.Android)
        {
            deviceType = DeviceType.Smartphone;
        }

        // Unknown devices running Java ME are likely feature phones.
        if (deviceType is null && os?.Name == OsNames.JavaMe)
        {
            deviceType = DeviceType.FeaturePhone;
        }

        // Devices running KaiOS are likely to be feature phones.
        if (os?.Name == OsNames.KaiOs)
        {
            deviceType = DeviceType.FeaturePhone;
        }

        // According to http://msdn.microsoft.com/en-us/library/ie/hh920767(v=vs.85).aspx, 
        // Internet Explorer 10 introduces the "Touch" UA string token. If this token appears at the end of the 
        // UA string, the device has touch capabilities and is running Windows 8 (or later).
        // This token is transmitted by touch-enabled systems running Windows 8 (or Windows RT).
        // 
        // Since most touch-enabled devices are tablets, with desktops and notebooks being the exception, 
        // it is assumed that all Windows 8 touch devices are tablets.
        if (deviceType is null && os is not null && (os.Name == OsNames.WindowsRt || IsWindows8OrLater(os)) &&
            TouchEnabledRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Tablet;
        }

        // Devices running Puffin Secure Browser that include the letter 'D' are assumed to be desktops.
        if (deviceType is null && PuffinSecureBrowserDesktopRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Desktop;
        }

        // Devices running Puffin Web Browser that include the letter 'P' are assumed to be smartphones.
        if (deviceType is null && PuffinWebBrowserSmartphoneRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Smartphone;
        }

        // Devices running Puffin Web Browser that include the letter 'T' are assumed to be smartphones.
        if (deviceType is null && PuffinWebBrowserTabletRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Tablet;
        }

        // Devices running Opera TV Store are assumed to be TVs.
        if (OperaTvStoreRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Tv;
        }

        // Devices running Coolita OS are assumed to be TVs.
        if (os?.Name == OsNames.CoolitaOs)
        {
            deviceType = DeviceType.Tv;
        }

        // Devices containing "Andr0id" in the user agent string are assumed to be TVs.
        if (deviceType != DeviceType.Tv && deviceType != DeviceType.Peripheral && AndroidRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Tv;
        }

        // Devices using these clients are assumed to be TVs.
        if (browser is not null && TvBrowsers.Contains(browser.Name) ||
            client is not null && TvClients.Contains(client.Name))
        {
            deviceType = DeviceType.Tv;
        }

        // User agents containing the "TV" fragment are assumed to be TVs.
        if (deviceType is null && TvFragmentRegex.IsMatch(userAgent))
        {
            deviceType = DeviceType.Tv;
        }

        // User agents containing the "Desktop" fragment are assumed to be desktops.
        if (deviceType != DeviceType.Desktop && !userAgent.Contains("Desktop") &&
            DesktopFragment.IsMatch(userAgent))
        {
            deviceType = DeviceType.Desktop;
        }

        if (deviceType is null && os is not null && browser is not null && IsDesktop(os, browser))
        {
            deviceType = DeviceType.Desktop;
        }

        if (deviceType is null)
        {
            result = null;
        }
        else
        {
            result = new DeviceInfo
            {
                Type = deviceType.Value,
                Model = model,
                Brand = brand is not null && BaseDeviceParser.BrandNameMapping.TryGetValue(brand, out var brandCode)
                    ? new BrandInfo { Name = brand, Code = brandCode, }
                    : null
            };
        }

        return result is not null;
    }


    public bool TryParse(string userAgent, [NotNullWhen(true)] out UserAgentInfo? result)
    {
        return TryParse(userAgent, ImmutableDictionary<string, string?>.Empty, out result);
    }

    public bool TryParse(
        string userAgent,
        IDictionary<string, string?> headers,
        [NotNullWhen(true)] out UserAgentInfo? result
    )
    {
        if (string.IsNullOrEmpty(userAgent) || !ContainsLetterRegex.IsMatch(userAgent))
        {
            result = null;
            return false;
        }

        var clientHints = ClientHints.Create(headers);

        if (ParserExtensions.TryRestoreUserAgent(userAgent, clientHints, out var restoredUserAgent))
        {
            userAgent = restoredUserAgent;
        }

        ClientInfo? client = null;
        BotInfo? bot = null;
        bool isBot = false;

        if (!_uaDetectorOptions.SkipBotParsing)
        {
            if (_uaDetectorOptions.SkipBotDetails)
            {
                isBot = _botParser.IsBot(userAgent);
            }
            else
            {
                _botParser.TryParse(userAgent, out bot);
            }
        }

        _osParser.TryParse(userAgent, clientHints, out var os);

        if (!_browserParser.TryParse(userAgent, clientHints, out var browser))
        {
            _clientParser.TryParse(userAgent, clientHints, out client);
        }

        TryParseDevice(userAgent, clientHints, os, browser, client, out var device);

        if ((_uaDetectorOptions.SkipBotParsing || !_uaDetectorOptions.SkipBotParsing && bot is null) &&
            os is null && browser is null && client is null && device is null)
        {
            result = null;
        }
        else
        {
            result = new UserAgentInfo
            {
                IsBot = isBot,
                Device = device,
                Os = os,
                Browser = browser,
                Client = client,
                Bot = bot,
            };
        }

        return result is not null;
    }
}
