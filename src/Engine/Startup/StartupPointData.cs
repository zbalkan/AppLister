namespace Engine.Startup
{
    internal sealed class StartupPointData
    {
        public readonly bool AllUsers;

        public readonly bool IsRegKey;

        public readonly bool IsRunOnce;

        public readonly bool IsWow;

        public readonly string Name;

        public readonly string Path;

        public StartupPointData(bool allUsers, bool isRegKey, bool isRunOnce, bool isWow, string name, string path)
        {
            AllUsers = allUsers;
            IsRegKey = isRegKey;
            IsRunOnce = isRunOnce;
            IsWow = isWow;
            Name = name;
            Path = path;
        }
    }
}