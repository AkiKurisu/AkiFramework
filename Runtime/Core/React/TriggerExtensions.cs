using UnityEngine;
namespace Kurisu.Framework.React
{
    public static class TriggerExtensions
    {
        #region FixedUpdate
        public static IObservable<Unit> FixedUpdateAsObservable(this GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<ObservableFixedUpdateTrigger>().FixedUpdateAsObservable();
        }
        public static IObservable<Unit> FixedUpdateAsObservable(this Component component)
        {
            return component.gameObject.GetOrAddComponent<ObservableFixedUpdateTrigger>().FixedUpdateAsObservable();
        }
        #endregion
        #region Update
        public static IObservable<Unit> UpdateAsObservable(this GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<ObservableUpdateTrigger>().UpdateAsObservable();
        }
        public static IObservable<Unit> UpdateAsObservable(this Component component)
        {
            return component.gameObject.GetOrAddComponent<ObservableUpdateTrigger>().UpdateAsObservable();
        }
        #endregion
        #region LateUpdate
        public static IObservable<Unit> LateUpdateAsObservable(this GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<ObservableLateUpdateTrigger>().LateUpdateAsObservable();
        }
        public static IObservable<Unit> LateUpdateAsObservable(this Component component)
        {
            return component.gameObject.GetOrAddComponent<ObservableLateUpdateTrigger>().LateUpdateAsObservable();
        }
        #endregion
        #region OnDestroy
        public static IObservable<Unit> OnDestroyAsObservable(this GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<ObservableDestroyTrigger>().OnDestroyAsObservable();
        }
        public static IObservable<Unit> OnDestroyAsObservable(this Component component)
        {
            return component.gameObject.GetOrAddComponent<ObservableDestroyTrigger>().OnDestroyAsObservable();
        }
        #endregion
    }
}