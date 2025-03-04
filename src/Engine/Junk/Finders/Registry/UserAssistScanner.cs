﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;
using Engine.Tools;

namespace Engine.Junk.Finders.Registry
{
    internal class UserAssistScanner : IJunkCreator
    {
        public string CategoryName => "Junk_UserAssist_GroupName";

        private static readonly IEnumerable<string> UserAssistGuids = new[]
        {
            //GUIDs for Windows XP
            "{75048700-EF1F-11D0-9888-006097DEACF9}",
            "{5E6AB780-7743-11CF-A12B-00AA004AE837",

            //GUIDs for Windows 7
            "{CEBFF5CD-ACE2-4F4F-9178-9926F41749EA}",
            "{F4E57C4B-2036-45F0-A9AB-443BCFE33D9F}}"
        };

        public IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (string.IsNullOrEmpty(target.InstallLocation))
            {
                yield break;
            }

            foreach (var userAssistGuid in UserAssistGuids)
            {
                using var key = RegistryTools.OpenRegistryKey(
                    $@"{SoftwareRegKeyScanner.KeyCu}\Microsoft\Windows\CurrentVersion\Explorer\UserAssist\{userAssistGuid}\Count");
                if (key == null)
                {
                    continue;
                }

                foreach (var valueName in key.GetValueNames())
                {
                    // Convert the value name to a usable form
                    var convertedName = Rot13(valueName);
                    var guidEnd = convertedName.IndexOf('}') + 1;
                    if (guidEnd > 0 && GuidTools.GuidTryParse(convertedName.Substring(0, guidEnd), out var g))
                    {
                        convertedName = NativeMethods.GetKnownFolderPath(g) + convertedName.Substring(guidEnd);
                    }

                    // Check for matches
                    if (!convertedName.StartsWith(target.InstallLocation,
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    var junk = new RegistryValueJunk(key.Name, valueName, target, this)
                    {
                        DisplayValueName = convertedName
                    };
                    junk.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                    yield return junk;
                }
            }
        }

        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
        }

        private static string Rot13(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return new string(input.Select(x => x >= 'a' && x <= 'z'
                ? (char)(((x - 'a' + 13) % 26) + 'a')
                : x >= 'A' && x <= 'Z'
                    ? (char)(((x - 'A' + 13) % 26) + 'A')
                    : x).ToArray());
        }

        private static class NativeMethods
        {
            public static string GetKnownFolderPath(Guid rfid)
            {
                SHGetKnownFolderPath(rfid, 0, IntPtr.Zero, out var pPath);

                var path = Marshal.PtrToStringUni(pPath);
                Marshal.FreeCoTaskMem(pPath);
                return path;
            }

            [DllImport("shell32.dll")]
            private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
                uint dwFlags, IntPtr hToken, out IntPtr pszPath);
        }
    }
}