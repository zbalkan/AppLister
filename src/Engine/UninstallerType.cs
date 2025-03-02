namespace Engine
{
    public enum UninstallerType
    {
        Unknown = 0,

        Msiexec,

        InnoSetup,

        Steam,

        Nsis,

        InstallShield,

        SdbInst,

        WindowsFeature,

        WindowsUpdate,

        StoreApp,

        SimpleDelete,

        Chocolatey,

        Oculus,

        PowerShell
    }
}