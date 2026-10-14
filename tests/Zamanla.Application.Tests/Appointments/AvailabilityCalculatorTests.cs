using Shouldly;
using Zamanla.Application.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Staffs.ValueObjects;

namespace Zamanla.Application.Tests.Appointments;

public sealed class AvailabilityCalculatorTests
{
    // işletmenin saat dilimi; işletim sisteminin saat dilimi listesine bağlı kalmasın diye sabit UTC+3
    private static readonly TimeZoneInfo Istanbul =
        TimeZoneInfo.CreateCustomTimeZone("Test/Istanbul", TimeSpan.FromHours(3), "Istanbul", "Istanbul");

    // 19 Ekim 2026 Pazartesi
    private static readonly DateOnly Monday = new(2026, 10, 19);
    private static readonly DateTimeOffset LastWeek = new(2026, 10, 12, 9, 0, 0, TimeSpan.FromHours(3));

    private static DateTimeOffset At(int hour, int minute) =>
        new(2026, 10, 19, hour, minute, 0, TimeSpan.FromHours(3));

    private static WorkingHour MondayHours(int startHour, int endHour) =>
        new(DayOfWeek.Monday, new TimeOnly(startHour, 0), new TimeOnly(endHour, 0));

    [Fact]
    public void GetSlots_WhenDayIsEmpty_ReturnsSlotsEvery15Minutes()
    {
        // Act
        var slots = AvailabilityCalculator.GetSlots(Monday, Istanbul, [MondayHours(9, 10)], [], 30, 0, LastWeek);

        // Assert
        slots.ShouldBe(new[] { At(9, 0), At(9, 15), At(9, 30) });
    }

    [Fact]
    public void GetSlots_WhenServiceDoesNotFitBeforeLunch_SkipsThatSlot()
    {
        // Arrange: 09–13 ve 14–18, öğle arası 13–14
        WorkingHour[] hours = [MondayHours(9, 13), MondayHours(14, 18)];

        // Act
        var slots = AvailabilityCalculator.GetSlots(Monday, Istanbul, hours, [], 45, 0, LastWeek);

        // Assert
        slots.ShouldContain(At(12, 15));
        slots.ShouldNotContain(At(12, 30));
        slots.ShouldNotContain(At(13, 15));
        slots.ShouldContain(At(14, 0));
    }

    [Fact]
    public void GetSlots_WhenStaffHasAppointment_SkipsSlotsUntilBufferEnds()
    {
        // Arrange: 10:00–10:30 randevu, hazırlık payıyla 10:40'a kadar dolu
        DateTimeRange[] busy = [new(At(10, 0), At(10, 40))];

        // Act
        var slots = AvailabilityCalculator.GetSlots(Monday, Istanbul, [MondayHours(9, 12)], busy, 30, 0, LastWeek);

        // Assert
        slots.ShouldContain(At(9, 30));
        slots.ShouldNotContain(At(9, 45));
        slots.ShouldNotContain(At(10, 30));
        slots.ShouldContain(At(10, 45));
    }

    [Fact]
    public void GetSlots_WhenNewBufferReachesNextAppointment_SkipsSlot()
    {
        // Arrange: 10:00'da başka randevu var; yeni hizmet 30 dk + 15 dk hazırlık
        DateTimeRange[] busy = [new(At(10, 0), At(10, 30))];

        // Act
        var slots = AvailabilityCalculator.GetSlots(Monday, Istanbul, [MondayHours(9, 12)], busy, 30, 15, LastWeek);

        // Assert
        slots.ShouldContain(At(9, 15));
        slots.ShouldNotContain(At(9, 30));
    }

    [Fact]
    public void GetSlots_WhenBufferPassesClosingTime_KeepsLastSlot()
    {
        // Act: son müşteri 09:30'da gelir, temizlik kapanıştan sonra yapılır
        var slots = AvailabilityCalculator.GetSlots(Monday, Istanbul, [MondayHours(9, 10)], [], 30, 15, LastWeek);

        // Assert
        slots.ShouldContain(At(9, 30));
    }

    [Fact]
    public void GetSlots_WhenTimeHasPassed_ReturnsOnlyFutureSlots()
    {
        // Act: saat 09:20
        var slots = AvailabilityCalculator.GetSlots(Monday, Istanbul, [MondayHours(9, 12)], [], 30, 0, At(9, 20));

        // Assert
        slots.First().ShouldBe(At(9, 30));
    }

    [Fact]
    public void GetSlots_WhenStaffDoesNotWorkThatDay_ReturnsEmpty()
    {
        // Arrange: yalnızca salı çalışıyor
        WorkingHour[] hours = [new(DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(18, 0))];

        // Act
        var slots = AvailabilityCalculator.GetSlots(Monday, Istanbul, hours, [], 30, 0, LastWeek);

        // Assert
        slots.ShouldBeEmpty();
    }
}
