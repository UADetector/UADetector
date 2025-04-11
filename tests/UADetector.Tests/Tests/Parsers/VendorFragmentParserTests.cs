using Shouldly;

using UADetector.Parsers;
using UADetector.Tests.Fixtures.Models;
using UADetector.Tests.Helpers;

namespace UADetector.Tests.Tests.Parsers;

public class VendorFragmentParserTests
{
    [Test]
    public async Task TryParseBrand_WithFixtureData_ShouldReturnExpectedBrand()
    {
        var fixturePath = Path.Combine("Fixtures", "Resources", "vendor_fragments.json");
        var fixtures = await FixtureLoader.LoadAsync<VendorFragmentFixture>(fixturePath);

        foreach (var fixture in fixtures)
        {
            VendorFragmentParser.TryParseBrand(fixture.UserAgent, out var result).ShouldBeTrue();

            result.ShouldNotBeNull();
            result.ShouldBe(fixture.Vendor);
        }
    }
}
