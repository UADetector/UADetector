using UaDetector.Results;

namespace UaDetector.Tests.Fixtures.Models;

public class BrowserFixture
{
    public required string UserAgent { get; init; }
    public required Dictionary<string, string?>? Headers { get; init; }
    public required BrowserInfo Browser { get; init; }
}
