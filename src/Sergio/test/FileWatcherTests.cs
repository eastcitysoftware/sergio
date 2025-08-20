namespace Sergio.Tests;

public sealed class FileWatcherTests {
    [Fact]
    public void WatchFile() {
        var testFilePath = Path.Combine(Path.GetTempPath(), "testfile.txt");
        File.WriteAllText(testFilePath, "Initial content");

        var changeMax = 3;
        var changeCount = 0;
        var changeDetected = false;
        FileWatcher.WatchFile(testFilePath,
            checkContinue: () => {
                if (changeCount >= changeMax) {
                    Console.WriteLine("Max changes reached, stopping watcher.");
                    return false;
                }
                File.WriteAllText(testFilePath, Path.GetRandomFileName());
                return true;
            },
            onChange: () => {
                changeDetected = true;
                changeCount++;
            });

        Assert.True(changeDetected);
        Assert.Equal(changeMax, changeCount);

        File.Delete(testFilePath);
    }
}
