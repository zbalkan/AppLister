/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System.Collections.Generic;

namespace InventoryEngine.Factory
{
    public interface IIndependantUninstallerFactory : IUninstallerFactory
    {
        bool IsEnabled();

        string DisplayName { get; }
    }

    public interface IUninstallerFactory
    {
        IList<ApplicationUninstallerEntry> GetUninstallerEntries();
    }
}