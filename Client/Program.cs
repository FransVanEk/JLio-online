using System;
using System.Net.Http;
using System.Threading.Tasks;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLioOnline.Client.Providers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using TG.Blazor.IndexedDB;

namespace JLioOnline.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var baseAddress = builder.Configuration["BaseAddress"] ?? builder.HostEnvironment.BaseAddress;
            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseAddress) });
            builder.Services.AddScoped<IItemsFetcher>(_ => new JsonPathItemsFetcher());
            builder.Services.AddScoped<PersistenceStore>();

            AddDatabase(builder);
            AddRadzen(builder);
            await builder.Build().RunAsync();
        }

        private static void AddRadzen(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<TooltipService>();
            builder.Services.AddScoped<ContextMenuService>();
        }

        private static void AddDatabase(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddIndexedDB(dbStore =>
            {
                dbStore.DbName = "JLioOnline";
                dbStore.Version = 1;

                dbStore.Stores.Add(new StoreSchema
                {
                    Name = "JLioModels",
                    PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false },
                });
            });
        }
    }
}