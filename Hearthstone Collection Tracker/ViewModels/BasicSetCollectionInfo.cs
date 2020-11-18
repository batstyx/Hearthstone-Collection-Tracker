using HearthDb.Enums;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    // used for serialization
    public class BasicSetCollectionInfo
    {
        public CardSet CardSet { get; set; }
        public List<CardInCollection> Cards { get; set; }
    }
}
