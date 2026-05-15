namespace OabPrep.Infrastructure.Email;

internal static class EmailTemplates
{
    public static string EmailConfirmation(string name, string confirmationUrl) =>
        Layout("Confirme seu e-mail", $"""
            <h2 style="color:#1a3c6e;margin:0 0 16px">Olá, {Encode(name)}!</h2>
            <p style="margin:0 0 16px;color:#444;">
                Obrigado por se cadastrar no <strong>OAB Prep</strong>. Para ativar sua conta,
                clique no botão abaixo:
            </p>
            <div style="text-align:center;margin:32px 0;">
                <a href="{confirmationUrl}"
                   style="background:#1a3c6e;color:#fff;text-decoration:none;padding:14px 32px;
                          border-radius:6px;font-size:16px;font-weight:bold;display:inline-block;">
                    Confirmar E-mail
                </a>
            </div>
            <p style="color:#888;font-size:13px;margin:0;">
                O link expira em 24 horas. Se você não criou uma conta, ignore este e-mail.
            </p>
            """);

    public static string PasswordReset(string name, string resetUrl) =>
        Layout("Redefinição de senha", $"""
            <h2 style="color:#1a3c6e;margin:0 0 16px">Olá, {Encode(name)}!</h2>
            <p style="margin:0 0 16px;color:#444;">
                Recebemos uma solicitação para redefinir a senha da sua conta no <strong>OAB Prep</strong>.
            </p>
            <div style="text-align:center;margin:32px 0;">
                <a href="{resetUrl}"
                   style="background:#c0392b;color:#fff;text-decoration:none;padding:14px 32px;
                          border-radius:6px;font-size:16px;font-weight:bold;display:inline-block;">
                    Redefinir Senha
                </a>
            </div>
            <p style="color:#888;font-size:13px;margin:0;">
                O link expira em 1 hora. Se você não solicitou a redefinição, ignore este e-mail —
                sua senha permanece a mesma.
            </p>
            """);

    public static string DataExportReady(string name, string downloadUrl, DateTime expiresAt) =>
        Layout("Exportação de dados pronta", $"""
            <h2 style="color:#1a3c6e;margin:0 0 16px">Olá, {Encode(name)}!</h2>
            <p style="margin:0 0 16px;color:#444;">
                Sua exportação de dados do <strong>OAB Prep</strong> está pronta para download,
                conforme sua solicitação via Lei Geral de Proteção de Dados (LGPD).
            </p>
            <div style="text-align:center;margin:32px 0;">
                <a href="{downloadUrl}"
                   style="background:#1a3c6e;color:#fff;text-decoration:none;padding:14px 32px;
                          border-radius:6px;font-size:16px;font-weight:bold;display:inline-block;">
                    Baixar Meus Dados
                </a>
            </div>
            <p style="color:#888;font-size:13px;margin:0;">
                O link expira em {expiresAt:dd/MM/yyyy HH:mm} UTC. Após esse prazo, os dados
                serão removidos dos nossos servidores.
            </p>
            """);

    private static string Layout(string title, string content) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
          <meta charset="UTF-8"/>
          <meta name="viewport" content="width=device-width,initial-scale=1"/>
          <title>{Encode(title)}</title>
        </head>
        <body style="margin:0;padding:0;background:#f4f6f9;font-family:Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6f9;padding:40px 0;">
            <tr><td align="center">
              <table width="600" cellpadding="0" cellspacing="0"
                     style="max-width:600px;background:#fff;border-radius:8px;
                            box-shadow:0 2px 8px rgba(0,0,0,.08);overflow:hidden;">
                <!-- Header -->
                <tr>
                  <td style="background:#1a3c6e;padding:28px 40px;text-align:center;">
                    <span style="color:#fff;font-size:24px;font-weight:bold;letter-spacing:1px;">
                      OAB Prep
                    </span>
                  </td>
                </tr>
                <!-- Content -->
                <tr>
                  <td style="padding:40px;">
                    {content}
                  </td>
                </tr>
                <!-- Footer -->
                <tr>
                  <td style="background:#f4f6f9;padding:20px 40px;text-align:center;
                             border-top:1px solid #e8ecf0;">
                    <p style="color:#aaa;font-size:12px;margin:0;">
                      Você está recebendo este e-mail porque possui uma conta no OAB Prep.<br/>
                      © {DateTime.UtcNow.Year} OAB Prep. Todos os direitos reservados.
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    private static string Encode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
