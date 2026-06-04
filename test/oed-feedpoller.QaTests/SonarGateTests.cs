using Altinn.Dd.Tests.SonarGate;
using Xunit;
using Xunit.Abstractions;

namespace oed_feedpoller.QaTests;

// Opt-in SonarQube quality-gate test for oed-feedpoller (Azure Function). The actual runner
// lives in the Altinn.Dd.Tests.SonarGate package — this file is just the option blob. See
// https://altinn.studio/repos/digdir/dd-qa for the package source.
//
// Test lives under test/ (not the repo root) because oed-feedpoller.csproj sits at the root and
// would otherwise greedily include this file in its own compile via the default Compile glob.
//
// Run with:  $env:QATESTS = "1"; dotnet test ./test/oed-feedpoller.QaTests
public class SonarGateTests(ITestOutputHelper output)
{
    [SkippableFact, Trait("Category", "qa")]
    public Task QualityGate_ReturnsOk() => SonarGate.RunAsync(new()
    {
        ProjectKey = "oed-feedpoller",
        ScanCsprojRelativePath = "oed-feedpoller.csproj",
    }, output);
}
