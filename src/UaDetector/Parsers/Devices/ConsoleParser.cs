using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using UaDetector.Regexes.Models;
using UaDetector.Results;
using UaDetector.Utils;

namespace UaDetector.Parsers.Devices;

internal sealed class ConsoleParser : DeviceParserBase
{
    private const string ResourceName = "Regexes.Resources.Devices.consoles.json";
    private static readonly IEnumerable<Device> Consoles;
    private static readonly Regex CombinedRegex;


    static ConsoleParser()
    {
        (Consoles, CombinedRegex) =
            RegexLoader.LoadRegexesWithCombined<Device>(ResourceName);
    }

    public override bool TryParse(
        string userAgent,
        [NotNullWhen(true)] out InternalDeviceInfo? result
    )
    {
        if (CombinedRegex.IsMatch(userAgent))
        {
            TryParse(userAgent, Consoles, out result);
        }
        else
        {
            result = null;
        }

        return result is not null;
    }
}
