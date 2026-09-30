using AutoMapper;
using Domain.Mapping;
using Microsoft.Extensions.Logging;

namespace CycleManager.Tests.Helpers
{
    public static class AutoMapperTestHelper
    {
        public static IMapper CreateMapper()
        {
            using var loggerFactory = LoggerFactory.Create(_ => { });

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DomainToResponseMappingProfile>();
            }, loggerFactory);

            try
            {
                config.AssertConfigurationIsValid();
            }
            catch(AutoMapperConfigurationException ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            return config.CreateMapper();
        }
    }
}
