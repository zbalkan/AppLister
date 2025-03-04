﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Extensions;
using Engine.InfoAdders;
using Engine.Shared;
using Engine.Startup;
using Engine.Tools;

namespace Engine.Factory
{
    public static class ApplicationUninstallerFactory
    {
        private static readonly InfoAdderManager InfoAdder = new InfoAdderManager();

        /// <summary>
        ///     Attach startup entries to uninstaller entries that are automatically detected as related.
        /// </summary>
        public static void AttachStartupEntries(IEnumerable<ApplicationUninstallerEntry> uninstallers, IEnumerable<StartupEntryBase> startupEntries) =>

            // Using DoForEach to avoid multiple enumerations
            StartupManager.AssignStartupEntries(uninstallers
                .DoForEach(x => { if (x != null) { x.StartupEntries = null; } }), startupEntries);

        public static IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var concurrentFactory = new ConcurrentApplicationFactory(GetMiscUninstallerEntries);

            try
            {
                // Find msi products ---------------------------------------------------------------------------------------
                var msiProducts = MsiTools.MsiEnumProducts().ToList();

                // Run some factories in a separate thread -----------------------------------------------------------------
                concurrentFactory.Start();

                // Find stuff mentioned in registry ------------------------------------------------------------------------
                IReadOnlyList<ApplicationUninstallerEntry> registryResults;
                if (UninstallToolsGlobalConfig.ScanRegistry)
                {
                    var sw = Stopwatch.StartNew();
                    var registryFactory = new RegistryFactory(msiProducts);
                    registryResults = registryFactory.GetUninstallerEntries();
                    Debug.WriteLine($"[Performance] Factory {nameof(RegistryFactory)} took {sw.ElapsedMilliseconds}ms to finish");

                    FactoryThreadedHelpers.GenerateMissingInformation(registryResults, InfoAdder, null, true);
                }
                else
                {
                    registryResults = new List<ApplicationUninstallerEntry>().AsReadOnly();
                }

                // Look for entries on drives, based on info in registry.
                // ---------------------------------------------------- Will introduce duplicates to
                // already detected stuff. Need to check for duplicates with other entries later.
                IReadOnlyList<ApplicationUninstallerEntry> driveResults;
                if (UninstallToolsGlobalConfig.ScanDrives)
                {
                    var sw = Stopwatch.StartNew();
                    var driveFactory = new DirectoryFactory(registryResults);
                    driveResults = driveFactory.GetUninstallerEntries();
                    Debug.WriteLine($"[Performance] Factory {nameof(DirectoryFactory)} took {sw.ElapsedMilliseconds}ms to finish");
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

                FactoryThreadedHelpers.GenerateMissingInformation(mergedResults, InfoAdder, msiProducts, false);

                var startupEntries = new List<StartupEntryBase>();
                foreach (var factory in StartupManager.Factories)
                {
                    try
                    {
                        startupEntries.AddRange(factory.Value());
                    }
                    catch (Exception)
                    {
                    }
                }

                try
                {
                    AttachStartupEntries(mergedResults, startupEntries);
                }
                catch (Exception)
                {
                }

                return mergedResults.OrderBy(e => e.DisplayNameTrimmed).ToList().AsReadOnly();
            }
            finally
            {
                concurrentFactory.Dispose();
            }
        }

        /// <summary>
        ///     Merge new results into the base list
        /// </summary>
        internal static void MergeResults(IList<ApplicationUninstallerEntry> baseEntries,
            IReadOnlyCollection<ApplicationUninstallerEntry> newResults)
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
                    {
                        entry.UninstallerKind = UninstallerType.Unknown;
                    }

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

        private static List<ApplicationUninstallerEntry> GetMiscUninstallerEntries()
        {
            var otherResults = new List<ApplicationUninstallerEntry>();

            var miscFactories = ReflectionTools.GetTypesImplementingBase<IIndependentUninstallerFactory>()
                .Attempt(Activator.CreateInstance)
                .Cast<IIndependentUninstallerFactory>()
                .Where(x => x.IsEnabled())
                .ToList();

            foreach (var kvp in miscFactories)
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    Debug.WriteLine($"Factory {kvp.GetType().Name} started.");
                    MergeResults(otherResults, kvp.GetUninstallerEntries());
                    Debug.WriteLine($"Factory {kvp.GetType().Name} took {sw.ElapsedMilliseconds}ms to finish");
                }
                catch (Exception ex)
                {
                    // catch and continue
                    Debug.WriteLine(ex);
                }
            }

            return otherResults;
        }
    }
}