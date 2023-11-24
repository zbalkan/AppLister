﻿using System;
using System.IO;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Startup;

namespace InventoryEngine.Junk.Containers
{
    public class StartupJunkNode : JunkResultBase
    {
        public static readonly ConfidenceRecord ConfidenceStartupIsRunOnce = new ConfidenceRecord(-5, "Confidence_Startup_IsRunOnce");

        public static readonly ConfidenceRecord ConfidenceStartupMatched = new ConfidenceRecord(6, "Confidence_Sta rtup_StartupMatched");

        internal StartupEntryBase Entry { get; }

        public override void Backup(string backupDirectory)
        {
            var p = Path.Combine(CreateBackupDirectory(backupDirectory), "Startup");
            Directory.CreateDirectory(p);
            Entry.CreateBackup(p);
        }

        public override void Delete()
        {
            // removed
        }

        public override void Open()
        {
            // removed
        }

        public override string GetDisplayName()
        {
            return Entry.ToString();
        }

        public StartupJunkNode(StartupEntryBase entry, ApplicationUninstallerEntry application, IJunkCreator source)
            : base(application, source)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));

            Confidence.Add(ConfidenceStartupMatched);

            if (entry is StartupEntry normalStartupEntry && normalStartupEntry.IsRunOnce)
            {
                // If the entry is RunOnce, give it some negative points to keep it out of automatic
                // removal. It might be used to clean up after uninstall on next boot.
                Confidence.Add(ConfidenceStartupIsRunOnce);
            }
        }
    }
}