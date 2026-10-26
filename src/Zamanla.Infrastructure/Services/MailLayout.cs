using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Zamanla.Infrastructure.Services;

// bütün e-postaların ortak HTML kalıbı; e-posta programları <style> etiketini çoğu zaman sildiği için stiller satır içinde
internal static class MailLayout
{
    // Türkçe harfler olduğu gibi kalır; sadece < > & " ' gibi HTML'i bozabilecek karakterler kaçırılır
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);
    public static string Render(string title, string content) =>
        $"""
        <!DOCTYPE html>
        <html lang="tr">
        <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>{Encode(title)}</title>
        </head>
        <body style="margin:0;padding:0;background-color:#f6f1ea;">
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f6f1ea;">
        <tr><td align="center" style="padding:32px 16px;">
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:520px;">
        <tr><td style="padding:0 4px 20px;font-family:Arial,Helvetica,sans-serif;">
        <span style="display:inline-block;width:32px;height:32px;line-height:32px;border-radius:9px;background-color:#b8492f;color:#ffffff;text-align:center;font-size:17px;font-weight:bold;vertical-align:middle;">z</span>
        <span style="padding-left:8px;font-size:21px;font-weight:bold;color:#1e1b18;vertical-align:middle;">zamanla</span>
        </td></tr>
        <tr><td style="background-color:#ffffff;border:1px solid #e6dccf;border-radius:18px;padding:32px 28px;font-family:Arial,Helvetica,sans-serif;color:#1e1b18;">
        <h1 style="margin:0 0 20px;font-size:22px;line-height:28px;color:#1e1b18;">{Encode(title)}</h1>
        {content}
        </td></tr>
        <tr><td style="padding:20px 4px 0;font-family:Arial,Helvetica,sans-serif;font-size:12px;line-height:18px;color:#6b6259;">
        Bu e-posta Zamanla tarafından otomatik gönderildi; yanıtlamanıza gerek yok.
        </td></tr>
        </table>
        </td></tr>
        </table>
        </body>
        </html>
        """;

    public static string Paragraph(string text) =>
        $"""<p style="margin:0 0 16px;font-size:15px;line-height:23px;color:#4a433c;">{Encode(text)}</p>""";

    public static string Button(string label, string url) =>
        $"""
        <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:8px 0 20px;">
        <tr><td style="border-radius:999px;background-color:#b8492f;">
        <a href="{Encode(url)}" target="_blank" style="display:inline-block;padding:13px 26px;font-size:15px;font-weight:bold;color:#ffffff;text-decoration:none;border-radius:999px;">{Encode(label)}</a>
        </td></tr>
        </table>
        """;

    // düğme çalışmazsa link kopyalanabilsin
    public static string LinkFallback(string url) =>
        $"""<p style="margin:0 0 20px;font-size:12px;line-height:18px;color:#6b6259;">Düğme çalışmazsa bu linki tarayıcınıza yapıştırın:<br><a href="{Encode(url)}" target="_blank" style="color:#8e3b22;word-break:break-all;">{Encode(url)}</a></p>""";

    // randevu bilgileri: solda başlık, sağda değer
    public static string Details(IEnumerable<(string Label, string Value)> rows)
    {
        var html = new StringBuilder();
        html.Append("""<table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:4px 0 20px;background-color:#fbf8f3;border:1px solid #f0e8dd;border-radius:12px;">""");
        foreach (var (label, value) in rows)
        {
            html.Append($"""<tr><td style="padding:10px 16px;width:96px;font-size:13px;line-height:20px;color:#6b6259;vertical-align:top;">{Encode(label)}</td><td style="padding:10px 16px 10px 0;font-size:14px;line-height:20px;font-weight:bold;color:#1e1b18;">{Encode(value)}</td></tr>""");
        }
        html.Append("</table>");
        return html.ToString();
    }

    public static string Note(string text) =>
        $"""<p style="margin:0;font-size:13px;line-height:20px;color:#6b6259;">{Encode(text)}</p>""";

    // müşteri ve salon adları kullanıcıdan geliyor; < > & gibi karakterler HTML olarak çalışmasın
    private static string Encode(string text) => Encoder.Encode(text);
}

