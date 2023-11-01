namespace WingetService
{
    public class WingetPackages
    {
        public string Id { get; set; }

        public string InstalledVersion { get; set; }

        public string Name { get; set; }

        public bool IsUpdateAvailable { get; set; }

        public string Source { get; set; }

        public string[] AvailableVersions { get; set; }
    }
}