﻿using Azure.Identity;
using AzureFunctions.Extensions.Middleware.Abstractions;
using AzureFunctions.Extensions.Middleware.Infrastructure;
using CosmicChat.Shared.Middleware;
using CosmicChat.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;


[assembly: FunctionsStartup(typeof(CosmicChat.API.Startup))]
namespace CosmicChat.API
{
   /// <summary>
   /// CosmicChat API Startup
   /// </summary>
   [ExcludeFromCodeCoverage]
   public class Startup : FunctionsStartup
   {
      IConfiguration Configuration { get; set; }
      public Startup()
      {

      }
      /// <summary>
      /// Use the configure function to configure dependency injection
      /// </summary>
      /// <param name="builder"></param>
      /// <exception cref="ArgumentNullException"></exception>
      public override void Configure(IFunctionsHostBuilder builder)
      {
         if (builder == null) throw new ArgumentNullException(nameof(builder), "Functions Host builder instance not created !");

         ConfigureServices(builder.Services);
      }
      public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
      {
         FunctionsHostBuilderContext context = builder.GetContext();

         var configBuilder = new ConfigurationBuilder()
            .SetBasePath(context.ApplicationRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();

         var localSettings = configBuilder.Build();

         configBuilder
             .AddAzureAppConfiguration(action =>
             {
                action.Connect(new Uri(localSettings.GetSection("AppConfigurationUri").Value),
                      new DefaultAzureCredential((builder.GetContext().EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase) ?
                      new DefaultAzureCredentialOptions
                      {
                         VisualStudioTenantId = "d787514b-d3f2-45ff-9bf1-971fb473fc85" // Replace with your Tenant ID 
                      } : new DefaultAzureCredentialOptions())))
                         .Select("*")
                         .ConfigureRefresh(refresh =>
                         {
                            refresh.Register("CosmicChat:Version", refreshAll: true);
                            refresh.SetCacheExpiration(TimeSpan.FromSeconds(10));
                         })
                         .ConfigureKeyVault(kv =>
                         {
                            kv.SetCredential(new DefaultAzureCredential(builder.GetContext().EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
                               ? new DefaultAzureCredentialOptions
                               {
                                  VisualStudioTenantId = "d787514b-d3f2-45ff-9bf1-971fb473fc85" // Replace with your Tenant ID 
                               } : new DefaultAzureCredentialOptions()));
                         });
             });

         Configuration = configBuilder.Build();
      }
      /// <summary>
      /// Use the CofigureServices function to inject all required services to servicecollection
      /// </summary>
      /// <param name="services"></param>
      private void ConfigureServices(IServiceCollection services)
      {
         services.AddSingleton(Configuration);
         services.AddAzureAppConfiguration();
         services.AddApplicationInsightsTelemetry();
         services.AddHttpContextAccessor();
         services.AddOptions();


         services.Configure<AzureMapConfiguration>(Configuration.GetSection("AzMap"));
         services.Configure<WebPubSubOptions>(Configuration.GetSection("WebPubSub"));

         #region Middleware 
         services.AddTransient<IHttpMiddlewareBuilder, HttpMiddlewareBuilder>((sp) =>
         {
            var funcBuilder = new HttpMiddlewareBuilder(sp.GetRequiredService<IHttpContextAccessor>());

            funcBuilder.Use(new ExceptionMiddleware(new LoggerFactory().CreateLogger(nameof(ExceptionMiddleware))));
            funcBuilder.Use(new ClaimsCheckMiddleware(new LoggerFactory().CreateLogger(nameof(ClaimsCheckMiddleware))));
            funcBuilder.Use(new AzureAppConfigurationRefreshMiddleware(new LoggerFactory().CreateLogger(nameof(ExceptionMiddleware)), 
               sp.GetRequiredService<IConfigurationRefresherProvider>()));

            return funcBuilder;
         });
         #endregion

      }
   }
}
