using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 * A Singleton class to manage how many copies of a card there are in all of
 * the decks the user has created. Be sure to call the Update method whenever
 * we need to refresh the deck lists.
 */

namespace Hearthstone_Collection_Tracker.Internal
{
    public sealed class CardsInDecks
    {
        private static volatile CardsInDecks instance;
        private static object syncRoot = new Object();

        private CardsInDecks()
        {
            UpdateCardsInDecks();
        }

        public static CardsInDecks Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new CardsInDecks();
                        }
                    }
                }

                return instance;
            }
        }

        private SortedDictionary<string, int> Cards { get; set; }

        public int CopiesInDecks(string cardId)
        {
            if (Cards.TryGetValue(cardId, out int copiesInDecks))                
            {
                return copiesInDecks;
            }
            return 0;
        }

        public void UpdateCardsInDecks()
        {
            Cards = new SortedDictionary<string, int>();
            var deckList = DeckList.Instance.Decks.Where(d => !d.IsArenaDeck && !d.IsBrawlDeck).ToList();
            foreach (var deck in deckList)
            {
                foreach (var card in deck.Cards)
                {
                    if (Cards.ContainsKey(card.Id))
                    {
                        int copiesOfCardInDeck = Cards[card.Id];
                        Cards[card.Id] = Math.Max(card.Count, copiesOfCardInDeck);
                    }
                    else
                    {
                        Cards.Add(card.Id, card.Count);
                    }
                }
            }

            if (HearthstoneCollectionTrackerPlugin.Settings != null)
            {
                foreach (var set in HearthstoneCollectionTrackerPlugin.Settings.ActiveAccountSetsInfo)
                {
                    foreach (CardInCollection card in set.Cards)
                    {
                        card.CopiesInDecks = CopiesInDecks(card.CardId); 
                    }
                }
            }
        }
    }
}
