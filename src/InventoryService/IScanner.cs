using System.Collections.Generic;
using InventoryWmiProvider;

namespace InventoryService
{
    internal interface IScanner
    {
        /// <summary>
        ///     Reads all the packages from the source.
        /// </summary>
        /// <returns>
        ///     List of packages collected. Returns empty list of nothing found.
        /// </returns>
        List<Package> GetAll();
    }
}