using System.Collections.Generic;
using System.Linq;

namespace SlavApp.Resembler
{
    public class PriorityQueue
    {
        private int size = 5;
        private List<PItemInternal> contents;

        public PriorityQueue(int? size)
        {
            if (size.HasValue)
                this.size = size.Value;
            contents = new List<PItemInternal>(this.size);
        }

        private int BinaryIndexOf(int priority)
        {
            var minIndex = 0;
            var maxIndex = contents.Count - 1;
            var currentIndex = 0;
            var currentElement = 0;

            while (minIndex <= maxIndex)
            {
                currentIndex = (minIndex + maxIndex) >> 1;
                currentElement = contents[currentIndex].priority;

                if (currentElement < priority)
                {
                    minIndex = currentIndex + 1;
                }
                else if (currentElement > priority)
                {
                    maxIndex = currentIndex - 1;
                }
                else
                {
                    return currentIndex;
                }
            }

            return -1 - minIndex;
        }

        public int Length
        {
            get { return this.contents.Count; }
        }

        public int? Insert(int data, int priority)
        {
            var index = BinaryIndexOf(priority);
            if (index < 0) index = -1 - index;
            if (index < size)
            {
                contents.Splice(index, 0, new PItemInternal() { data = data, priority = priority });
            }
            else
            {

            }
            return contents.Count == size ? contents[contents.Count - 1].priority : (int?)null;
        }

        public IEnumerable<PItem> List()
        {
            return contents.Select(item => new PItem { i = item.data, d = item.priority });
        }

        private class PItemInternal
        {
            public int data { get; set; }

            public int priority { get; set; }
        }

        public class PItem
        {
            public int i { get; set; }

            public int d { get; set; }
        }
    }
}