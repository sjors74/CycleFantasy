using AutoMapper;
using Domain.Mapping;

namespace CycleManager.Tests.Helpers
{
    public static class AutoMapperTestHelper
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(DomainToResponseMappingProfile).Assembly);
            });
            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}
