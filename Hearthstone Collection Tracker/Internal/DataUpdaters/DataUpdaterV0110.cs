using HearthDb.Enums;
using Hearthstone_Collection_Tracker.Properties;
using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public class DataUpdaterV0110 : IDataUpdater
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

        private static readonly Version _version = new Version(0, 11, 0);

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
            foreach (var file in Directory.GetFiles(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Collection_*.xml", SearchOption.TopDirectoryOnly))
            {
                UpdateCollection(file);
            }

            foreach (var dir in Directory.GetDirectories(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Changes_*"))
            {
                foreach (var file in Directory.GetFiles(dir, "*.xml"))
                {
                    UpdateCollection(file);
                }
            }

            var settings = XmlManager<PluginSettings>.Load(ConfigFilePath);
            settings.CurrentVersion = new ModuleVersion(Version);
            XmlManager<PluginSettings>.Save(ConfigFilePath, settings);
        }

        private static void UpdateCollection(string file)
        {
            var oldCardSets = XmlManager<List<Old.BasicSetCollectionInfo>>.Load(file);

            var newCardSets = oldCardSets.Select(set => new New.BasicSetCollectionInfo
            {
                CardSet = SetMap[set.SetName],
                Cards = set.Cards,
            }).ToList();

            XmlManager<List<New.BasicSetCollectionInfo>>.Save(file, newCardSets);
        }

        private static readonly Dictionary<string, CardSet> SetMap = new Dictionary<string, CardSet> {
            { SetNames.Classic, CardSet.EXPERT1 },
            { SetNames.ScholomanceAcademy, CardSet.SCHOLOMANCE },
            { SetNames.AshesOfOutland, CardSet.BLACK_TEMPLE },
            { SetNames.DescentofDragons, CardSet.DRAGONS },
            { SetNames.SaviorsofUldum, CardSet.ULDUM },
            { SetNames.RiseofShadows, CardSet.DALARAN },
            { SetNames.HallofFame, CardSet.REWARD },
            { SetNames.RastakhansRumble, CardSet.TROLL },
            { SetNames.TheBoomsdayProject, CardSet.BOOMSDAY },
            { SetNames.TheWitchwood, CardSet.GILNEAS },
            { SetNames.KoboldsandCatacombs, CardSet.LOOTAPALOOZA },
            { SetNames.KnightsoftheFrozenThrone, CardSet.ICECROWN },
            { SetNames.JourneytoUnGoro, CardSet.UNGORO },
            { SetNames.MeanStreetsofGadgetzan, CardSet.GANGS },
            { SetNames.WhispersoftheOldGods, CardSet.OG },
            { SetNames.TheGrandTournament, CardSet.TGT },
            { SetNames.GoblinsvsGnomes, CardSet.GVG },
        };

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

        public class Old
        {
            public class BasicSetCollectionInfo
            {
                public string SetName { get; set; }
                public List<CardInCollection> Cards { get; set; }
            }
        }

        public class New
        {
            public class BasicSetCollectionInfo
            {
                public CardSet CardSet { get; set; }
                public List<CardInCollection> Cards { get; set; }
            }
        }


        public class CardInCollection
        {
            public int AmountNonGolden { get; set; }
            public int AmountGolden { get; set; }
            public int CopiesInDecks { get; set; }
            public int DesiredAmount { get; set; }
            public string CardId { get; set; }
        }
    }
}
