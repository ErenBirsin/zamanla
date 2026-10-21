namespace Zamanla.Application.Permissions;

public sealed record PermissionDefinition(string Value, string Label);

// personel hesabında işaretlenebilen yetkiler; [Permission("...")] değerleri buradan gelir
public static class PermissionCatalog
{
    public const string AppointmentsManageAll = "appointments:manage-all";
    public const string RevenueView = "revenue:view";
    public const string CustomersDelete = "customers:delete";
    public const string ServicesManage = "services:manage";
    public const string StaffManage = "staff:manage";
    public const string TimeOffsManage = "timeoffs:manage";
    public const string SettingsManage = "settings:manage";

    public static readonly IReadOnlyList<PermissionDefinition> All =
    [
        new(AppointmentsManageAll, "Başka personelin randevularını yönetir"),
        new(RevenueView, "Ciroyu görür"),
        new(CustomersDelete, "Müşteri siler"),
        new(ServicesManage, "Hizmetleri yönetir"),
        new(StaffManage, "Personeli ve çalışma saatlerini yönetir"),
        new(TimeOffsManage, "İzinleri yönetir"),
        new(SettingsManage, "Salon ayarlarını değiştirir")
    ];

    public static bool Exists(string value) => All.Any(p => p.Value == value);

    public static string GetLabel(string value) => All.FirstOrDefault(p => p.Value == value)?.Label ?? value;
}
