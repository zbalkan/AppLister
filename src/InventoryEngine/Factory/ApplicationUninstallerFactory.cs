using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InventoryEngine.InfoAdders;
using InventoryEngine.Startup;
using InventoryEngine.Tools;
using InventoryEngine.Extensions;

namespace InventoryEngine.Factory
{
    public static class ApplicationUninstallerFactory
    {
        private static readonly InfoAdderManager InfoAdder = new InfoAdderManager();

        public static IList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var concurrentFactory = new ConcurrentApplicationFactory(GetMiscUninstallerEntries);

            try
            {
                // Find msi products ---------------------------------------------------------------------------------------
                var msiProducts = MsiTools.MsiEnumProducts().ToList();

                // Run some factories in a separate thread -----------------------------------------------------------------
                concurrentFactory.Start();

                // Find stuff mentioned in registry ------------------------------------------------------------------------
                IList<ApplicationUninstallerEntry> registryResults;
                if (UninstallToolsGlobalConfig.ScanRegistry)
                {
                    var sw = Stopwatch.StartNew();
                    var registryFactory = new RegistryFactory(msiProducts);
                    registryResults = registryFactory.GetUninstallerEntries();
                    Trace.WriteLine($"[Performance] Factory {nameof(RegistryFactory)} took {sw.ElapsedMilliseconds}ms to finish");

                    // Fill in install llocations for DirectoryFactory to improve speed and quality
                    // of results
                    if (UninstallToolsGlobalConfig.UninstallerFactoryCache != null)
                        ApplyCache(registryResults, UninstallToolsGlobalConfig.UninstallerFactoryCache, InfoAdder);

                    FactoryThreadedHelpers.GenerateMissingInformation(registryResults, InfoAdder, null, true);
                }
                else
                {
                    registryResults = new List<ApplicationUninstallerEntry>();
                }

                // Look for entries on drives, based on info in registry.
                // ---------------------------------------------------- Will introduce duplicates to
                // already detected stuff. Need to check for duplicates with other entries later.
                IList<ApplicationUninstallerEntry> driveResults;
                if (UninstallToolsGlobalConfig.ScanDrives)
                {
                    var sw = Stopwatch.StartNew();
                    var driveFactory = new DirectoryFactory(registryResults);
                    driveResults = driveFactory.GetUninstallerEntries();
                    Trace.WriteLine($"[Performance] Factory {nameof(DirectoryFactory)} took {sw.ElapsedMilliseconds}ms to finish");
                }
                else
                {
                    driveResults = new List<ApplicationUninstallerEntry>();
                }

                // Join up with the thread ----------------------------------------------------------------------------------
                var otherResults = concurrentFactory.GetResults();

                // Handle duplicate entries ----------------------------------------------------------------------------------
                var mergedResults = registryResults.ToList();
                MergeResults(mergedResults, otherResults);
                // Make sure to merge driveResults last
                MergeResults(mergedResults, driveResults);

                // Fill in any missing information -------------------------------------------------------------------------
                if (UninstallToolsGlobalConfig.UninstallerFactoryCache != null)
                    ApplyCache(mergedResults, UninstallToolsGlobalConfig.UninstallerFactoryCache, InfoAdder);

                FactoryThreadedHelpers.GenerateMissingInformation(mergedResults, InfoAdder, msiProducts, false);

                // PersistentCache missing information to speed up future scans
                if (UninstallToolsGlobalConfig.UninstallerFactoryCache != null)
                {
                    foreach (var entry in mergedResults)
                        UninstallToolsGlobalConfig.UninstallerFactoryCache.TryCacheItem(entry);

                    try
                    {
                        UninstallToolsGlobalConfig.UninstallerFactoryCache.Save();
                    }
                    catch (SystemException e)
                    {
                        Trace.WriteLine($"Failed to save cache: {e}");
                    }
                }
                var startupEntries = new List<StartupEntryBase>();
                foreach (var factory in StartupManager.Factories)
                {
                    try
                    {
                        startupEntries.AddRange(factory.Value());
                    }
                    catch (Exception ex)
                    {

                    }
                }

                try
                {
                    AttachStartupEntries(mergedResults, startupEntries);
                }
                catch (Exception ex)
                {

                }

                return mergedResults;
            }
            finally
            {
                concurrentFactory.Dispose();
            }
        }

        /// <summary>
        ///     Merge new results into the base list
        /// </summary>
        internal static void MergeResults(ICollection<ApplicationUninstallerEntry> baseEntries,
            ICollection<ApplicationUninstallerEntry> newResults)
        {
            var newToAdd = new List<ApplicationUninstallerEntry>();
            foreach (var entry in newResults)
            {
                var matchedEntry = baseEntries.Select(x => new { x, score = ApplicationEntryTools.AreEntriesRelated(x, entry) })
                    .Where(x => x.score >= 1)
                    .OrderByDescending(x => x.score)
                    .Select(x => x.x)
                    .FirstOrDefault();

                if (matchedEntry != null)
                {
                    // Prevent setting incorrect UninstallerType
                    if (matchedEntry.UninstallPossible)
                        entry.UninstallerKind = UninstallerType.Unknown;

                    InfoAdder.CopyMissingInformation(matchedEntry, entry);
                    continue;
                }

                // If the entry failed to match to anything, add it to the base results as new
                newToAdd.Add(entry);
            }

            foreach (var newEntry in newToAdd)
            {
                baseEntries.Add(newEntry);
            }
        }

        private static void ApplyCache(ICollection<ApplicationUninstallerEntry> baseEntries, ApplicationUninstallerFactoryCache cache, InfoAdderManager infoAdder)
        {
            var hits = 0;
            foreach (var entry in baseEntries)
            {
                var matchedEntry = cache.TryGetCachedItem(entry);
                if (matchedEntry != null)
                {
                    infoAdder.CopyMissingInformation(entry, matchedEntry);
                    hits++;
                }
                else
                {
                    Debug.WriteLine("PersistentCache miss: " + entry.DisplayName);
                }
            }
            Trace.WriteLine($"PersistentCache hits: {hits}/{baseEntries.Count}");
        }

        private static List<ApplicationUninstallerEntry> GetMiscUninstallerEntries()
        {
            var otherResults = new List<ApplicationUninstallerEntry>();

            var miscFactories = ReflectionTools.GetTypesImplementingBase<IIndependantUninstallerFactory>()
                .Attempt(Activator.CreateInstance)
                .Cast<IIndependantUninstallerFactory>()
                .Where(x => x.IsEnabled())
                .ToList();

            foreach (var kvp in miscFactories)
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    MergeResults(otherResults, kvp.GetUninstallerEntries());
                    Trace.WriteLine($"[Performance] Factory {kvp.GetType().Name} took {sw.ElapsedMilliseconds}ms to finish");
                }
                catch (Exception ex)
                {
                    // catch
                }
            }

            return otherResults;
        }

        /// <summary>
        ///     Attach startup entries to uninstaller entries that are automatically detected as related.
        /// </summary>
        public static void AttachStartupEntries(IEnumerable<ApplicationUninstallerEntry> uninstallers, IEnumerable<StartupEntryBase> startupEntries) =>
            // Using DoForEach to avoid multiple enumerations
            StartupManager.AssignStartupEntries(uninstallers
                .DoForEach(x => { if (x != null) x.StartupEntries = null; }), startupEntries);
    }
}