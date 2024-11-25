using AutoMapper;
using Product.Dal.Entities;
using Product.Web.Extensions;
using Product.Web.Models.Client;
using Product.Web.Models.Proposal;
using Product.Web.Models.Template;

namespace Product.Web.AutoMapper.Config;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        // --------------------
        // Client mappings

        CreateMap<ClientDraft, Client>()
           .IgnoreAllNonExisting().ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<Client, ClientDraft>()
           .IgnoreAllNonExisting().ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<ClientHistory, Client>()
          .IgnoreAllNonExisting()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ClientId));
        CreateMap<Client, ClientViewModel>()
            .IgnoreAllNonExisting()
            .ForMember(dest => dest.DiffClient, opt => opt.MapFrom(src => new Client { Name = string.Empty, BasicInformation = string.Empty }))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<ClientDefinition, ClientViewModel>()
            .IgnoreAllNonExisting()
            .ForMember(dest => dest.DiffClient, opt => opt.MapFrom(src => new Client { Name = string.Empty, BasicInformation = string.Empty }));

        CreateMap<ClientHistory, ClientViewModel>()
            .IgnoreAllNonExisting()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ClientId))
            .ForMember(dest => dest.DiffClient, opt => opt.MapFrom(src => new Client { Name = string.Empty, BasicInformation = string.Empty })); // Empty diff profile  

        CreateMap<ClientDraft, ClientViewModel>()
         .IgnoreAllNonExisting()
         .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ClientId))
         .ForMember(dest => dest.DiffClient, opt => opt.MapFrom(src => new Client { Name = string.Empty, BasicInformation = string.Empty }));

        // --------------------
        // Proposal mappings

        CreateMap<Proposal, ProposalViewModel>()
            .IgnoreAllNonExisting();

        // --------------------
        // Template mappings

        CreateMap<Template, TemplateViewModel>()
            .IgnoreAllNonExisting();
    }
}
