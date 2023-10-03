using Hearthstone_Collection_Tracker.ViewModels;
using HearthMirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Hearthstone;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal.Importing
{
    internal class HearthstoneImporter
    {
        public int ImportStepDelay { get; set; }

        public List<BasicSetCollectionInfo> Import()
        {
            var sets = SetCardsManager.CreateEmptyCollection();

            try
            {
                var collection = Reflection.Client.GetCollection();
                var goldenCollection = collection.Where(x => x.PremiumType == 1);
                var commonCollection = collection.Where(x => x.PremiumType == 0);
                foreach(var set in sets)
                {
                    foreach(var card in set.Cards)
                    {
                        var amountGolden = goldenCollection.Where(x => x.Id.Equals(card.CardId)).Select(x => x.Count).FirstOrDefault();
                        var amountNonGolden = commonCollection.Where(x => x.Id.Equals(card.CardId)).Select(x => x.Count).FirstOrDefault();

                        card.AmountNonGolden = Math.Min(amountNonGolden, card.MaxAmountInCollection);
                        card.AmountGolden = Math.Min(amountGolden, card.MaxAmountInCollection);
                    }

                }

            }
            catch(ImportingException)
            {
                Log.Error("COLLECTION TRACKER: import exception");
                throw;
            }
            catch(Exception e)
            {
                Log.Error("COLLECTION TRACKER: Random exception when importing");
                Log.Error(e);
            }

            return sets;
        }
    }
}
