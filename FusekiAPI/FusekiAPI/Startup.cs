using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageService;
using FusekiAPI.Parser;
using WorkflowCore.Interface;
using FusekiAPI.SagaPattern.Orchestrator;
using FusekiAPI.SagaPattern.Saga;
using WorkflowCore.Models;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using FusekiAPI.SagaPattern.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace FusekiAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment webHost)
        {
            Configuration = configuration;
            Webhost = webHost;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Webhost { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                options.Authority = "http://localhost:8082";
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = false
                };
                options.RequireHttpsMetadata = false;
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ClientIdPolicy",
                    policy => policy.RequireClaim("client_id", "fusekiapiclient"));

            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FusekiAPI", Version = "v1" });
            });
            services.AddScoped<IMessageService, MessageBrokerService>();
            services.AddScoped<IInsertIntoGraph, InsertIntoGraphOrchestrator>();
            services.AddScoped<IParser, FusekiAPI.Parser.Parser>();
            services.AddHostedService<FusekiAPI.MessageService.MessageService>();
            services.AddWorkflow(x =>
            {
                x.UsePostgreSQL(BuildPostgresConnectionString(), false, true);
            });
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            Infrastructure.DependencyInjectConfig.Register(builder, Configuration);
        }
        private string BuildPostgresConnectionString()
        {
            var defaultPostGresConfig = Configuration.GetSection("PostGres");
            var server = Environment.GetEnvironmentVariable("POSTGRES_SERVER")
                         ?? defaultPostGresConfig.GetValue<string>("Server");
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT")
                       ?? defaultPostGresConfig.GetValue<string>("Port");
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
                           ?? defaultPostGresConfig.GetValue<string>("Password");
            var database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE")
                           ?? defaultPostGresConfig.GetValue<string>("Database");
            var user = Environment.GetEnvironmentVariable("POSTGRES_USERID")
                       ?? defaultPostGresConfig.GetValue<string>("UserID");
            return $"Server={server};Port={port};Database={database};User Id={user};Password={password}";
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FusekiAPI v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            var host = app.ApplicationServices.GetService<IWorkflowHost>();
            host.RegisterWorkflow<InsertIntoGraphOrchestrator, InsertIntoGraphSaga>();
            host.Start();
        }
    }
}
