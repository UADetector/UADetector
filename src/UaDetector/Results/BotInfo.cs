using UaDetector.Models.Enums;

namespace UaDetector.Results;

public sealed class BotInfo
{
    public required string Name { get; init; }
    public BotCategory? Category { get; init; }
    public string? Url { get; init; }
    public ProducerInfo? Producer { get; init; }

    public override string ToString()
    {
        return string.Join(
            ", ",
            new[]
            {
                $"{nameof(Name)}: {Name}",
                Category is null ? null : $"{nameof(Category)}: {Category}",
                string.IsNullOrEmpty(Url) ? null : $"{nameof(Url)}: {Url}",
                Producer is null ? null : $"{nameof(Producer)}: {Producer}",
            }.Where(x => !string.IsNullOrEmpty(x))
        );
    }
}
