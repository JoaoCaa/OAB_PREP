namespace OabPrep.Application.UseCases.Auth.ForgotPassword;

public sealed record ForgotPasswordResponse(
    string Message = "Se o e-mail estiver cadastrado, você receberá as instruções em breve.");
