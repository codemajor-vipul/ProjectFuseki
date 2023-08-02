using Autofac;
using FusekiAPI.SagaPattern.Steps;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FusekiAPI.Infrastructure
{
    public static class DependencyInjectConfig
    {
        public static void Register(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterInstance(configuration);
            builder.RegisterType<InsertIntoGraphStep>().InstancePerDependency();
        }
    }
}
