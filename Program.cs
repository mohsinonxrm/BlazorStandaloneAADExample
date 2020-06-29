using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorStandaloneAADExample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Get configuration data about the Web API set in wwwroot/appsettings.json
            var CDSWebApiConfig = builder.Configuration.GetSection("CDSWebAPI");
            var resourceUrl = CDSWebApiConfig.GetSection("ResourceUrl").Value;
            var version = CDSWebApiConfig.GetSection("Version").Value;
            var timeoutSeconds = int.Parse(CDSWebApiConfig.GetSection("TimeoutSeconds").Value);

            // Create an named definition of an HttpClient that can be created in a component page
            builder.Services.AddHttpClient("GDSClient", client =>
            {
                client.BaseAddress = new Uri($"https://globaldisco.crm.dynamics.com/");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            // Create an named definition of an HttpClient that can be created in a component page
            builder.Services.AddHttpClient("CDSClient", client =>
            {
                // See https://docs.microsoft.com/powerapps/developer/common-data-service/webapi/compose-http-requests-handle-errors
                //client.BaseAddress = new Uri($"{resourceUrl}/api/data/{version}/");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            });

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

                // Add access to Common Data Service to the scope of the access token when the user signs in
                options.ProviderOptions.DefaultAccessTokenScopes.Add($"{resourceUrl}/user_impersonation");
                options.ProviderOptions.AdditionalScopesToConsent.Add($"https://globaldisco.crm.dynamics.com/user_impersonation");
            });


            await builder.Build().RunAsync();
        }
    }
}
