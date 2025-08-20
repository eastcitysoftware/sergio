using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Sergio;

public static class StaticFileServer {
    public static WebApplication? Build(string websitesFile, int port = 80) {
        var wapp = BuildServer(websitesFile, port);

        var websitesOption = wapp.Services.GetRequiredService<IOptions<List<WebsiteConfig>>>();

        if (websitesOption.Value is List<WebsiteConfig> websites && websites.Count > 0) {
            wapp.ConfigureWebsites(websites);
            return wapp;
        }

        return default;
    }

    static WebApplication BuildServer(string websitesFile, int port) {
        var bldr = WebApplication.CreateEmptyBuilder(new());
        bldr.WebHost.UseKestrelCore();
        bldr.Configuration.AddJsonFile(websitesFile, optional: false);
        bldr.Services
            .Configure<KestrelServerOptions>(options => {
                options.ListenLocalhost(port);
                options.Limits.MaxRequestBodySize = 1 * 1024 * 1024; // 1 MB
            })
            .Configure<List<WebsiteConfig>>(bldr.Configuration.GetSection("Websites"));

        return bldr.Build();
    }

    static void ConfigureWebsites(this WebApplication wapp, List<WebsiteConfig> websites) {
        foreach (var websiteConfig in websites) {
            Console.WriteLine($"Configuring website: {websiteConfig.domain}");
            Console.WriteLine($"┕ Root: {websiteConfig.root}");

            wapp.MapWhen(
                ctx => string.Equals(websiteConfig.domain, ctx.Request.Host.Host, StringComparison.OrdinalIgnoreCase),
                website => {
                    var fileProvider = new PhysicalFileProvider(websiteConfig.root);
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

public sealed record WebsiteConfig(
    string domain,
    string root,
    int cacheExpirationSeconds
);
