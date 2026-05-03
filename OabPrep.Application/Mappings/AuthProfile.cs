using AutoMapper;
using OabPrep.Application.UseCases.Auth.Register;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.Mappings;

public sealed class AuthProfile : Profile
{
    public AuthProfile()
    {
        // CreateUserInput → User: usa factory method para garantir invariantes de domínio
        CreateMap<CreateUserInput, User>()
            .ConstructUsing((src, _) => User.Create(src.Name, src.NormalizedEmail, src.PasswordHash))
            .ForAllMembers(opt => opt.Ignore());
    }
}
