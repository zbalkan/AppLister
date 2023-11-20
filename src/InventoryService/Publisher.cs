﻿using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using InventoryWmiProvider;

namespace InventoryService
{
    public static class Publisher
    {
        public static void Publish(IEnumerable<Package> packages)
        {
            try
            {
                foreach (var package in packages)
                {
                    InstrumentationManager.Publish(package);
                }
            }
            catch
            {
                throw new Exception("The WMI Provider assembly is not registered.");
            }
        }

        public static void Unpublish(IEnumerable<Package> packages)
        {
            foreach (var package in packages)
            {
                try
                {
                    InstrumentationManager.Revoke(package);
                }
                catch
                {
                    // It is okay to ignore if they do not exist.
                }
            }
        }
    }
}