using UnityEngine;
using UnityEngine.Events;
namespace Kurisu.Framework
{
	public class RuntimeAnchorBase<T> : DescriptiveScriptableObject
	{
		public event UnityAction OnAnchorProvided;
		public event UnityAction OnAnchorUnset;
		[Header("Debug")]
		[SerializeField] private T _value;

		public T Value => _value;
		public bool IsSet => _value != null;

		public void Provide(T newValue)
		{
			if (newValue == null)
			{
				Debug.LogError("A null value was provided to the " + name + " runtime anchor. Use Unset instead");
				return;
			}

			_value = newValue;

			OnAnchorProvided?.Invoke();
		}

		public void Unset()
		{
			_value = default(T);

			OnAnchorUnset?.Invoke();
		}

		private void OnDisable()
		{
			Unset();
		}
	}
}