using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using UaDetector.Models.Enums;
using UaDetector.Regexes.Models;
using UaDetector.Results;
using UaDetector.Utils;

namespace UaDetector.Parsers.Devices;

internal sealed class HbbTvParser : DeviceParserBase
{
    private const string ResourceName = "Regexes.Resources.Devices.televisions.json";
    private static readonly IEnumerable<Device> Televisions = RegexLoader.LoadRegexes<Device>(ResourceName);

    internal static readonly Regex
        HbbTvRegex = RegexUtility.BuildUserAgentRegex(@"(?:HbbTV|SmartTvA)/([1-9](?:\.[0-9]){1,2})");


    public override bool TryParse(
        string userAgent,
        [NotNullWhen(true)] out InternalDeviceInfo? result
    )
    {
        if (HbbTvRegex.IsMatch(userAgent))
        {
            if (!TryParse(userAgent, Televisions, out result))
            {
                result = new InternalDeviceInfo { Type = DeviceType.Tv, Brand = null, Model = null, };
            }
        }
        else
        {
            result = null;
        }

        return result is not null;
    }
}
