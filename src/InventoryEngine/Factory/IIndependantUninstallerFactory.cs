namespace InventoryEngine.Factory
{
    public interface IIndependantUninstallerFactory : IUninstallerFactory
    {
        string DisplayName { get; }

        bool IsEnabled();
    }
}