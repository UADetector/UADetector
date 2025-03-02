using System.Text.RegularExpressions;

using UADetector.Models.Enums;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace UADetector.Parsers;

internal sealed class YamlRegexConverter : IYamlTypeConverter
{
    private readonly RegexPatternType _patternType;
    private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
    private const string UserAgentRegexPattern = "(?:^|[^A-Z0-9_-]|[^A-Z0-9-]_|sprd-|MZ-)(?:{0})";


    public YamlRegexConverter(RegexPatternType patternType)
    {
        _patternType = patternType;
    }

    public bool Accepts(Type type) => type == typeof(Regex);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        parser.TryConsume<Scalar>(out var scalar);

        if (scalar is null)
        {
            return null;
        }

        return _patternType switch
        {
            RegexPatternType.None => new Regex(scalar.Value, Options),
            RegexPatternType.UserAgent => new Regex(string.Format(UserAgentRegexPattern, scalar.Value), Options),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var regex = (Regex)value!;
        emitter.Emit(new Scalar(regex.ToString()));
    }
}
