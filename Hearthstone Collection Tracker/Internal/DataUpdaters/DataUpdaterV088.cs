using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public class DataUpdaterV088 : IDataUpdater
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

        private static readonly Version _version = new Version(0, 8, 8);

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
            var actualCards = Hearthstone_Deck_Tracker.Hearthstone.Database.GetActualCards();

            foreach (var file in Directory.GetFiles(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Collection_*.xml", SearchOption.TopDirectoryOnly))
            {
                var cardSets = XmlManager<List<BasicSetCollectionInfo>>.Load(file);

                var newHallOfFame = new List<CardInCollection>();

                var Classic = cardSets.Where(s => s.SetName == SetNames.Classic).FirstOrDefault();
                var classicIds = new string[] { "EX1_161", "EX1_310", "EX1_349" };

                newHallOfFame.AddRange(Classic.Cards.Where(c => classicIds.Contains(c.CardId)));
                Classic.Cards.RemoveAll(c => classicIds.Contains(c.CardId));

                var TheWitchwood = cardSets.Where(s => s.SetName == SetNames.TheWitchwood).FirstOrDefault();
                var witchwoodIds = new string[] { "GIL_130", "GIL_530", "GIL_692", "GIL_826", "GIL_837", "GIL_838" };

                newHallOfFame.AddRange(TheWitchwood.Cards.Where(c => witchwoodIds.Contains(c.CardId)));
                TheWitchwood.Cards.RemoveAll(c => witchwoodIds.Contains(c.CardId));

                BasicSetCollectionInfo HallOfFame = cardSets.Where(s => s.SetName == SetNames.HallofFame).FirstOrDefault();

                if (HallOfFame == null)
                {
                    HallOfFame = new BasicSetCollectionInfo()
                    {
                        SetName = SetNames.HallofFame,
                        Cards = actualCards.Where(c => c.Set == SetNames.HallofFame).Select(c => newHallOfFame.Where(x => x.CardId == c.Id).FirstOrDefault() ?? new CardInCollection()
                        {
                            AmountGolden = 0,
                            AmountNonGolden = 0,
                            CardId = c.Id,
                            DesiredAmount = c.Rarity == Rarity.LEGENDARY ? 1 : 2
                        }).ToList()
                    };

                    cardSets.Add(HallOfFame);
                }
                else
                {
                    HallOfFame.Cards.AddRange(newHallOfFame.Where(c => !HallOfFame.Cards.Select(x => x.CardId).Contains(c.CardId)));
                }
                
                XmlManager<List<BasicSetCollectionInfo>>.Save(file, cardSets);
            }

            var settings = XmlManager<PluginSettings>.Load(ConfigFilePath);
            settings.CurrentVersion = new ModuleVersion(_version);
            XmlManager<PluginSettings>.Save(ConfigFilePath, settings);
        }

        public class PluginSettings
        {
            public ModuleVersion CurrentVersion { get; set; }

            public string ActiveAccount { get; set; }

            public List<AccountSummary> Accounts { get; set; }

            public double CollectionWindowWidth { get; set; }

            public double CollectionWindowHeight { get; set; }

            public bool DefaultShowAllCards { get; set; }

            public bool NotifyNewDeckMissingCards { get; set; }

            public bool EnableDesiredCardsFeature { get; set; }
        }

        public class AccountSummary
        {
            public string AccountName { get; set; }

            public string FileStoragePath { get; set; }
        }

        public class BasicSetCollectionInfo
        {
            public string SetName { get; set; }

            public List<CardInCollection> Cards { get; set; }
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