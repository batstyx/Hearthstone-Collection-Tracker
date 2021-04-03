using HearthDb.Enums;
using Hearthstone_Collection_Tracker.Properties;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public class DataUpdaterV0120 : IDataUpdater
    {
        public static string ConfigFilePath { get; } = Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, "config.xml");

        public static bool ConfigFolderExists => Directory.Exists(HearthstoneCollectionTrackerPlugin.PluginDataDir);

        public static bool ConfigFileExists => File.Exists(ConfigFilePath);

        public static bool? IsOlderVersion(Version version)
        {
            if (!ConfigFileExists) return null;
            try
            {
                var settings = XmlManager<PluginSettings>.Load(ConfigFilePath);
                return settings.CurrentVersion < new ModuleVersion(version);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static readonly Version _version = new Version(0, 12, 0);

        public Version Version => _version;

        public bool RequiresUpdate
        {
            get
            {
                if (!ConfigFolderExists || !ConfigFileExists)
                {
                    return false;
                }
                return IsOlderVersion(Version) ?? false;
            }
        }

        public void PerformUpdate()
        {
            try
            {
                PerformBackup();

                var settings = XmlManager<PluginSettings>.Load(ConfigFilePath);
                settings.CurrentVersion = new ModuleVersion(Version);
                XmlManager<PluginSettings>.Save(ConfigFilePath, settings);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void PerformBackup()
        {
            var dirInfo = new DirectoryInfo(Path.Combine(HearthstoneCollectionTrackerPlugin.PluginDataDir, $"Backup.{Version}"));
            if (!dirInfo.Exists) dirInfo.Create();

            foreach (var file in Directory.GetFiles(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Collection_*.xml", SearchOption.TopDirectoryOnly))
            {
                var collectionFile = new FileInfo(file);                
                collectionFile.CopyTo(Path.Combine(dirInfo.FullName, collectionFile.Name));
            }

            var settingsFile = new FileInfo(ConfigFilePath);
            settingsFile.CopyTo(Path.Combine(dirInfo.FullName, settingsFile.Name));
        }

        public class PluginSettings
        {
            public ModuleVersion CurrentVersion { get; set; }
            public string ActiveAccount { get; set; }
            public List<AccountSummary> Accounts { get; set; }
            public double CollectionWindowLeft { get; set; }
            public double CollectionWindowTop { get; set; }
            public double CollectionWindowWidth { get; set; }
            public double CollectionWindowHeight { get; set; }
            public bool DefaultShowAllCards { get; set; }
            public bool NotifyNewDeckMissingCards { get; set; }
            public bool EnableDesiredCardsFeature { get; set; }
            public bool EnableAutoImport { get; set; }
            public bool EnableImportHistory { get; set; }
            public bool UseDecksForDesiredCards { get; set; }
        }

        public class AccountSummary
        {
            public string AccountName { get; set; }
            public string FileStoragePath { get; set; }
        }
    }
}
