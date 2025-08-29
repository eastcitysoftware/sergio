using System.CommandLine;
using Sergio;

var command =
    new RootCommand(
        description: "sergio, static web server with hot reload");

var inputArgument =
    new Argument<string>(
        name: "input") {
        Description = "The path to a website directory or sergio.json file"
    };

var portOption =
    new Option<int>("port", ["--port", "-p"]) {
        Description = "The port to listen on",
        DefaultValueFactory = _ => 8080
    };

var enableCompressionOption =
    new Option<bool>("disable-compression", ["--disable-compression", "-ec"]) {
        Description = "Disable response compression",
        DefaultValueFactory = _ => false
    };

var verboseLoggingOption =
    new Option<bool>("verbose", ["--verbose"]) {
        Description = "Enable verbose logging",
        DefaultValueFactory = _ => false
    };

command.Arguments.Add(inputArgument);
command.Options.Add(portOption);
command.Options.Add(enableCompressionOption);
command.Options.Add(verboseLoggingOption);

command.SetAction(ExecuteCommand);
command.Parse(args).Invoke();

void ExecuteCommand(ParseResult parseResult) {
    var input = parseResult.GetValue(inputArgument) ?? ".";

    // test if the input is a relative path and convert to absolute
    if (!Path.IsPathRooted(input)) {
        input = Path.GetFullPath(input);
    }

    var config = new StaticFileServerConfig(
        Port: parseResult.GetValue(portOption),
        DisableCompression: parseResult.GetValue(enableCompressionOption),
        VerboseLogging: parseResult.GetValue(verboseLoggingOption)
    );

    var executionMode = input switch {
        _ when File.Exists(input) && Path.GetExtension(input).Equals(".json", StringComparison.OrdinalIgnoreCase)
            => ExecutionMode.MultipleWebsites,
        _ when Directory.Exists(input)
            => ExecutionMode.SingleWebsite,
        _ =>
            throw new ArgumentException($"Input must be a valid directory or a .json configuration file: {input}")
    };

    switch (executionMode) {
        case ExecutionMode.SingleWebsite:
            RunSingleWebsite(config, input);
            break;
        case ExecutionMode.MultipleWebsites:
            RunMultipleWebsites(config, input);
            break;
        default:
            throw new ArgumentException("Invalid execution mode");
    }
}

void RunSingleWebsite(StaticFileServerConfig serverConfig, string websiteDirectory) {
    if (!Directory.Exists(websiteDirectory)) {
        Console.WriteLine($"Website directory not found: {websiteDirectory}");
        return;
    }

    var websiteConfig = new WebsiteConfig(
        Domain: "localhost",
        Root: websiteDirectory,
        CacheExpirationSeconds: 3600 // 1 hour
    );

    if (StaticFileServer.Build(serverConfig, [websiteConfig]) is WebApplication wapp) {
        wapp.Run();
    }
    else {
        Console.WriteLine("No websites configured, exiting.");
    }
}

void RunMultipleWebsites(StaticFileServerConfig serverConfig, string websiteConfigFile) {
    if (!File.Exists(websiteConfigFile)) {
        Console.WriteLine($"Configuration file not found: {websiteConfigFile}");
        return;
    }

    var websites = Website.LoadFromJson(websiteConfigFile);

    if (websites is null || websites.Count == 0) {
        Console.WriteLine("No websites configured in the JSON file, exiting.");
        return;
    }

    WebApplication? wapp = default;
    var ct = new CancellationTokenSource();

    FileWatcher.WatchFile(websiteConfigFile,
        checkContinue: () => {
            if (Console.KeyAvailable
                && Console.ReadKey(true).Key == ConsoleKey.Escape) {
                Console.WriteLine("Stopping file watcher...");
                return false;
            }
            return true;
        },
        onChange: () => {
            wapp?.StopAsync().Wait();
            wapp = StaticFileServer.Build(serverConfig, websites);

            if (wapp is not null) {
                // run in a task to avoid blocking the main thread
                Task.Run(() => wapp.RunAsync(ct.Token), cancellationToken: ct.Token);
            }
            else {
                Console.WriteLine("No websites configured, exiting.");

            }
        });

    ct.Cancel();
    wapp?.StopAsync().Wait();
}

enum ExecutionMode {
    SingleWebsite,
    MultipleWebsites
}
