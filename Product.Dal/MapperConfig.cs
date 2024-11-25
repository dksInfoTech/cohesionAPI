using AutoMapper;
using Product.Dal.Entities;

namespace Product.Dal;

public static class MapperConfig
{
    public static MapperConfiguration Config { get; private set; }

    static MapperConfig()
    {
        Config = new MapperConfiguration(cfg =>
        {
            // Profile to history
            cfg.CreateMap<ClientDefinition, ClientHistory>();

            // Profile to draft and reverse
            cfg.CreateMap<ClientDefinition, ClientDraft>();
            cfg.CreateMap<ClientDraft, ClientDefinition>();
        });
    }
}
