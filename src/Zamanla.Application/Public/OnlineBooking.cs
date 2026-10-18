namespace Zamanla.Application.Public;

// online randevu kuralları; salon ayarları gelince oraya taşınabilir
public static class OnlineBooking
{
    public const int MaxDaysAhead = 60;

    // aynı telefonla kendisi ve yakınları için alınabilir; takvim sahte randevularla doldurulmasın
    public const int MaxUpcomingPerPhone = 5;
}
