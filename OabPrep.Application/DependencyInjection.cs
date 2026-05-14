using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OabPrep.Application.UseCases.Auth.ConfirmEmail;
using OabPrep.Application.UseCases.Auth.ForgotPassword;
using OabPrep.Application.UseCases.Auth.Login;
using OabPrep.Application.UseCases.Auth.Logout;
using OabPrep.Application.UseCases.Auth.Refresh;
using OabPrep.Application.UseCases.Auth.Register;
using OabPrep.Application.UseCases.Auth.ResetPassword;
using OabPrep.Application.UseCases.LawAreas.Create;
using OabPrep.Application.UseCases.LawAreas.Deactivate;
using OabPrep.Application.UseCases.LawAreas.GetLawAreaById;
using OabPrep.Application.UseCases.LawAreas.GetLawAreas;
using OabPrep.Application.UseCases.LawAreas.Update;
using OabPrep.Application.UseCases.Questions.Create;
using OabPrep.Application.UseCases.Questions.Deactivate;
using OabPrep.Application.UseCases.Questions.GetById;
using OabPrep.Application.UseCases.Questions.GetList;
using OabPrep.Application.UseCases.Questions.Update;
using OabPrep.Application.UseCases.Admin.Reports.GetQuestionStats;
using OabPrep.Application.UseCases.Admin.Reports.GetSystemSummary;
using OabPrep.Application.UseCases.Admin.Users;
using OabPrep.Application.UseCases.Performance.GetAreaPerformance;
using OabPrep.Application.UseCases.Performance.GetUserPerformance;
using OabPrep.Application.UseCases.Profile.ChangePassword;
using OabPrep.Application.UseCases.Profile.DeleteAccount;
using OabPrep.Application.UseCases.Profile.GetProfile;
using OabPrep.Application.UseCases.Profile.RequestDataExport;
using OabPrep.Application.UseCases.Profile.UpdateProfile;
using OabPrep.Application.UseCases.Profile.UploadAvatar;
using OabPrep.Application.UseCases.Sessions.CreateSession;
using OabPrep.Application.UseCases.Sessions.FinishSession;
using OabPrep.Application.UseCases.Sessions.GetSession;
using OabPrep.Application.UseCases.Sessions.SubmitAnswer;
using OabPrep.Application.UseCases.Chat.GetHistory;
using OabPrep.Application.UseCases.Chat.SendMessage;
using OabPrep.Application.UseCases.Chat.SendSessionMessage;
using OabPrep.Application.UseCases.Sessions.ToggleReviewMark;
using System.Reflection;

namespace OabPrep.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<ConfirmEmailUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<ForgotPasswordUseCase>();
        services.AddScoped<ResetPasswordUseCase>();
        services.AddScoped<RefreshTokenUseCase>();
        services.AddScoped<LogoutUseCase>();

        services.AddScoped<GetLawAreasUseCase>();
        services.AddScoped<GetLawAreaByIdUseCase>();
        services.AddScoped<CreateLawAreaUseCase>();
        services.AddScoped<UpdateLawAreaUseCase>();
        services.AddScoped<DeactivateLawAreaUseCase>();

        services.AddScoped<CreateQuestionUseCase>();
        services.AddScoped<UpdateQuestionUseCase>();
        services.AddScoped<DeactivateQuestionUseCase>();
        services.AddScoped<GetQuestionsUseCase>();
        services.AddScoped<GetQuestionByIdUseCase>();

        services.AddScoped<CreateSessionUseCase>();
        services.AddScoped<SubmitAnswerUseCase>();
        services.AddScoped<FinishSessionUseCase>();
        services.AddScoped<ToggleReviewMarkUseCase>();
        services.AddScoped<GetSessionUseCase>();
        services.AddScoped<GetUserPerformanceUseCase>();
        services.AddScoped<GetAreaPerformanceUseCase>();

        services.AddScoped<ListUsersUseCase>();
        services.AddScoped<GetAdminUserUseCase>();
        services.AddScoped<BlockUserUseCase>();
        services.AddScoped<UnblockUserUseCase>();
        services.AddScoped<ChangeUserRoleUseCase>();

        services.AddScoped<GetSystemSummaryUseCase>();
        services.AddScoped<GetQuestionStatsUseCase>();

        services.AddScoped<GetProfileUseCase>();
        services.AddScoped<UpdateProfileUseCase>();
        services.AddScoped<ChangePasswordUseCase>();
        services.AddScoped<UploadAvatarUseCase>();
        services.AddScoped<DeleteAccountUseCase>();
        services.AddScoped<RequestDataExportUseCase>();

        services.AddScoped<SendChatMessageUseCase>();
        services.AddScoped<GetChatHistoryUseCase>();
        services.AddScoped<SendSessionChatMessageUseCase>();

        return services;
    }
}
