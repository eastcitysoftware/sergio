using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;

namespace Sergio;

public sealed record StaticFileServerConfig(
    int Port = 8080,
    bool DisableCompression = false,
    bool VerboseLogging = false
);

public static class StaticFileServer {
    public static WebApplication? Build(StaticFileServerConfig config, List<WebsiteConfig> websites) {
        var wapp = BuildServer(config);

        if (websites.Count > 0) {
            wapp.ConfigureWebsites(config, websites);
            return wapp;
        }

        return default;
    }

    static WebApplication BuildServer(StaticFileServerConfig config) {
        var bldr = WebApplication.CreateEmptyBuilder(new());
        bldr.WebHost.UseKestrelCore();
        bldr.Logging.AddConsole();

        if (config.VerboseLogging) {
            bldr.Logging.SetMinimumLevel(LogLevel.Information);
        }
        else {
            bldr.Logging.SetMinimumLevel(LogLevel.Critical);
        }

        if (!config.DisableCompression) {
            bldr.Services.AddResponseCompression();
        }

        bldr.Services
            .Configure<KestrelServerOptions>(options => {
                options.ListenLocalhost(config.Port);
                options.Limits.MaxRequestBodySize = 1 * 1024 * 1024; // 1 MB
            });

        var wapp = bldr.Build();

        if (!config.DisableCompression) {
            wapp.UseResponseCompression();
        }

        return wapp;
    }

    static void ConfigureWebsites(this WebApplication wapp, StaticFileServerConfig config, List<WebsiteConfig> websites) {
        foreach (var websiteConfig in websites) {
            Console.WriteLine($"Configuring website: {websiteConfig.Domain}:{config.Port}");
            Console.WriteLine($"  └ Root: {websiteConfig.Root}");

            wapp.MapWhen(
                ctx => string.Equals(websiteConfig.Domain, ctx.Request.Host.Host, StringComparison.OrdinalIgnoreCase),
                website => {
                    var fileProvider = new PhysicalFileProvider(websiteConfig.Root);
                    website
                        .UseDefaultFiles(new DefaultFilesOptions {
                            FileProvider = fileProvider,
                            RequestPath = ""
                        })
                        .UseStaticFiles(new StaticFileOptions {
                            FileProvider = fileProvider,
                            RequestPath = ""
                        });
                });
        }
    }
}
