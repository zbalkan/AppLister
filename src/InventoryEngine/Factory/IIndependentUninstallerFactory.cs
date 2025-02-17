namespace InventoryEngine.Factory
{
    public interface IIndependentUninstallerFactory : IUninstallerFactory
    {
        string DisplayName { get; }

        bool IsEnabled();
    }
}