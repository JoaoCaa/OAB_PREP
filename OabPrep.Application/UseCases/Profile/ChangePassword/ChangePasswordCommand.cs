namespace OabPrep.Application.UseCases.Profile.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword);
