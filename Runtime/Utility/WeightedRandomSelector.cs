using System.Collections.Generic;
using System.Linq;
namespace Chris
{
    public sealed class WeightedRandomSelector<T>
    {
        private readonly List<T> _items;
        
        private readonly List<double> _weights;
        
        private readonly System.Random _random;
        
        private T _lastSelected;
        public int Count => _items.Count;
        public WeightedRandomSelector(int capacity)
        {
            _items = new List<T>(capacity);
            _weights = new List<double>(capacity);
            _random = new System.Random();
        }
        public WeightedRandomSelector()
        {
            _items = new List<T>();
            _weights = new List<double>();
            _random = new System.Random();
        }
        public void AddItem(T item, double weight = 1)
        {
            _items.Add(item);
            _weights.Add(weight);
        }

        public T GetRandomItem(double decayFactor = 0.9)
        {
            while (true)
            {
                double totalWeight = _weights.Sum() - (_lastSelected != null ? _weights[_items.IndexOf(_lastSelected)] : 0);
                double randomNumber = _random.NextDouble() * totalWeight;
                double cumulativeWeight = 0;
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i].Equals(_lastSelected))
                    {
                        // Skip the last selected item
                        continue;
                    }

                    cumulativeWeight += _weights[i];
                    if (randomNumber < cumulativeWeight)
                    {
                        T selected = _items[i];
                        // Decrease the weight of the selected item for future selections
                        _weights[i] *= decayFactor;
                        // Update the last selected item
                        return _lastSelected = selected;
                    }
                }

                // If all items are the last selected item, reset the lastSelected to default
                _lastSelected = default;
                // Perform the selection again
            }
        }
    }
}