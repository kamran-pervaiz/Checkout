using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApi.IntegrationTests
{
    public class ApiWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .ConfigureAppConfiguration((context, conf) =>
                {
                    //if want to add Test specific configurations
                })
                .ConfigureServices(services => { });

            base.ConfigureWebHost(builder);
        }
    }
}