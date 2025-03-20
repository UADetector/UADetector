using System.Text.RegularExpressions;

using UADetector.Regexes.Models.Clients;

namespace UADetector.Regexes.Models.Browsers;

internal sealed class Browser
{
    public required Regex Regex { get; init; }
    public required string Name { get; init; }
    public string? Version { get; init; }
    public Engine? Engine { get; init; }
}
