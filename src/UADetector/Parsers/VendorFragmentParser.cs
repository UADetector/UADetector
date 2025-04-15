using System.Diagnostics.CodeAnalysis;

using UADetector.Regexes.Models;
using UADetector.Utils;

namespace UADetector.Parsers;

internal static class VendorFragmentParser
{
    private const string ResourceName = "Regexes.Resources.vendor_fragments.json";

    private static readonly IEnumerable<VendorFragment> VendorFragments =
        RegexLoader.LoadRegexes<VendorFragment>(ResourceName, "[^a-z0-9]+");


    public static bool TryParseBrand(string userAgent, [NotNullWhen(true)] out string? result)
    {
        foreach (var vendorFragment in VendorFragments)
        {
            foreach (var regex in vendorFragment.Regexes)
            {
                if (regex.IsMatch(userAgent))
                {
                    result = vendorFragment.Brand;
                    return true;
                }
            }
        }

        result = null;
        return false;
    }
}
