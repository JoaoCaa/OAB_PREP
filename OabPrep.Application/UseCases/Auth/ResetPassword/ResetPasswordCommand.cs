namespace OabPrep.Application.UseCases.Auth.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword);
