using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace InventoryEngine.Extensions
{
    internal static class ProcessExtensions
    {
        internal static IEnumerable<Process> GetChildProcesses(this Process process)
        {
            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            var results = new List<Process>();
            try
            {
                var searchString = $"Select * From Win32_Process Where ParentProcessID={process.Id}";
                using (var mos = new ManagementObjectSearcher(searchString))
                {
                    foreach (var mo in mos.Get())
                    {
                        //var mo = (ManagementObject) o;
                        Process resultProcess = null;
                        try
                        {
                            resultProcess = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                        }
                        catch (ArgumentException)
                        {
                            // Process exited by now
                        }

                        if (resultProcess != null)
                        {
                            results.Add(resultProcess);
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine("Failed to GetChildProcesses using ManagementObjectSearcher: " + e1.Message);

                try
                {
                    var allProcesses = Process.GetProcesses()
                        .Attempt(proc => new { proc, parent = ParentProcessUtilities.GetParentProcess(proc.Handle) })
                        .Where(x => x.parent != null)
                        .ToList();

                    var newChildren = allProcesses.Where(p => p.parent == process).Select(x => x.proc).ToList();
                    while (newChildren.Any())
                    {
                        results.AddRange(newChildren);
                        newChildren = allProcesses.Where(p => newChildren.Contains(p.parent)).Select(x => x.proc).ToList();
                    }
                }
                catch (Exception e2)
                {
                    Console.WriteLine("Failed to GetChildProcesses using ParentProcessUtilities: " + e2);
                }
            }

            return results.Distinct();
        }
    }
}