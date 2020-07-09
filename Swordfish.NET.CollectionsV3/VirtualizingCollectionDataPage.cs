// Copied from http://www.zagstudio.com/blog/378 license MS-PL (Microsoft Public License)

using System;
using System.Collections.Generic;
using System.Linq;

namespace Swordfish.NET.Collections
{
    public class VirtualizingCollectionDataPage<T> where T : class
    {
        public VirtualizingCollectionDataPage(int firstIndex, int pageLength)
        {
            this.Items = new List<VirtualizingCollectionDataWrapper<T>>(pageLength);
            for (int i = 0; i < pageLength; i++)
            {
                this.Items.Add(new VirtualizingCollectionDataWrapper<T>(firstIndex + i));
            }
            this.TouchTime = DateTime.Now;
        }

        public IList<VirtualizingCollectionDataWrapper<T>> Items { get; set; }

        public DateTime TouchTime { get; set; }

        public bool IsInUse
        {
            get { return this.Items.Any(wrapper => wrapper.IsInUse); }
        }

        public void Populate(IList<T> newItems)
        {
            int i;
            int index = 0;
            for (i = 0; i < newItems.Count && i < this.Items.Count; i++)
            {
                this.Items[i].Data = newItems[i];
                index = this.Items[i].Index;
            }

            while (i < newItems.Count)
            {
                index++;
                this.Items.Add(new VirtualizingCollectionDataWrapper<T>(index) { Data = newItems[i] });
                i++;
            }

            while (i < this.Items.Count)
            {
                this.Items.RemoveAt(this.Items.Count - 1);
            }
        }
    }
}
