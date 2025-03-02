using System.Collections.Generic;
using WmiProvider;

namespace AppLister
{
    public interface IScanner
    {
        /// <summary>
        ///     Reads all the Apps from the source.
        /// </summary>
        /// <returns>
        ///     List of Apps collected. Returns empty list of nothing found.
        /// </returns>
        List<App> GetAll();
    }
}