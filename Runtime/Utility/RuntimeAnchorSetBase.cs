using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Chris
{
	public class RuntimeAnchorSetBase<T> : ManagedObject
	{
		public event UnityAction OnElementAdded;
		public event UnityAction OnElementRemoved;
		public event UnityAction OnSetEmptied;

		[Header("Debug")]
		[SerializeField] private List<T> _values;

		public List<T> Values => _values;
		public bool IsEmpty => _values.Count == 0;

		/// <summary>
		/// Returns a random element in the set.
		/// </summary>
		/// <returns>A random element in the set. Returns <c>null</c> if the list is empty.</returns>
		public T GetRandomElement()
		{
			if (IsEmpty)
				return default;

			int randomIndex = Random.Range(0, _values.Count);
			return _values[randomIndex];
		}

		/// <summary>
		/// Returns a random element in the set that is not the one provided.
		/// </summary>
		/// <param name="elementToExclude">An element of the set to exclude from the search.</param>
		/// <returns>A random element in the set. Returns <c>null</c> if the list is empty.</returns>
		public T GetRandomElement(T elementToExclude)
		{
			if (IsEmpty) return default;
			if (_values.Count < 2) return default; //need at least 2 elements

			int indexToSkip;

			//Checking if the provided excluded element is actually in the set
			if (_values.Contains(elementToExclude))
				indexToSkip = _values.IndexOf(elementToExclude);
			else
				indexToSkip = 0;
			int tries = 0;

			//Search for a random index, but with a maximum number of tries
			int randomIndex;
			do
			{
				randomIndex = Random.Range(0, _values.Count);
				tries++;
			}
			while (randomIndex == indexToSkip
				   && tries < _values.Count * 2);

			//Search is over, return null anyway if no suitable element was found
			if (randomIndex == indexToSkip)
				return default;
			else
				return _values[randomIndex];
		}

		/// <summary>
		/// Adds an element to the set, and invokes the OnElementAdded event.
		/// </summary>
		/// <returns><c>true</c> if the element was correctly added, <c>false</c> if it was null or already present.</returns>
		public bool AddToSet(T newElement)
		{
			if (newElement == null)
			{
				Debug.LogError("A null value was provided to the " + this.name + " runtime anchor set.");
				return false;
			}

			if (!_values.Contains(newElement))
			{
				_values.Add(newElement);
				OnElementAdded?.Invoke();
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Removes an element from the set and invokes the OnElementRemoved event.
		/// </summary>
		/// <returns><c>true</c> if the element was successfully removed. <c>false</c> if it wasn't present.</returns>
		public bool RemoveFromSet(T elementToRemove)
		{
			if (elementToRemove == null)
			{
				Debug.LogError("Can't remove a null value from the " + this.name + " runtime anchor set.");
				return false;
			}

			if (_values.Contains(elementToRemove))
			{
				_values.Remove(elementToRemove);
				OnElementRemoved?.Invoke();

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Clears the list of elements and fires the OnSetEmptied event.
		/// </summary>
		public void EmptySet()
		{
			_values.Clear();
			OnSetEmptied?.Invoke();
		}

		protected override void OnReset()
		{
			EmptySet();
		}
	}
}