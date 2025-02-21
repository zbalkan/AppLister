using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using InventoryEngine.Extensions;
using InventoryEngine.Shared;
using InventoryEngine.Tools;

namespace InventoryEngine.InfoAdders
{
    /// <summary>
    ///     Read infoadders from assembly
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    internal class InfoAdderManager
    {
        private static readonly Type BoolType = typeof(bool);

        private static readonly IMissingInfoAdder[] InfoAdders;

        private static readonly ICollection<CompiledPropertyInfo<ApplicationUninstallerEntry>> NonUninstallerProperties;

        private static readonly Dictionary<string, CompiledPropertyInfo<ApplicationUninstallerEntry>> TargetProperties;

        private static readonly ICollection<CompiledPropertyInfo<ApplicationUninstallerEntry>> UninstallerProperties;

        /// <summary>
        ///     Static constructor to ensure this is initiated only once.
        /// </summary>
        /// <exception cref="ReflectionTypeLoadException">
        ///     If this exception is thrown, let the service die. It is a broken state.
        /// </exception>
        static InfoAdderManager()
        {
            InfoAdders = GetInfoAdders().ToArray();

            var defaultValues = new ApplicationUninstallerEntry();
            TargetProperties = typeof(ApplicationUninstallerEntry)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CanRead && x.CanWrite && IsTypeValid(x.PropertyType))
                 .ToDictionary(x => x.Name, x =>
                 {
                     var compiled = x.CompileAccessors<ApplicationUninstallerEntry>();
                     compiled.Tag = compiled.CompiledGet(defaultValues);
                     return compiled;
                 });

            // Split properties related to uninstaller and its type so they can be moved all at same
            // time TODO Better sorting logic? If names change or props are added without uninstall
            // in name they'll slip through
            foreach (var group in TargetProperties.Where(x => x.Key != nameof(ApplicationUninstallerEntry.UninstallerKind))
                .GroupBy(x => x.Key.Contains("uninstall", StringComparison.OrdinalIgnoreCase)))
            {
                if (group.Key)
                {
                    UninstallerProperties = group.Select(x => x.Value).ToList();
                }
                else
                {
                    NonUninstallerProperties = group.Select(x => x.Value).ToList();
                }
            }
        }

        /// <summary>
        ///     Fill in information using all detected IMissingInfoAdder classes
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Control flow", "TI6101:Do not change a loop variable inside a for loop block", Justification = "We need to reset the counter if anything is found.")]
        public void AddMissingInformation(ApplicationUninstallerEntry target, bool skipRunLast = false)
        {
            var adders = InfoAdders.Where(x => !skipRunLast || x.Priority > InfoAdderPriority.RunLast).ToList();
            var valueIsDefaultCache = new Dictionary<string, bool>();

            // Checks if the value is default, buffering the result
            bool IsValueDefault(string key)
            {
                if (valueIsDefaultCache.TryGetValue(key, out var valIsDefault))
                {
                    return valIsDefault;
                }

                if (!TargetProperties.TryGetValue(key, out var property))
                {
                    return true;
                }

                valIsDefault = Equals(property.CompiledGet(target), property.Tag);
                valueIsDefaultCache.Add(key, valIsDefault);

                return valIsDefault;

                // If we can't check if the value is default, assume that it is to be safe
            }

            for (var index = 0; index < adders.Count; index++)
            {
                var infoAdder = adders[index];
                var requirements = infoAdder.RequiredValueNames;

                //TODO prioritize ones with all values existing from same priority tier?
                if (requirements.Any())
                {
                    if (infoAdder.RequiresAllValues)
                    {
                        if (infoAdder.RequiredValueNames.Any(IsValueDefault))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (infoAdder.RequiredValueNames.All(IsValueDefault))
                        {
                            continue;
                        }
                    }
                }

                // Only run the adder if it can actually fill in any missing values
                if (!infoAdder.AlwaysRun && !infoAdder.CanProduceValueNames.Any(IsValueDefault))
                {
                    continue;
                }

                infoAdder.AddMissingInformation(target);

                adders.Remove(infoAdder);

                // Remove items that might have changed from cache so they get recalculated
                foreach (var valueName in infoAdder.CanProduceValueNames)
                {
                    valueIsDefaultCache.Remove(valueName);

                    foreach (var relatedValueName in ApplicationUninstallerEntry.PropertyRelationships[valueName])
                    {
                        valueIsDefaultCache.Remove(relatedValueName);
                    }
                }

                // Retry all adders from the start if any of them succeeded
                index = -1;
            }
        }

        /// <summary>
        ///     Copy missing property values
        /// </summary>
        /// <param name="baseEntry">
        ///     Copy values to this object
        /// </param>
        /// <param name="entryToMerge">
        ///     Copy from this object
        /// </param>
        public void CopyMissingInformation(ApplicationUninstallerEntry baseEntry, ApplicationUninstallerEntry entryToMerge)
        {
            // If one of these is not null it will be merged by loop below. If both are not null
            // they need special logic.
            if (baseEntry.StartupEntries != null && entryToMerge.StartupEntries != null)
            {
                baseEntry.StartupEntries = baseEntry.StartupEntries.Concat(entryToMerge.StartupEntries);
            }

            void CopyPropertyIfBetter(CompiledPropertyInfo<ApplicationUninstallerEntry> property, bool alwaysCopy)
            {
                // If entryToMerge has a default (not set) value for this property, skip it so we
                // don't lose data
                var newValue = property.CompiledGet(entryToMerge);
                if (Equals(newValue, property.Tag))
                {
                    return;
                }

                if (alwaysCopy)
                {
                    property.CompiledSet(baseEntry, newValue);
                }
                else
                {
                    // Copy new value to base entry if base doesn't have the value set, or if the
                    // values are strings and merged value is longer
                    var oldValue = property.CompiledGet(baseEntry);
                    if (Equals(oldValue, property.Tag) ||
                        (newValue is string sNew && oldValue is string sOld && sNew.Length > sOld.Length))
                    {
                        property.CompiledSet(baseEntry, newValue);
                    }
                }
            }

            foreach (var property in NonUninstallerProperties)
            {
                CopyPropertyIfBetter(property, false);
            }

            // Make sure that all uninstaller-related properties are only copied when necessary, and
            // that UninstallerKind always changes together with the uninstall strings or we will
            // get bugs elsewhere if there is a mismatch
            if ((baseEntry.UninstallerKind == UninstallerType.Unknown && entryToMerge.UninstallerKind != UninstallerType.Unknown) ||
                (baseEntry.UninstallerKind == UninstallerType.SimpleDelete && entryToMerge.UninstallPossible) ||
                !baseEntry.UninstallPossible ||
                entryToMerge.UninstallerKind == UninstallerType.PowerShell)
            {
                baseEntry.UninstallerKind = entryToMerge.UninstallerKind;
                foreach (var property in UninstallerProperties)
                {
                    CopyPropertyIfBetter(property, true);
                }
            }

            baseEntry.AdditionalJunk.AddRange(entryToMerge.AdditionalJunk);
        }

        /// <summary>
        ///     Get classes from assembly.
        /// </summary>
        /// <exception cref="ReflectionTypeLoadException">
        ///     When using reflection to get the type, it is possible to get this exception. Yet, no
        ///     proper way yto handle.
        /// </exception>
        private static IEnumerable<IMissingInfoAdder> GetInfoAdders()
        {
            var type = typeof(IMissingInfoAdder);
            var types = Assembly.GetExecutingAssembly().GetTypes() //AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

            var instances = types.Attempt(Activator.CreateInstance);

            return instances.Cast<IMissingInfoAdder>().OrderByDescending(x => x.Priority);
        }

        /// <summary>
        ///     Check if we can correctly detect if the type has no value.
        /// </summary>
        private static bool IsTypeValid(Type type) =>
            type != BoolType || Nullable.GetUnderlyingType(type) != null;

        private string GetDebuggerDisplay() => ToString();
    }
}