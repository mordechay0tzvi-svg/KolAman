namespace FilesWatcher;
class FileWatcher
{
    FileSystemWatcher _watcher = new FileSystemWatcher();
    public FileWatcher(string dirPath)
    {
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
        
        _watcher.Created += new FileSystemEventHandler(OnCreated);
    }
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        string value = $"Created: {e.FullPath}";
        Console.WriteLine(value);
        _whereCreatedFilePath = value;
    }
    public static string _whereCreatedFilePath;
    public string WhereCreated()
    {
        return _whereCreatedFilePath;
    }
}
