namespace FilesWatcher;
class FileWatcher
{
    FileSystemWatcher _watcher = new FileSystemWatcher();
    public FileWatcher(string dirPath)
    {
        _watcher.InternalBufferSize = 65536;
        _watcher.Path = dirPath;
        _watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

        _watcher.Created += OnCreated;
        _watcher.Filter = "*.readey";

        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;

        Console.WriteLine("Press enter to exit.");
        Console.ReadLine();
    }
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        string value = $"Created: {e.FullPath}";
        Console.WriteLine(value);
        _whereCreatedFilePath = e.FullPath;
    }
    static string? _whereCreatedFilePath;
    public string WhereCreated()
    {
        return _whereCreatedFilePath ?? "";
    }
}