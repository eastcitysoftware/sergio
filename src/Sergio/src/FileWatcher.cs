namespace Sergio;

public static class FileWatcher {
    public static void WatchFile(string filePath, Func<bool> checkContinue, Action onChange) {
        if (!File.Exists(filePath)) {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        onChange();

        Console.WriteLine($"Watching file: {filePath}");
        Console.WriteLine("Press ESC to stop...");
        var initial = GetFileStats(filePath);

        var shouldContinue = true;

        do {
            var current = GetFileStats(filePath);

            if (current != initial) {
                Console.WriteLine($"File changed: {filePath}");
                initial = current;
                onChange();
            }

            shouldContinue = checkContinue();

            Task.Delay(500).Wait();
        } while (shouldContinue);

    }

    private static FileStats GetFileStats(string filePath) {
        var fileInfo = new FileInfo(filePath);
        return new FileStats(fileInfo.Length, fileInfo.LastWriteTime);
    }

    public record struct FileStats(long Length, DateTime LastWriteTime);
}
