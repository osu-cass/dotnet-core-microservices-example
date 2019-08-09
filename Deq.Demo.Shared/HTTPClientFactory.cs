using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Deq.Demo.Shared
{
    public class ConfigureHTTPClientFactory
    {
        private readonly IHttpClientFactory _clientFactory;

        public ConfigureHTTPClientFactory(IServiceCollection services, (string, string, string) urls)
        {
            services.AddHttpClient("contacts", c =>
            {
                c.BaseAddress = new Uri(urls.Item1);
            });

            services.AddHttpClient("departments", c =>
            {
                c.BaseAddress = new Uri(urls.Item2);
            });

            services.AddHttpClient("portal", c =>
            {
                c.BaseAddress = new Uri(urls.Item3);
            });
        }
    }
}
