using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal.Importing
{
    internal class ImportHistory
    {
        public static void SaveChange(AccountSummary account, List<BasicSetCollectionInfo> change)
        {
            if (account == null || change?.Count == 0) return;

            var path = Path.Combine(Path.GetDirectoryName(account.FileStoragePath), string.Format("Changes_{0}", account.AccountName));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            path = Path.Combine(path, Path.ChangeExtension(Path.GetRandomFileName(), ".xml"));
            XmlManager<List<BasicSetCollectionInfo>>.Save(path, change);
        }

        public string AccountName { get; set; }

        public List<CollectionChange> Changes { get; set; }
    }

    public class CollectionChange
{
        public DateTime DateTimeStamp { get; set; }

        public List<BasicSetCollectionInfo> change { get; set; }
    }
}
