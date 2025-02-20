using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using InventoryEngine.Extensions;
using InventoryEngine.InfoAdders;
using InventoryEngine.Shared;

namespace InventoryEngine.Factory
{
    internal static class FactoryThreadedHelpers
    {
        internal static int MaxThreadsPerDrive = 2;

        public static IReadOnlyList<ApplicationUninstallerEntry> DriveApplicationScan(
            List<string> dirsToSkip,
            List<DirectoryInfo> itemsToScan)
        {
            var dividedItems = SplitByPhysicalDrives(itemsToScan, d => d);

            void GetUninstallerEntriesThread(DirectoryInfo data, List<ApplicationUninstallerEntry> state)
            {
                if (UninstallToolsGlobalConfig.IsSystemDirectory(data) ||
                    data.Name.StartsWith("Windows", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                var detectedEntries = DirectoryFactory.TryCreateFromDirectory(data, dirsToSkip).ToList();

                ApplicationUninstallerFactory.MergeResults(state, detectedEntries);
            }

            var workSpreader = new ThreadedWorkSpreader<DirectoryInfo, List<ApplicationUninstallerEntry>>
                (MaxThreadsPerDrive, GetUninstallerEntriesThread, list => new List<ApplicationUninstallerEntry>(list.Count), data => data.FullName);

            workSpreader.Start(dividedItems);

            var results = new List<ApplicationUninstallerEntry>();

            foreach (var workerResults in workSpreader.Join())
            {
                ApplicationUninstallerFactory.MergeResults(results, workerResults);
            }

            return results.AsReadOnly();
        }

        public static void GenerateMissingInformation(IReadOnlyCollection<ApplicationUninstallerEntry> entries,
            InfoAdderManager infoAdder, IList<Guid> msiProducts, bool skipRunLast)
        {
            void WorkLogic(ApplicationUninstallerEntry entry, object state)
            {
                infoAdder.AddMissingInformation(entry, skipRunLast);
                if (msiProducts != null)
                {
                    entry.IsValid = CheckIsValid(entry, msiProducts);
                }
            }

            var workSpreader = new ThreadedWorkSpreader<ApplicationUninstallerEntry, object>(MaxThreadsPerDrive,
                WorkLogic, list => null, entry => entry.DisplayName ?? entry.RatingId ?? string.Empty);

            var cDrive = new DirectoryInfo(Environment.SystemDirectory).Root;
            var dividedItems = SplitByPhysicalDrives(entries, entry =>
            {
                var loc = entry.InstallLocation ?? entry.UninstallerLocation;
                if (!string.IsNullOrEmpty(loc))
                {
                    try
                    {
                        return new DirectoryInfo(loc);
                    }
                    catch (SystemException ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
                return cDrive;
            });

            workSpreader.Start(dividedItems);
            workSpreader.Join();
        }

        private static IList<IList<TData>> SplitByPhysicalDrives<TData>(IReadOnlyCollection<TData> itemsToScan, Func<TData, DirectoryInfo> locationGetter)
        {
            var output = new List<IList<TData>>();
            try
            {
                using var searcherDtp = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDriveToDiskPartition");
                using var searcherLtp = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_LogicalDiskToPartition");
                var dtp = searcherDtp.Get().Cast<ManagementObject>().Select(queryObj => new
                {
                    Drive = queryObj["Antecedent"] as string,
                    Partition = queryObj["Dependent"] as string
                });

                var ltp = searcherLtp.Get().Cast<ManagementObject>().Select(queryObj => new
                {
                    Partition = queryObj["Antecedent"] as string,
                    LogicalDrive = queryObj["Dependent"] as string
                });

                var correlatedDriveList = ltp.Join(dtp, arg => arg.Partition, arg => arg.Partition, (x, y) => new
                {
                    LogicalName = x.LogicalDrive.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Append('\\'),
                    y.Drive
                }).Where(x => !string.IsNullOrEmpty(x.LogicalName.ToString())).GroupBy(x => x.Drive);

                var inputList = itemsToScan.Select(x => new { locationGetter(x).Root.Name, x }).ToList();
                foreach (var logicalDriveGroup in correlatedDriveList)
                {
                    var filteredByPhysicalDrive = inputList.Where(x =>
                        logicalDriveGroup.Any(y =>
                            y.LogicalName.ToString().Equals(x.Name, StringComparison.OrdinalIgnoreCase))).ToList();

                    inputList.RemoveAll(filteredByPhysicalDrive);
                    output.Add(filteredByPhysicalDrive.ConvertAll(x => x.x));
                }
                // Bundle leftovers as a single drive
                output.Add(inputList.ConvertAll(x => x.x));
            }
            catch (SystemException ex)
            {
                Debug.WriteLine("Failed to get logical disk to physical drive relationships - " + ex);
                output.Clear();
                output.Add(itemsToScan.ToList());
            }
            return output;
        }

        private static bool CheckIsValid(ApplicationUninstallerEntry target, IEnumerable<Guid> msiProducts)
        {
            if (string.IsNullOrEmpty(target.UninstallerFullFilename))
            {
                return false;
            }

            bool isPathRooted;
            try
            {
                isPathRooted = Path.IsPathRooted(target.UninstallerFullFilename);
            }
            catch (ArgumentException)
            {
                isPathRooted = false;
            }

            if (isPathRooted && File.Exists(target.UninstallerFullFilename))
            {
                return true;
            }

            if (target.UninstallerKind == UninstallerType.Msiexec)
            {
                return msiProducts.Contains(target.BundleProviderKey);
            }

            return !isPathRooted;
        }
    }
}