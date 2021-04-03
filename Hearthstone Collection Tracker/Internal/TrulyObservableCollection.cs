using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Hearthstone_Collection_Tracker.Internal
{
    public sealed class TrulyObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
    {
        public TrulyObservableCollection()
        {
            CollectionChanged += FullObservableCollectionCollectionChanged;
        }

        public TrulyObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
            CollectionChanged += FullObservableCollectionCollectionChanged;
            foreach (var item in collection.OfType<INotifyPropertyChanged>())
            {
                item.PropertyChanged += ItemPropertyChanged;
            }
        }

        public TrulyObservableCollection(List<T> list)
            : base(list)
        {
            CollectionChanged += FullObservableCollectionCollectionChanged;
            foreach (var item in list.OfType<INotifyPropertyChanged>())
            {
                item.PropertyChanged += ItemPropertyChanged;
            }
        }

        private void FullObservableCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
            OnCollectionChanged(args);
        }
    }
}
