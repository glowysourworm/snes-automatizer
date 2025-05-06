using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace snes_automatizer.Extension
{
    /// <summary>
    /// Collection that can be overridden to provide extra notification for certain collection events that
    /// don't get propagated by an ObservableCollection. Also provides a custom event for item properties updating.
    /// </summary>
    public class SimpleObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public event SimpleEventHandler<SimpleObservableCollection<T>, T, PropertyChangedEventArgs> ItemPropertyChanged;

        bool _sorting;

        public SimpleObservableCollection()
        {
            _sorting = false;
        }

        public SimpleObservableCollection(IEnumerable<T> collection) : base(collection)
        {
            Hook();
        }

        public SimpleObservableCollection(List<T> list) : base(list)
        {
            Hook();
        }

        public void RemoveBy(SimpleLinqPredicate<T> predicate)
        {
            for (int index = this.Count - 1; index >= 0; index--)
            {
                if (predicate(this[index], index))
                {
                    this.RemoveAt(index);
                }
            }
        }

        public void Sort<TResult>(Func<T, TResult> keySelector)
        {
            _sorting = true;

            var list = this.OrderBy(keySelector).ToList();

            this.Clear();

            foreach (var item in list)
            {
                this.Add(item);
            }

            _sorting = false;

            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            item.PropertyChanged += OnItemChanged;
        }

        protected override void RemoveItem(int index)
        {
            this[index].PropertyChanged -= OnItemChanged;

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            Unhook();

            base.ClearItems();
        }

        private void Unhook()
        {
            foreach (var item in this)
                item.PropertyChanged -= OnItemChanged;
        }

        private void Hook()
        {
            foreach (var item in this)
                item.PropertyChanged += OnItemChanged;
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_sorting)
                return;

            if (ItemPropertyChanged != null)
            {
                ItemPropertyChanged(this, (T)sender, e);
            }
        }
    }
}
