using AutoMapper;
using Domain.Mapping;
using Microsoft.Extensions.Logging;
using System;

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

            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}
