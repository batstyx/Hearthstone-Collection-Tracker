using HearthDb.Enums;
using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.Properties;
using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Collection_Tracker
{
    internal static class SetCardsManager
    {
        public static readonly CardSet[] CollectableSets =
        {
            CardSet.STORMWIND,      // UnitedinStormwind
            CardSet.THE_BARRENS,    // ForgedintheBarrens
            CardSet.DARKMOON_FAIRE, // MadnessattheDarkmoonFaire
            CardSet.SCHOLOMANCE,    // ScholomanceAcademy
            CardSet.BLACK_TEMPLE,   // AshesOfOutland
            CardSet.VANILLA,        // Classic
            CardSet.EXPERT1,        // Legacy Classic
            CardSet.HOF,            // HallofFame
            CardSet.DRAGONS,        // DescentofDragons
            CardSet.ULDUM,          // SaviorsofUldum
            CardSet.DALARAN,        // RiseofShadows           
            CardSet.TROLL,          // RastakhansRumble
            CardSet.BOOMSDAY,       // TheBoomsdayProject
            CardSet.GILNEAS,        // TheWitchwood
            CardSet.LOOTAPALOOZA,   // KoboldsandCatacombs
            CardSet.ICECROWN,       // KnightsoftheFrozenThrone
            CardSet.UNGORO,         // JourneytoUnGoro
            CardSet.GANGS,          // MeanStreetsofGadgetzan
            CardSet.OG,             // WhispersoftheOldGods
            CardSet.TGT,            // TheGrandTournament
            CardSet.GVG,            // GoblinsvsGnomes
        };

        public static readonly CardSet[] StandardSets =
        {
            CardSet.VANILLA,        // Classic
            CardSet.BLACK_TEMPLE,   // AshesOfOutland
            CardSet.SCHOLOMANCE,    // ScholomanceAcademy
            CardSet.DARKMOON_FAIRE, // MadnessattheDarkmoonFaire
            CardSet.THE_BARRENS,    // ForgedintheBarrens
            CardSet.STORMWIND,      // UnitedinStormwind
        };

        public static ZodiacYear GetCardSetYear(CardSet cardSet)
        {
            switch (cardSet)
            {
                case CardSet.EXPERT1: return ZodiacYear.INVALID;        // Classic
                case CardSet.REWARD: return ZodiacYear.INVALID;         // HallofFame
                case CardSet.STORMWIND: return ZodiacYear.GRYPHON;      // UnitedinStormwind
                case CardSet.THE_BARRENS: return ZodiacYear.GRYPHON;    // ForgedintheBarrens
                case CardSet.DARKMOON_FAIRE: return ZodiacYear.PHOENIX; // MadnessattheDarkmoonFaire
                case CardSet.SCHOLOMANCE: return ZodiacYear.PHOENIX;    // ScholomanceAcademy
                case CardSet.BLACK_TEMPLE: return ZodiacYear.PHOENIX;   // AshesOfOutland
                case CardSet.DRAGONS: return ZodiacYear.DRAGON;         // DescentofDragons
                case CardSet.ULDUM: return ZodiacYear.DRAGON;           // SaviorsofUldum
                case CardSet.DALARAN: return ZodiacYear.DRAGON;         // RiseofShadows               
                case CardSet.TROLL: return ZodiacYear.RAVEN;            // RastakhansRumble
                case CardSet.BOOMSDAY: return ZodiacYear.RAVEN;         // TheBoomsdayProject
                case CardSet.GILNEAS: return ZodiacYear.RAVEN;          // TheWitchwood
                case CardSet.LOOTAPALOOZA: return ZodiacYear.MAMMOTH;   // KoboldsandCatacombs
                case CardSet.ICECROWN: return ZodiacYear.MAMMOTH;       // KnightsoftheFrozenThrone
                case CardSet.UNGORO: return ZodiacYear.MAMMOTH;         // JourneytoUnGoro
                case CardSet.GANGS: return ZodiacYear.KRAKEN;           // MeanStreetsofGadgetzan
                case CardSet.OG: return ZodiacYear.KRAKEN;              // WhispersoftheOldGods
                case CardSet.TGT: return ZodiacYear.PRE_STANDARD;       // TheGrandTournament
                case CardSet.GVG: return ZodiacYear.PRE_STANDARD;       // GoblinsvsGnomes
                default: return ZodiacYear.INVALID;
            }
        }

        public static List<BasicSetCollectionInfo> LoadSetsInfo(string collectionStoragePath)
        {
            List<BasicSetCollectionInfo> collection = null;
            try
            {
                var setInfos = XmlManager<List<BasicSetCollectionInfo>>.Load(collectionStoragePath);
                if (setInfos != null)
                {
                    var cards = Database.GetActualCards();
                    CardsInDecks.Instance.UpdateCardsInDecks();
                    collection = setInfos;
                    foreach (var set in CollectableSets)
                    {
                        var currentSetCards = cards.Where(c => c.CardSet.Equals(set));
                        var setInfo = setInfos.FirstOrDefault(si => si.CardSet.Equals(set));
                        if (setInfo == null)
                        {
                            collection.Add(new BasicSetCollectionInfo()
                            {
                                CardSet = set,
                                Cards = currentSetCards.Select(c => new CardInCollection(c)).ToList()
                            });
                        }
                        else
                        {
                            foreach (var card in currentSetCards)
                            {
                                var savedCard = setInfo.Cards.FirstOrDefault(c => c.CardId == card.Id);
                                if (savedCard == null)
                                {
                                    setInfo.Cards.Add(new CardInCollection(card));
                                }
                                else
                                {
                                    savedCard.Card = card;
                                    savedCard.AmountGolden = savedCard.AmountGolden.Clamp(0, savedCard.MaxAmountInCollection);
                                    savedCard.AmountNonGolden = savedCard.AmountNonGolden.Clamp(0, savedCard.MaxAmountInCollection);
                                    savedCard.CopiesInDecks = CardsInDecks.Instance.CopiesInDecks(card.Name);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File with your collection information is corrupted.", ex);
            }
            return collection;
        }

        public static List<BasicSetCollectionInfo> CreateEmptyCollection()
        {
            var cards = Database.GetActualCards();
            var setCards = CollectableSets.Select(set => new BasicSetCollectionInfo()
            {
                CardSet = set,
                Cards = cards.Where(c => c.CardSet == set)
                        .Select(c => new CardInCollection(c))
                        .ToList()
            }).ToList();
            return setCards;
        }

        public static void SaveCollection(List<BasicSetCollectionInfo> collections, string saveFilePath)
        {
            XmlManager<List<BasicSetCollectionInfo>>.Save(saveFilePath, collections);
        }
    }
}
