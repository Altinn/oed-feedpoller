using Altinn.Dd.Tests.SonarGate;
using Xunit;
using Xunit.Abstractions;

namespace oed_feedpoller.QaTests;

// Opt-in SonarQube quality-gate test for oed-feedpoller (Azure Function). The actual runner
// lives in the Altinn.Dd.Tests.SonarGate package — this file is just the option blob. See
// https://altinn.studio/repos/digdir/dd-qa for the package source.
// Run with:  $env:QATESTS = "1"; dotnet test ./tests/oed-feedpoller.QaTests
public class SonarGateTests(ITestOutputHelper output)
{
    [SkippableFact, Trait("Category", "qa")]
    public Task QualityGate_ReturnsOk() => SonarGate.RunAsync(new()
    {
        ProjectKey = "oed-feedpoller",
        ScanCsprojRelativePath = "src/oed-feedpoller/oed-feedpoller.csproj",
        Coverage = new()
        {
            TestCsprojRelativePath = "tests/oed-feedpoller.UnitTests/oed-feedpoller.UnitTests.csproj",
            Excludes =
            [
                "[xunit.*]*",
                "[oed-feedpoller.UnitTests]*",
                "[oed-feedpoller.QaTests]*",
            ],
        },
    }, output);
}
