namespace Sergio;

public class Program {
    public static int Main(string[] args) {
        var root =
            args.Length > 0
                ? args[0]
                : Directory.GetCurrentDirectory();

        var websitesFile = Path.Combine(root, "sergio.json");

        if (!File.Exists(websitesFile)) {
            Console.WriteLine($"Configuration file not found: {websitesFile}");
            return -1;
        }

        WebApplication? wapp = default;
        var ct = new CancellationTokenSource();

        FileWatcher.WatchFile(websitesFile,
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
                wapp = StaticFileServer.Build(websitesFile);

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
        return 0;
    }
}
