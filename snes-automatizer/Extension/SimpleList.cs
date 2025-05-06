using System.Collections.ObjectModel;
using System.ComponentModel;

namespace snes_automatizer.Extension
{
    public class SimpleList<T> : List<T>
    {
        public void AddRange(params T[] items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
    }
}
