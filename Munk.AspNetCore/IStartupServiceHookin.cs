using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Munk.AspNetCore
{
    public interface IStartupServiceHookIn
    {
        void Configure(IApplicationBuilder app, IWebHostEnvironment env);

        void ConfigureEnvironment(IWebHostEnvironment env);

        void ConfigureService(IServiceCollection services, IConfiguration configuration);

        void ConfigureServiceOverride(IServiceCollection services, IConfiguration configuration);

        bool IsServiceFabricNodeZero(IServiceProvider serviceProvider, IWebHostEnvironment env);
    }
}
