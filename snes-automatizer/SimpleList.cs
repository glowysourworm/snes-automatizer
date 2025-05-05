using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snes_automatizer
{
    public class SimpleList<T> : List<T>
    {
        public void AddRange(params T[] items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }
    }

    public class SimpleObservableCollection<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }
    }
}
