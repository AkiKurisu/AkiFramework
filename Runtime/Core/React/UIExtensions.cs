using UnityEngine;
using UnityEngine.UI;
namespace Kurisu.Framework.React
{
    public static partial class UIExtensions
    {
        /// <summary>
        /// Observe onClick event.
        /// </summary>
        public static IObservable<Unit> OnClickAsObservable(this Button button)
        {
            return button.onClick.AsObservable();
        }

        /// <summary>
        /// Observe onValueChanged with current `isOn` value on subscribe.
        /// /// </summary>
        public static IObservable<bool> OnValueChangedAsObservable(this Toggle toggle)
        {
            // Optimized Defer + StartWith
            return Observable.CreateWithState<bool, Toggle>(toggle, (t, observer) =>
            {
                observer(t.isOn);
                return t.onValueChanged.AsObservable().Subscribe(observer);
            });
        }

        /// <summary>
        /// Observe onValueChanged with current `value` on subscribe.
        /// </summary>
        public static IObservable<float> OnValueChangedAsObservable(this Scrollbar scrollbar)
        {
            return Observable.CreateWithState<float, Scrollbar>(scrollbar, (s, observer) =>
            {
                observer(s.value);
                return s.onValueChanged.AsObservable().Subscribe(observer);
            });
        }

        /// <summary>
        /// Observe onValueChanged with current `normalizedPosition` value on subscribe.
        /// </summary>
        public static IObservable<Vector2> OnValueChangedAsObservable(this ScrollRect scrollRect)
        {
            return Observable.CreateWithState<Vector2, ScrollRect>(scrollRect, (s, observer) =>
            {
                observer(s.normalizedPosition);
                return s.onValueChanged.AsObservable().Subscribe(observer);
            });
        }

        /// <summary>
        /// Observe onValueChanged with current `value` on subscribe.
        /// </summary>
        public static IObservable<float> OnValueChangedAsObservable(this Slider slider)
        {
            return Observable.CreateWithState<float, Slider>(slider, (s, observer) =>
            {
                observer(s.value);
                return s.onValueChanged.AsObservable().Subscribe(observer);
            });
        }

        /// <summary>
        /// Observe onEndEdit(Submit) event.
        /// </summary>
        public static IObservable<string> OnEndEditAsObservable(this InputField inputField)
        {
            return inputField.onEndEdit.AsObservable();
        }

        /// <summary>
        /// Observe onValueChanged with current `text` value on subscribe.
        /// </summary>
        public static IObservable<string> OnValueChangedAsObservable(this InputField inputField)
        {
            return Observable.CreateWithState<string, InputField>(inputField, (i, observer) =>
            {
                observer(i.text);
                return i.onValueChanged.AsObservable().Subscribe(observer);
            });
        }


        /// <summary>
        /// Observe onValueChanged with current `value` on subscribe.
        /// /// </summary>
        public static IObservable<int> OnValueChangedAsObservable(this Dropdown dropdown)
        {
            return Observable.CreateWithState<int, Dropdown>(dropdown, (d, observer) =>
            {
                observer(d.value);
                return d.onValueChanged.AsObservable().Subscribe(observer);
            });
        }

    }
}
