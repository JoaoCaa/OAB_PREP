namespace OabPrep.Infrastructure.Email;

internal static class EmailTemplates
{
    public static string EmailConfirmation(string name, string confirmationUrl, string unsubscribeUrl) =>
        Layout("Confirme seu e-mail", unsubscribeUrl, $"""
            <h2 style="color:#1a3c6e;font-family:Arial,sans-serif;font-size:22px;font-weight:bold;margin:0 0 16px;padding:0;">Olá, {Encode(name)}!</h2>
            <p style="color:#444;font-family:Arial,sans-serif;font-size:15px;line-height:24px;margin:0 0 16px;padding:0;">
                Obrigado por se cadastrar no <strong>OAB Prep</strong>. Para ativar sua conta,
                clique no botão abaixo:
            </p>
            <!--[if mso]>
            <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word"
              href="{confirmationUrl}" style="height:50px;v-text-anchor:middle;width:220px;" arcsize="12%"
              fillcolor="#1a3c6e" stroke="f">
              <w:anchorlock/>
              <center style="color:#fff;font-family:Arial,sans-serif;font-size:16px;font-weight:bold;">Confirmar E-mail</center>
            </v:roundrect>
            <![endif]-->
            <!--[if !mso]><!-->
            <table width="100%" cellpadding="0" cellspacing="0" role="presentation"><tr>
              <td align="center" style="padding:24px 0;">
                <a href="{confirmationUrl}"
                   style="background:#1a3c6e;border-radius:6px;color:#fff;display:inline-block;
                          font-family:Arial,sans-serif;font-size:16px;font-weight:bold;
                          padding:14px 36px;text-decoration:none;-webkit-text-size-adjust:none;">
                   Confirmar E-mail
                </a>
              </td>
            </tr></table>
            <!--<![endif]-->
            <p style="color:#888;font-family:Arial,sans-serif;font-size:13px;line-height:20px;margin:0;">
                O link expira em <strong>24 horas</strong>. Se você não criou uma conta no OAB Prep,
                ignore este e-mail com segurança — nenhuma ação é necessária.
            </p>
            """);

    public static string PasswordReset(string name, string resetUrl, string unsubscribeUrl) =>
        Layout("Redefinição de senha", unsubscribeUrl, $"""
            <h2 style="color:#1a3c6e;font-family:Arial,sans-serif;font-size:22px;font-weight:bold;margin:0 0 16px;padding:0;">Olá, {Encode(name)}!</h2>
            <p style="color:#444;font-family:Arial,sans-serif;font-size:15px;line-height:24px;margin:0 0 16px;padding:0;">
                Recebemos uma solicitação para redefinir a senha da sua conta no <strong>OAB Prep</strong>.
                Se foi você, clique no botão abaixo:
            </p>
            <!--[if mso]>
            <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word"
              href="{resetUrl}" style="height:50px;v-text-anchor:middle;width:220px;" arcsize="12%"
              fillcolor="#c0392b" stroke="f">
              <w:anchorlock/>
              <center style="color:#fff;font-family:Arial,sans-serif;font-size:16px;font-weight:bold;">Redefinir Senha</center>
            </v:roundrect>
            <![endif]-->
            <!--[if !mso]><!-->
            <table width="100%" cellpadding="0" cellspacing="0" role="presentation"><tr>
              <td align="center" style="padding:24px 0;">
                <a href="{resetUrl}"
                   style="background:#c0392b;border-radius:6px;color:#fff;display:inline-block;
                          font-family:Arial,sans-serif;font-size:16px;font-weight:bold;
                          padding:14px 36px;text-decoration:none;-webkit-text-size-adjust:none;">
                   Redefinir Senha
                </a>
              </td>
            </tr></table>
            <!--<![endif]-->
            <p style="color:#888;font-family:Arial,sans-serif;font-size:13px;line-height:20px;margin:0;">
                O link expira em <strong>1 hora</strong>. Se você não solicitou a redefinição,
                ignore este e-mail — sua senha permanece a mesma e nenhuma alteração foi feita.
            </p>
            """);

    public static string DataExportReady(string name, string downloadUrl, DateTime expiresAt, string unsubscribeUrl) =>
        Layout("Exportação de dados pronta", unsubscribeUrl, $"""
            <h2 style="color:#1a3c6e;font-family:Arial,sans-serif;font-size:22px;font-weight:bold;margin:0 0 16px;padding:0;">Olá, {Encode(name)}!</h2>
            <p style="color:#444;font-family:Arial,sans-serif;font-size:15px;line-height:24px;margin:0 0 16px;padding:0;">
                Sua exportação de dados do <strong>OAB Prep</strong> está pronta para download,
                conforme sua solicitação exercida por meio da
                <strong>Lei Geral de Proteção de Dados (LGPD)</strong>.
            </p>
            <!--[if mso]>
            <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word"
              href="{downloadUrl}" style="height:50px;v-text-anchor:middle;width:220px;" arcsize="12%"
              fillcolor="#1a3c6e" stroke="f">
              <w:anchorlock/>
              <center style="color:#fff;font-family:Arial,sans-serif;font-size:16px;font-weight:bold;">Baixar Meus Dados</center>
            </v:roundrect>
            <![endif]-->
            <!--[if !mso]><!-->
            <table width="100%" cellpadding="0" cellspacing="0" role="presentation"><tr>
              <td align="center" style="padding:24px 0;">
                <a href="{downloadUrl}"
                   style="background:#1a3c6e;border-radius:6px;color:#fff;display:inline-block;
                          font-family:Arial,sans-serif;font-size:16px;font-weight:bold;
                          padding:14px 36px;text-decoration:none;-webkit-text-size-adjust:none;">
                   Baixar Meus Dados
                </a>
              </td>
            </tr></table>
            <!--<![endif]-->
            <p style="color:#888;font-family:Arial,sans-serif;font-size:13px;line-height:20px;margin:0;">
                O link expira em <strong>{expiresAt:dd/MM/yyyy} às {expiresAt:HH:mm} UTC</strong>.
                Após esse prazo, os dados serão removidos dos nossos servidores de forma permanente.
            </p>
            """);

    public static string AccountBlocked(string name, string supportUrl, string unsubscribeUrl) =>
        Layout("Conta suspensa", unsubscribeUrl, $"""
            <h2 style="color:#c0392b;font-family:Arial,sans-serif;font-size:22px;font-weight:bold;margin:0 0 16px;padding:0;">Conta suspensa</h2>
            <p style="color:#444;font-family:Arial,sans-serif;font-size:15px;line-height:24px;margin:0 0 16px;padding:0;">
                Olá, <strong>{Encode(name)}</strong>. Sua conta no <strong>OAB Prep</strong> foi suspensa
                por violação dos nossos Termos de Uso.
            </p>
            <p style="color:#444;font-family:Arial,sans-serif;font-size:15px;line-height:24px;margin:0 0 24px;padding:0;">
                Se você acredita que isso foi um equívoco ou deseja solicitar revisão,
                entre em contato com nossa equipe de suporte:
            </p>
            <!--[if mso]>
            <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word"
              href="{supportUrl}" style="height:50px;v-text-anchor:middle;width:220px;" arcsize="12%"
              fillcolor="#6c757d" stroke="f">
              <w:anchorlock/>
              <center style="color:#fff;font-family:Arial,sans-serif;font-size:16px;font-weight:bold;">Contatar Suporte</center>
            </v:roundrect>
            <![endif]-->
            <!--[if !mso]><!-->
            <table width="100%" cellpadding="0" cellspacing="0" role="presentation"><tr>
              <td align="center" style="padding:8px 0 24px;">
                <a href="{supportUrl}"
                   style="background:#6c757d;border-radius:6px;color:#fff;display:inline-block;
                          font-family:Arial,sans-serif;font-size:16px;font-weight:bold;
                          padding:14px 36px;text-decoration:none;-webkit-text-size-adjust:none;">
                   Contatar Suporte
                </a>
              </td>
            </tr></table>
            <!--<![endif]-->
            <p style="color:#888;font-family:Arial,sans-serif;font-size:13px;line-height:20px;margin:0;">
                Se não reconhece esta notificação, entre em contato imediatamente com
                <a href="mailto:suporte@oabprep.app" style="color:#1a3c6e;">suporte@oabprep.app</a>.
            </p>
            """);

    private const string ResponsiveCss = """
        @media only screen and (max-width:600px) {
          .email-container { width:100%!important;max-width:100%!important; }
          .fluid { width:100%!important;max-width:100%!important; }
          .mobile-padding { padding:20px!important; }
          .button-td { padding:16px 0!important; }
        }
        """;

    private static string Layout(string title, string unsubscribeUrl, string content) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR" xmlns="http://www.w3.org/1999/xhtml"
              xmlns:v="urn:schemas-microsoft-com:vml"
              xmlns:o="urn:schemas-microsoft-com:office:office">
        <head>
          <meta charset="UTF-8"/>
          <meta http-equiv="X-UA-Compatible" content="IE=edge"/>
          <meta name="viewport" content="width=device-width,initial-scale=1"/>
          <meta name="format-detection" content="telephone=no,date=no,address=no,email=no"/>
          <title>{Encode(title)}</title>
          <!--[if mso]>
          <noscript><xml><o:OfficeDocumentSettings>
            <o:PixelsPerInch>96</o:PixelsPerInch>
          </o:OfficeDocumentSettings></xml></noscript>
          <![endif]-->
          <style>
            {ResponsiveCss}
          </style>
        </head>
        <body style="margin:0;padding:0;background:#f4f6f9;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;">
          <!--[if mso | IE]>
          <table role="presentation" width="100%" style="background:#f4f6f9;"><tr><td>
          <![endif]-->
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0"
                 style="background:#f4f6f9;margin:0;padding:40px 0;">
            <tr><td align="center" style="padding:0;">
              <!-- Wrapper -->
              <table class="email-container" role="presentation" cellpadding="0" cellspacing="0"
                     style="max-width:600px;width:100%;">
                <!-- Header -->
                <tr>
                  <td style="background:#1a3c6e;border-radius:8px 8px 0 0;padding:28px 40px;text-align:center;">
                    <span style="color:#fff;display:block;font-family:Arial,sans-serif;
                                 font-size:26px;font-weight:bold;letter-spacing:2px;">
                      OAB Prep
                    </span>
                    <span style="color:#a8c4e8;display:block;font-family:Arial,sans-serif;
                                 font-size:12px;letter-spacing:1px;margin-top:4px;">
                      Preparação para o Exame da OAB
                    </span>
                  </td>
                </tr>
                <!-- Content -->
                <tr>
                  <td class="mobile-padding"
                      style="background:#fff;padding:40px;border-left:1px solid #e8ecf0;border-right:1px solid #e8ecf0;">
                    {content}
                  </td>
                </tr>
                <!-- Footer -->
                <tr>
                  <td style="background:#f4f6f9;border:1px solid #e8ecf0;border-radius:0 0 8px 8px;
                             padding:20px 40px;text-align:center;">
                    <p style="color:#aaa;font-family:Arial,sans-serif;font-size:12px;line-height:18px;margin:0 0 8px;">
                      Você está recebendo este e-mail porque possui uma conta no OAB Prep.<br/>
                      © {DateTime.UtcNow.Year} OAB Prep. Todos os direitos reservados.
                    </p>
                    <p style="color:#ccc;font-family:Arial,sans-serif;font-size:11px;margin:0;">
                      <a href="{unsubscribeUrl}"
                         style="color:#aaa;text-decoration:underline;">
                        Cancelar recebimento de e-mails
                      </a>
                      &nbsp;·&nbsp;
                      <a href="https://oabprep.app/privacidade"
                         style="color:#aaa;text-decoration:underline;">
                        Política de Privacidade
                      </a>
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
          <!--[if mso | IE]></td></tr></table><![endif]-->
        </body>
        </html>
        """;

    private static string Encode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
