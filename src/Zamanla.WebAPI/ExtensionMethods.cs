using GenericRepository;
using TS.MediatR;
using Zamanla.Application.Auth;
using Zamanla.Application.Services;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.SalonServices.ValueObjects;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Staffs.ValueObjects;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Tenants.ValueObjects;

namespace Zamanla.WebAPI;

public static class ExtensionMethods
{
    private const string DemoSlug = "demo-kuafor";

    // projeyi ilk açan dolu bir salonla karşılaşsın; demo salon zaten varsa bir şey yapmaz
    public static async Task SeedDemoDataAsync(this WebApplication app)
    {
        using var scoped = app.Services.CreateScope();
        var srv = scoped.ServiceProvider;
        var tenantRepository = srv.GetRequiredService<ITenantRepository>();
        var salonServiceRepository = srv.GetRequiredService<ISalonServiceRepository>();
        var staffRepository = srv.GetRequiredService<IStaffRepository>();
        var customerRepository = srv.GetRequiredService<ICustomerRepository>();
        var appointmentRepository = srv.GetRequiredService<IAppointmentRepository>();
        var unitOfWork = srv.GetRequiredService<IUnitOfWork>();
        var sender = srv.GetRequiredService<ISender>();

        if (await tenantRepository.SlugExistsAsync(new Slug(DemoSlug)))
        {
            return;
        }

        #region Salon ve Sahibi
        // kayıt ekranıyla aynı yol: salon ve sahibi birlikte açılır, şifre hash'lenir
        var register = await sender.Send(new RegisterBusinessCommand(
            "Demo Kuaför", DemoSlug, "Demo", "Kullanıcı", "demo@zamanla.test", "Demo12345"));
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

        #region Hizmetler, Personel ve Müşteriler
        SalonService haircut = new(new Name("Saç kesimi"), new Category("Kesim"), new Duration(45), new Duration(10), new Price(450));
        SalonService cutAndBlow = new(new Name("Kesim + fön"), new Category("Kesim"), new Duration(60), new Duration(10), new Price(650));
        SalonService blowDry = new(new Name("Fön"), new Category("Kesim"), new Duration(30), new Duration(5), new Price(300));
        SalonService beard = new(new Name("Sakal tıraşı"), new Category("Sakal"), new Duration(20), new Duration(5), new Price(200));
        SalonService coloring = new(new Name("Saç boyama"), new Category("Renk"), new Duration(90), new Duration(15), new Price(1200));
        SalonService keratin = new(new Name("Keratin bakımı"), new Category("Bakım"), new Duration(120), new Duration(15), new Price(3000));
        salonServiceRepository.AddRange([haircut, cutAndBlow, blowDry, beard, coloring, keratin]);

        DayOfWeek[] weekdays = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday];
        Staff elif = new(
            new FirstName("Elif"),
            new LastName("Aydın"),
            new Color("peach"),
            weekdays.SelectMany(day => new[]
            {
                new WorkingHour(day, new TimeOnly(9, 0), new TimeOnly(13, 0)),
                new WorkingHour(day, new TimeOnly(14, 0), new TimeOnly(19, 0))
            }),
            [new(haircut.Id), new(cutAndBlow.Id), new(blowDry.Id), new(coloring.Id)]);
        Staff mert = new(
            new FirstName("Mert"),
            new LastName("Kaya"),
            new Color("sage"),
            weekdays.Select(day => new WorkingHour(day, new TimeOnly(10, 0), new TimeOnly(19, 0))),
            [new(haircut.Id), new(cutAndBlow.Id), new(beard.Id)]);
        // pazartesi izinli
        Staff zeynep = new(
            new FirstName("Zeynep"),
            new LastName("Arslan"),
            new Color("lavender"),
            weekdays.Skip(1).Select(day => new WorkingHour(day, new TimeOnly(10, 0), new TimeOnly(18, 0))),
            [new(blowDry.Id), new(coloring.Id), new(keratin.Id)]);
        staffRepository.AddRange([elif, mert, zeynep]);

        // numaralar uydurma
        var customers = new[]
        {
            ("Ayşe", "Yılmaz"), ("Mehmet", "Demir"), ("Zehra", "Kaya"), ("Can", "Öztürk"),
            ("Selin", "Çelik"), ("Burak", "Şahin"), ("Deniz", "Koç"), ("Ece", "Arslan")
        }
        .Select((name, index) => new Customer(
            new FirstName(name.Item1),
            new LastName(name.Item2),
            new PhoneNumber($"0555 000 00 {index + 10}"),
            null,
            null))
        .ToList();
        customerRepository.AddRange(customers);
        #endregion

        #region Randevular
        // bugünden itibaren bir haftalık takvim; bugünün geçmiş saatleri tamamlanmış sayılır
        var now = srv.GetRequiredService<TimeProvider>().GetUtcNow();
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, timeZone).DateTime);

        // kaynak bool tutulur: owned type nesnesi tek randevuya ait olmalı, her randevuya yenisi verilir
        var plan = new (Staff Staff, int Hour, int Minute, SalonService[] Services, bool Online)[]
        {
            (elif, 9, 30, new[] { haircut }, false),
            (elif, 11, 0, new[] { cutAndBlow }, true),
            (elif, 14, 30, new[] { coloring }, false),
            (elif, 17, 0, new[] { blowDry }, true),
            (mert, 10, 0, new[] { beard }, false),
            (mert, 11, 0, new[] { haircut, beard }, true),
            (mert, 15, 0, new[] { haircut }, false),
            (zeynep, 10, 30, new[] { keratin }, true),
            (zeynep, 14, 0, new[] { blowDry }, false)
        };

        List<Appointment> appointments = [];
        for (var offset = 0; offset < 7; offset++)
        {
            var date = today.AddDays(offset);
            for (var index = 0; index < plan.Length; index++)
            {
                var item = plan[index];
                // her gün biraz farklı dolsun
                if ((offset + index) % 4 == 3 || !item.Staff.WorkingHours.Any(h => h.Day == date.DayOfWeek))
                {
                    continue;
                }

                var local = date.ToDateTime(new TimeOnly(item.Hour, item.Minute));
                var startAt = new DateTimeOffset(local, timeZone.GetUtcOffset(local));
                var customer = customers[(offset * 3 + index) % customers.Count];
                Appointment appointment = new(
                    customer.Id,
                    item.Staff.Id,
                    item.Services.Select(s => new AppointmentSalonService(s.Id, s.Duration.Value, s.Buffer.Value, s.Price.Value)),
                    startAt,
                    item.Online ? Source.Online : Source.Panel,
                    null,
                    now);

                if (startAt < now)
                {
                    // bugünün ilk randevusu gelinmedi örneği
                    if (appointments.Count == 0)
                    {
                        appointment.MarkAsNoShow(now);
                        customer.IncreaseNoShowCount();
                    }
                    else
                    {
                        appointment.Complete(now);
                    }
                }

                appointments.Add(appointment);
            }
        }

        appointmentRepository.AddRange(appointments);
        #endregion

        await unitOfWork.SaveChangesAsync();
    }
}
