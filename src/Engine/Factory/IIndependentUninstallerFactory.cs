namespace Engine.Factory
{
    public interface IIndependentUninstallerFactory : IUninstallerFactory
    {
        string DisplayName { get; }

        bool IsEnabled();
    }
}