using System;
using Digdir.Oed.FeedPoller;
using Xunit;

namespace oed_feedpoller.UnitTests;

/// <summary>
/// Behavior tests for <see cref="Helpers.ShouldRunUpdate(DateTime?)"/>, the scheduling
/// gate that decides whether a feed poll runs at a given Norwegian wall-clock time.
///
/// All tests pass an explicit <see cref="DateTime"/> rather than using the parameterless
/// overload, so they are deterministic and OS-independent (the parameterless path resolves
/// the Windows-only "W. Europe Standard Time" zone id, which is not present on all hosts).
///
/// The <see cref="DateTime"/> date component is irrelevant to the logic; only Hour/Minute
/// are read. A fixed date is used purely for clarity.
/// </summary>
public class HelpersShouldRunUpdateTests
{
    private static DateTime At(int hour, int minute) => new(2026, 6, 16, hour, minute, 0);

    // Busy hours 06:00–17:59: always run, regardless of minute.
    [Theory]
    [InlineData(6, 0)]
    [InlineData(6, 15)]
    [InlineData(12, 30)]
    [InlineData(17, 0)]
    [InlineData(17, 59)]
    public void ShouldRunUpdate_DuringBusyHours_ReturnsTrue(int hour, int minute)
    {
        Assert.True(Helpers.ShouldRunUpdate(At(hour, minute)));
    }

    // Maintenance window hours 01:00–04:59: never run, regardless of minute.
    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 30)]
    [InlineData(2, 30)]
    [InlineData(3, 0)]
    [InlineData(4, 0)]
    [InlineData(4, 59)]
    public void ShouldRunUpdate_DuringMaintenanceWindow_ReturnsFalse(int hour, int minute)
    {
        Assert.False(Helpers.ShouldRunUpdate(At(hour, minute)));
    }

    // Transition hours (00, 05, 18–23): run only "every half hour", i.e. when the minute is
    // within {58,59,0,1,2} (around the hour) or {28,29,30,31,32} (around the half hour),
    // matching a timer that fires at most every 5 minutes.
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 30)]
    [InlineData(5, 28)]
    [InlineData(5, 30)]
    [InlineData(5, 58)]
    [InlineData(5, 59)]
    [InlineData(18, 0)]
    [InlineData(18, 2)]
    [InlineData(18, 30)]
    [InlineData(20, 1)]
    [InlineData(23, 32)]
    [InlineData(23, 59)]
    public void ShouldRunUpdate_DuringTransitionHours_OnTheHalfHour_ReturnsTrue(int hour, int minute)
    {
        Assert.True(Helpers.ShouldRunUpdate(At(hour, minute)));
    }

    // Transition hours, but outside the half-hour windows: do not run.
    [Theory]
    [InlineData(0, 10)]
    [InlineData(0, 27)]
    [InlineData(0, 33)]
    [InlineData(5, 20)]
    [InlineData(18, 3)]
    [InlineData(18, 15)]
    [InlineData(23, 45)]
    [InlineData(23, 57)]
    public void ShouldRunUpdate_DuringTransitionHours_OffTheHalfHour_ReturnsFalse(int hour, int minute)
    {
        Assert.False(Helpers.ShouldRunUpdate(At(hour, minute)));
    }

    // Handover boundary: the last busy minute always runs, but one hour later (now a transition
    // hour) the same minute is off-window and does not run.
    [Fact]
    public void ShouldRunUpdate_BusyToTransitionHandover_IsRespected()
    {
        Assert.True(Helpers.ShouldRunUpdate(At(17, 59)));   // last busy hour, always true
        Assert.False(Helpers.ShouldRunUpdate(At(18, 15)));  // transition hour, off-window
    }

    // Documents the discrepancy between the comment in Helpers.cs ("At 0400-0559 ... update
    // every half hour") and the actual behavior: hour 04 is caught by the 01:00–04:59
    // maintenance rule first, so 04:30 never runs despite the comment. This test pins the
    // ACTUAL behavior; whether the comment or the behavior is wrong is a separate decision.
    [Fact]
    public void ShouldRunUpdate_At0430_ReturnsFalse_DespiteHalfHourComment()
    {
        Assert.False(Helpers.ShouldRunUpdate(At(4, 30)));
    }
}
