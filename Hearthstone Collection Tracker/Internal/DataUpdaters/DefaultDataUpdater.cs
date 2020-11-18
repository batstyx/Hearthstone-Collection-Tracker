using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public static class DefaultDataUpdater
    {
        /// <summary>
        /// A list of update steps that are required to keep data files up-to-date
        /// </summary>
        public static IEnumerable<IDataUpdater> Updaters = new List<IDataUpdater>()
        {            
            new DataUpdaterV088(),
            new DataUpdaterV0110(),
        };


        public static void PerformUpdates()
        {
            foreach(var updater in Updaters.OrderBy(u => u.Version))
            {
                if (updater.RequiresUpdate)
                    updater.PerformUpdate();
            }
        }
    }
}
