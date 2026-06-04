using Altinn.Dd.Tests.SonarGate;
using Xunit;
using Xunit.Abstractions;

namespace QaTests;

// Opt-in SonarQube quality-gate test for oed-feedpoller (Azure Function). The actual runner
// lives in the Altinn.Dd.Tests.SonarGate package — this file is just the option blob. See
// https://altinn.studio/repos/digdir/dd-qa for the package source.
//
// Run with:  $env:QATESTS = "1"; dotnet test ./QaTests/QaTests.csproj
public class SonarGateTests(ITestOutputHelper output)
{
    [SkippableFact, Trait("Category", "qa")]
    public Task QualityGate_ReturnsOk() => SonarGate.RunAsync(new()
    {
        ProjectKey = "oed-feedpoller",
        ScanCsprojRelativePath = "oed-feedpoller.csproj",
    }, output);
}
