using GenericRepository;
using TS.MediatR;
using Zamanla.Application.Auth;
using Zamanla.Application.Permissions;
using Zamanla.Application.Services;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Notifications;
using Zamanla.Domain.Notifications.ValueObjects;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.SalonServices.ValueObjects;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Tenants.ValueObjects;
using Zamanla.Domain.TimeOffs;
using Zamanla.Domain.Users;
using Zamanla.Domain.Users.ValueObjects;

namespace Zamanla.WebAPI;

public static class ExtensionMethods
{
    private const string DemoSlug = "demo-kuafor";
    // yönetim sayfasını denemek için bilinen link: /b/demo-kuafor/r/demo-randevu
    private const string DemoManageToken = "demo-randevu";

    // projeyi ilk açan dolu bir salonla karşılaşsın; demo salon zaten varsa bir şey yapmaz
    // bütün isimler, numaralar ve e-postalar uydurmadır
    public static async Task SeedDemoDataAsync(this WebApplication app)
    {
        using var scoped = app.Services.CreateScope();
        var srv = scoped.ServiceProvider;
        var tenantRepository = srv.GetRequiredService<ITenantRepository>();
        var salonServiceRepository = srv.GetRequiredService<ISalonServiceRepository>();
        var staffRepository = srv.GetRequiredService<IStaffRepository>();
        var appUserRepository = srv.GetRequiredService<IAppUserRepository>();
        var timeOffRepository = srv.GetRequiredService<ITimeOffRepository>();
        var customerRepository = srv.GetRequiredService<ICustomerRepository>();
        var appointmentRepository = srv.GetRequiredService<IAppointmentRepository>();
        var notificationRepository = srv.GetRequiredService<INotificationRepository>();
        var unitOfWork = srv.GetRequiredService<IUnitOfWork>();
        var sender = srv.GetRequiredService<ISender>();

        if (await tenantRepository.SlugExistsAsync(new Slug(DemoSlug)))
        {
            return;
        }

        var now = srv.GetRequiredService<TimeProvider>().GetUtcNow();
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, timeZone).DateTime);
        // sabit tohum: veri her kurulumda aynı düzende dağılsın
        Random random = new(2026);

        #region Salon ve Sahibi
        // kayıt ekranıyla aynı yol: salon ve sahibi birlikte açılır, şifre hash'lenir
        var register = await sender.Send(new RegisterBusinessCommand(
             "Demo Kuaför", DemoSlug, "Derya", "Yalçın", "demo@zamanla.test", "Demo12345!"));
        if (!register.IsSuccessful)
        {
            return;
        }

        var tenant = (await tenantRepository.FirstOrDefaultAsync(t => t.Slug.Value == DemoSlug))!;
        tenant.SetAddress(new Address("İstanbul", "Kadıköy", "Moda Cad. No: 12"));
        tenant.SetPhoneNumber(new PhoneNumber("0216 000 00 00"));
        tenantRepository.Update(tenant);
        srv.GetRequiredService<ITenantContext>().SetTenantId(tenant.Id);
        #endregion

        #region Hizmetler
        SalonService haircut = new(new Name("Saç kesimi"), new Category("Kesim"), new Duration(45), new Duration(10), new Price(450));
        SalonService cutAndBlow = new(new Name("Kesim + fön"), new Category("Kesim"), new Duration(60), new Duration(10), new Price(650));
        SalonService mensCut = new(new Name("Erkek saç kesimi"), new Category("Kesim"), new Duration(30), new Duration(5), new Price(350));
        SalonService kidsCut = new(new Name("Çocuk saç kesimi"), new Category("Kesim"), new Duration(30), new Duration(5), new Price(250));
        SalonService blowDry = new(new Name("Fön"), new Category("Fön"), new Duration(30), new Duration(5), new Price(300));
        SalonService wavyBlow = new(new Name("Dalgalı fön"), new Category("Fön"), new Duration(45), new Duration(5), new Price(400));
        SalonService coloring = new(new Name("Saç boyama"), new Category("Renk"), new Duration(90), new Duration(15), new Price(1200));
        SalonService rootColor = new(new Name("Dip boya"), new Category("Renk"), new Duration(60), new Duration(10), new Price(800));
        SalonService highlights = new(new Name("Röfle"), new Category("Renk"), new Duration(120), new Duration(15), new Price(1800));
        SalonService keratin = new(new Name("Keratin bakımı"), new Category("Bakım"), new Duration(120), new Duration(15), new Price(3000));
        SalonService mask = new(new Name("Saç bakım maskesi"), new Category("Bakım"), new Duration(30), new Duration(5), new Price(350));
        SalonService beard = new(new Name("Sakal tıraşı"), new Category("Sakal"), new Duration(20), new Duration(5), new Price(200));
        // artık yapılmıyor; pasif hizmet görünümü için
        SalonService perm = new(new Name("Perma"), new Category("Bakım"), new Duration(120), new Duration(15), new Price(1500));
        perm.Deactivate();
        salonServiceRepository.AddRange([haircut, cutAndBlow, mensCut, kidsCut, blowDry, wavyBlow, coloring, rootColor, highlights, keratin, mask, beard, perm]);
        #endregion

        #region Personel ve Hesaplar
        DayOfWeek[] mondayToSaturday = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday];
        DayOfWeek[] tuesdayToSaturday = [.. mondayToSaturday.Skip(1)];

        Staff elif = new(
            new FirstName("Elif"),
            new LastName("Aydın"),
            new Color("peach"),
            mondayToSaturday.SelectMany(day => new[]
            {
                new WorkingHour(day, new TimeOnly(9, 0), new TimeOnly(13, 0)),
                new WorkingHour(day, new TimeOnly(14, 0), new TimeOnly(19, 0))
            }),
            [new(haircut.Id), new(cutAndBlow.Id), new(kidsCut.Id), new(blowDry.Id), new(wavyBlow.Id), new(coloring.Id), new(rootColor.Id), new(highlights.Id)]);
        Staff mert = new(
            new FirstName("Mert"),
            new LastName("Kaya"),
            new Color("sage"),
            mondayToSaturday.Select(day => new WorkingHour(day, new TimeOnly(10, 0), new TimeOnly(19, 0))),
            [new(haircut.Id), new(cutAndBlow.Id), new(mensCut.Id), new(kidsCut.Id), new(beard.Id)]);
        // pazartesi izinli
        Staff zeynep = new(
            new FirstName("Zeynep"),
            new LastName("Arslan"),
            new Color("lavender"),
            tuesdayToSaturday.Select(day => new WorkingHour(day, new TimeOnly(10, 0), new TimeOnly(18, 0))),
            [new(blowDry.Id), new(wavyBlow.Id), new(coloring.Id), new(rootColor.Id), new(highlights.Id), new(keratin.Id), new(mask.Id)]);
        // hesabı yok; yalnızca takvimde görünür
        Staff can = new(
            new FirstName("Can"),
            new LastName("Yıldız"),
            new Color("sky"),
            tuesdayToSaturday.Select(day => new WorkingHour(day, new TimeOnly(11, 0), new TimeOnly(19, 0))),
            [new(haircut.Id), new(mensCut.Id), new(beard.Id), new(mask.Id)]);
        staffRepository.AddRange([elif, mert, zeynep, can]);

        // davet kabulüyle aynı sonuç: personele bağlı hesap ve seçilen yetkiler
        AppUser elifUser = new(tenant.Id, new FirstName("Elif"), new LastName("Aydın"), new Email("elif@zamanla.test"), new Password("Elif2026!"), UserRole.Staff);
        elifUser.SetStaffId(elif.Id);
        elifUser.SetPermissions([new Permission(PermissionCatalog.TimeOffsManage)]);
        // yetkisi yok: yalnızca kendi randevularını görür
        AppUser mertUser = new(tenant.Id, new FirstName("Mert"), new LastName("Kaya"), new Email("mert@zamanla.test"), new Password("Mert2026!"), UserRole.Staff);
        mertUser.SetStaffId(mert.Id);
        appUserRepository.AddRange([elifUser, mertUser]);
        #endregion

        #region İzinler
        var zeynepLeave = new TimeOff(zeynep.Id, DayRange(today.AddDays(7), 2, timeZone), new Note("Yıllık izin"));
        // pazar salon kapalı; rapor günü çalışılan bir güne denk gelsin
        var sickDay = today.AddDays(-10);
        if (sickDay.DayOfWeek == DayOfWeek.Sunday)
        {
            sickDay = sickDay.AddDays(1);
        }

        var mertSickDay = new TimeOff(mert.Id, DayRange(sickDay, 1, timeZone), new Note("Rapor"));
        TimeOff[] timeOffs = [zeynepLeave, mertSickDay];
        timeOffRepository.AddRange(timeOffs);
        #endregion

        #region Müşteriler
        string[] womenNames = ["Ayşe", "Zehra", "Selin", "Ece", "Deniz", "Merve", "Büşra", "Gizem", "Esra", "Melis", "Ceren", "İrem", "Pelin", "Sevgi", "Nazlı", "Aslı"];
        string[] menNames = ["Mehmet", "Burak", "Emre", "Oğuz", "Kerem", "Tolga", "Serkan", "Umut", "Barış", "Onur", "Volkan", "Hakan", "Cem", "Kaan", "Arda", "Murat"];
        string[] lastNames = ["Yılmaz", "Demir", "Kaya", "Öztürk", "Çelik", "Şahin", "Koç", "Arslan", "Doğan", "Kılıç", "Aslan", "Çetin", "Kara", "Koçak", "Kurt", "Özdemir", "Polat", "Aydın", "Erdoğan", "Yıldız", "Aksoy", "Güneş", "Tekin", "Bulut"];

        // ad ve soyad sırası çakışmasın diye farklı adımlarla dolaşılır
        var women = Enumerable.Range(0, 48).Select(i => (womenNames[i % 16], lastNames[i * 7 % 24])).ToList();
        var men = Enumerable.Range(0, 32).Select(i => (menNames[i % 16], lastNames[(i * 5 + 3) % 24])).ToList();
        Dictionary<int, string> notes = new()
        {
            [0] = "Boya 6/1, amonyak alerjisi var",
            [3] = "Kısa kesim sever, ense makineyle",
            [7] = "Saç derisi hassas, keratin yapılmasın",
            [52] = "Sakalı yalnızca makasla",
            [55] = "Oğlunu da getiriyor, çocuk kesimi"
        };

        var customers = women.Concat(men)
            .Select((name, index) => new Customer(
                new FirstName(name.Item1),
                new LastName(name.Item2),
                new PhoneNumber($"0555 000 {index / 100:00} {index % 100:00}"),
                // üç müşteriden ikisinin e-postası var
                index % 3 == 2 ? null : new Email($"{ToAscii(name.Item1)}.{ToAscii(name.Item2)}@ornek.test"),
                notes.TryGetValue(index, out var note) ? new Note(note) : null))
            .ToList();
        customerRepository.AddRange(customers);

        var womenCustomers = customers.Take(women.Count).ToList();
        var menCustomers = customers.Skip(women.Count).ToList();
        // birkaç kez gelmeyen müşteriler; 3 ve üstünde takvimde uyarı çıkar
        Customer[] unreliable = [womenCustomers[5], menCustomers[1]];
        #endregion

        #region Randevular
        // her personelin günlük şablonu: saat ve o saate sığan hizmet seçenekleri;
        // hazırlık payı dahil bir sonraki saate ve mesai bitişine taşmaz
        var schedules = new (Staff Staff, List<Customer> Pool, (int Hour, int Minute, SalonService[][] Options)[] Slots)[]
        {
            (elif, womenCustomers,
            [
                (9, 0, [[haircut], [blowDry], [kidsCut]]),
                (10, 0, [[cutAndBlow], [rootColor]]),
                (11, 15, [[blowDry], [kidsCut], [haircut]]),
                (12, 15, [[blowDry], [kidsCut]]),
                (14, 0, [[coloring], [rootColor]]),
                (16, 0, [[highlights], [coloring]]),
                (18, 15, [[blowDry]])
            ]),
            (mert, menCustomers,
            [
                (10, 0, [[mensCut], [beard]]),
                (10, 45, [[haircut, beard], [cutAndBlow]]),
                (12, 0, [[beard], [mensCut]]),
                (13, 30, [[mensCut], [kidsCut]]),
                (14, 15, [[cutAndBlow], [haircut]]),
                (15, 30, [[kidsCut], [mensCut]]),
                (16, 15, [[mensCut, beard]]),
                (17, 30, [[haircut], [mensCut]]),
                (18, 30, [[beard]])
            ]),
            (zeynep, womenCustomers,
            [
                (10, 0, [[keratin], [highlights]]),
                (12, 30, [[wavyBlow], [blowDry]]),
                (13, 30, [[coloring], [rootColor]]),
                (15, 30, [[mask], [blowDry]]),
                (16, 15, [[blowDry], [mask]]),
                (17, 0, [[wavyBlow]])
            ]),
            (can, menCustomers,
            [
                (11, 0, [[mensCut]]),
                (11, 45, [[beard]]),
                (12, 30, [[haircut]]),
                (14, 0, [[mensCut, beard]]),
                (15, 0, [[mask]]),
                (16, 0, [[mensCut]]),
                (17, 0, [[haircut]]),
                (18, 0, [[mensCut]])
            ])
        };

        // son 4 hafta dolu, önümüzdeki 2 haftada online randevuya yer kalsın
        List<Appointment> appointments = [];
        List<Notification> notifications = [];
        var demoTokenUsed = false;
        for (var date = today.AddDays(-28); date <= today.AddDays(13); date = date.AddDays(1))
        {
            // aynı müşteri aynı gün iki kez gelmesin
            HashSet<Guid> bookedToday = [];
            foreach (var (staff, pool, slots) in schedules)
            {
                if (!staff.WorkingHours.Any(h => h.Day == date.DayOfWeek) ||
                    timeOffs.Any(t => t.StaffId == staff.Id && Overlaps(t.Time, date, timeZone)))
                {
                    continue;
                }

                foreach (var slot in slots)
                {
                    var local = date.ToDateTime(new TimeOnly(slot.Hour, slot.Minute));
                    var startAt = new DateTimeOffset(local, timeZone.GetUtcOffset(local));
                    var isPast = startAt < now;
                    var fill = (isPast ? 70 : 40) + (date.DayOfWeek == DayOfWeek.Saturday ? 15 : 0);
                    if (random.Next(100) >= fill)
                    {
                        continue;
                    }

                    var roll = random.Next(100);
                    var isNoShow = isPast && roll < 3;
                    var isCancelled = isPast ? roll is >= 3 and < 8 : roll < 5;
                    var online = random.Next(100) < 35;

                    var candidates = isNoShow ? unreliable.Where(pool.Contains).ToList() : pool;
                    var customer = candidates[random.Next(candidates.Count)];
                    if (!bookedToday.Add(customer.Id))
                    {
                        continue;
                    }

                    var services = slot.Options[random.Next(slot.Options.Length)];
                    // geçmiş randevular birkaç gün önceden, ileri tarihliler son 10 günde alınmış görünsün
                    var createdAt = isPast
                        ? startAt.AddDays(-random.Next(1, 8)).AddHours(-random.Next(0, 10))
                        : Min(now.AddHours(-random.Next(1, 240)), startAt.AddHours(-2));
                    // kaynak ve hizmet satırları her randevuya ayrı verilir: owned type nesnesi paylaşılmaz
                    Appointment appointment = new(
                        customer.Id,
                        staff.Id,
                        services.Select(s => new AppointmentSalonService(s.Id, s.Duration.Value, s.Buffer.Value, s.Price.Value)),
                        startAt,
                        online ? Source.Online : Source.Panel,
                        null,
                        createdAt);

                    if (online)
                    {
                        // birkaç gün sonraki ilk online randevu bilinen linkle açılır
                        var useDemoToken = !demoTokenUsed && !isCancelled && startAt > now.AddDays(2);
                        demoTokenUsed |= useDemoToken;
                        appointment.SetManageToken(ManageToken.FromPlainText(useDemoToken ? DemoManageToken : Guid.NewGuid().ToString("N")));
                    }

                    if (isNoShow)
                    {
                        appointment.MarkAsNoShow(Min(startAt.AddMinutes(15), now));
                        customer.IncreaseNoShowCount();
                    }
                    else if (isCancelled)
                    {
                        appointment.Cancel(
                            online ? "Müşteri randevu linkinden iptal etti" : "Müşteri arayıp iptal etti",
                            Min(Max(startAt.AddDays(-1), createdAt.AddHours(1)), now));
                    }
                    else if (isPast)
                    {
                        appointment.Complete(Min(appointment.Time.End, now));
                    }

                    // zilde son 2 günde online alınan ya da iptal edilen randevular; son 12 saattekiler okunmamış
                    var notifiedAt = isCancelled ? appointment.Histories.Last().CreatedAt : createdAt;
                    if (online && !isNoShow && notifiedAt > now.AddDays(-2))
                    {
                        Notification notification = new(
                            isCancelled ? NotificationType.AppointmentCancelled : NotificationType.AppointmentCreated,
                            appointment.Id);
                        if (notifiedAt < now.AddHours(-12))
                        {
                            notification.MarkAsRead();
                        }

                        notifications.Add(notification);
                    }

                    appointments.Add(appointment);
                }
            }
        }

        appointmentRepository.AddRange(appointments);
        notificationRepository.AddRange(notifications);
        #endregion

        await unitOfWork.SaveChangesAsync();
    }

    // salonun saat diliminde gün başından itibaren tam günler
    private static DateTimeRange DayRange(DateOnly date, int days, TimeZoneInfo timeZone)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = date.AddDays(days).ToDateTime(TimeOnly.MinValue);
        return new DateTimeRange(
            new DateTimeOffset(start, timeZone.GetUtcOffset(start)).ToUniversalTime(),
            new DateTimeOffset(end, timeZone.GetUtcOffset(end)).ToUniversalTime());
    }

    private static bool Overlaps(DateTimeRange range, DateOnly date, TimeZoneInfo timeZone)
    {
        var day = DayRange(date, 1, timeZone);
        return range.Start < day.End && range.End > day.Start;
    }

    private static DateTimeOffset Min(DateTimeOffset first, DateTimeOffset second) => first < second ? first : second;

    private static DateTimeOffset Max(DateTimeOffset first, DateTimeOffset second) => first > second ? first : second;

    // e-posta adresi için Türkçe harfler sadeleşir: "Şahin" → "sahin"
    private static string ToAscii(string text) => text
        .ToLower(new System.Globalization.CultureInfo("tr-TR"))
        .Replace('ç', 'c').Replace('ğ', 'g').Replace('ı', 'i').Replace('ö', 'o').Replace('ş', 's').Replace('ü', 'u');
}
