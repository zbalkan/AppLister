using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using InventoryWmiProvider;

namespace InventoryService
{
    public static class Publisher
    {
        public static void CheckDependency()
        {
            try
            {
                var App = new App
                {
                    Id = "Test"
                };
                InstrumentationManager.Publish(App);
                InstrumentationManager.Revoke(App);
            }
            catch
            {
                throw new Exception("The WMI Provider assembly is not registered.");
            }
        }

        public static void Publish(IEnumerable<App> Apps)
        {
            try
            {
                foreach (var App in Apps)
                {
                    InstrumentationManager.Publish(App);
                }
            }
            catch
            {
                throw new Exception("The WMI Provider assembly is not registered.");
            }
        }

        public static void Unpublish(IEnumerable<App> Apps)
        {
            foreach (var App in Apps)
            {
                try
                {
                    InstrumentationManager.Revoke(App);
                }
                catch
                {
                    // It is okay to ignore if they do not exist.
                }
            }
        }
    }
}