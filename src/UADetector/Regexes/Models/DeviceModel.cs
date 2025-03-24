using System.Text.RegularExpressions;

using YamlDotNet.Serialization;

namespace UADetector.Regexes.Models;

internal sealed class DeviceModel
{
    public required Regex Regex { get; init; }
    
    [YamlMember(Alias = "model")]
    public required string Name { get; init; }
}
