using System.Collections.Generic;
using System.Linq;
namespace Kurisu.Framework.Utility
{
    public class WeightedRandomSelector<T>
    {
        private readonly List<T> items;
        private readonly List<double> weights;
        private readonly System.Random random;
        private T lastSelected;
        public int Count => items.Count;
        public WeightedRandomSelector(int capacity)
        {
            items = new List<T>(capacity);
            weights = new List<double>(capacity);
            random = new System.Random(capacity);
        }
        public WeightedRandomSelector()
        {
            items = new List<T>();
            weights = new List<double>();
            random = new System.Random();
        }
        public void AddItem(T item, double weight = 1)
        {
            items.Add(item);
            weights.Add(weight);
        }

        public T GetRandomItem(double decayFactor = 0.9)
        {
            double totalWeight = weights.Sum() - (lastSelected != null ? weights[items.IndexOf(lastSelected)] : 0);
            double randomNumber = random.NextDouble() * totalWeight;
            double cumulativeWeight = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Equals(lastSelected))
                {
                    // Skip the last selected item
                    continue;
                }

                cumulativeWeight += weights[i];
                if (randomNumber < cumulativeWeight)
                {
                    T selected = items[i];
                    // Decrease the weight of the selected item for future selections
                    weights[i] *= decayFactor;
                    // Update the last selected item
                    return lastSelected = selected;
                }
            }
            // If all items are the last selected item, reset the lastSelected to default
            lastSelected = default;
            // Perform the selection again
            return GetRandomItem(decayFactor);
        }
    }
}