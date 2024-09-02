using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using Kurisu.Framework.React;
using R3;
using Kurisu.Framework.Resource;
namespace Kurisu.Framework.UI
{
    public static class UIEntry
    {
        private static Transform root;
        public static Transform VisualRoot{
            get
            {
                if(!root)
                {
                    root = new GameObject(nameof(UIEntry)).transform;
                    Disposable.Create(static () => root = null).AddTo(root);
                }
                return root;
            }
        }
    }
    /// <summary>
    /// UI factory loading prefab by type name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UIFactory<T>
    {
        public static GameObject Prefab
        {
            get
            {
                if (prefab == null)
                {
                    LoadPrefab();
                }
                return prefab;
            }
        }
        private static GameObject prefab;
        private static readonly string address;
        static UIFactory()
        {
            address = typeof(T).Name;
        }
        private static void LoadPrefab()
        {
            var handle = ResourceSystem.AsyncInstantiate(address, UIEntry.VisualRoot);
            prefab = handle.WaitForCompletion();
            // Release (decrease ref count) after destroy
            Disposable.Create(() => { prefab = null; handle.Dispose(); }).AddTo(prefab);
        }
        public static GameObject Instantiate(Transform parent)
        {
            return UnityEngine.Object.Instantiate(Prefab, parent);
        }
    }
    /// <summary>
    /// UI base element
    /// </summary>
    public abstract class BaseField : IDisposable
    {
        public abstract class UIFactory<T> : UI.UIFactory<T> where T : BaseField
        {

        }
        public BaseField()
        {
            Visible.Subscribe(b =>
            {
                foreach (GameObject controlObject in ViewItems)
                {
                    controlObject.SetActive(b);
                }
            });
        }
        public static readonly Color DefaultControlTextColor = new(0.922f, 0.886f, 0.843f);
        public Color TextColor { get; set; } = DefaultControlTextColor;
        private readonly List<GameObject> _viewItems = new(1);
        public List<GameObject> ViewItems => _viewItems;
        public ReactiveProperty<bool> Visible { get; } = new(true);
        internal virtual void CreateView(Transform parent)
        {
            GameObject view = OnCreateView(parent);
            if (view.TryGetComponent<LayoutElement>(out var layoutElement))
            {
                layoutElement.minWidth = 300f;
            }
            view.SetActive(Visible.Value);
            _viewItems.Add(view);
        }
        protected abstract GameObject OnCreateView(Transform parent);

        public virtual void Dispose()
        {
            Visible.Dispose();
        }
    }
    /// <summary>
    /// UI generic base element
    /// </summary>
    public abstract class BaseField<TValue> : BaseField
    {
        protected BaseField(TValue initialValue) : base()
        {
            _value = new ReactiveProperty<TValue>(initialValue);
            _onValueChanged = new Subject<TValue>();
        }
        
        public TValue Value
        {
            get
            {
                return _value.Value;
            }
            set
            {
                SetValue(value);
            }
        }
        
        public Observable<TValue> OnValueChanged
        {
            get
            {
                return _onValueChanged;
            }
        }
        
        protected Observable<TValue> OnNotifyViewChanged
        {
            get
            {
                return _value;
            }
        }
        
        public void SetValue(TValue newValue)
        {
            SetValue(newValue, true);
        }
        
        public void SetValue(TValue newValue, bool fireEvents)
        {
            if (Equals(newValue, _value.Value))
            {
                return;
            }
            _value.OnNext(newValue);
            if (_canFireEvents && fireEvents)
            {
                _onValueChanged.OnNext(newValue);
            }
        }

        internal override void CreateView(Transform parent)
        {
            bool wasCreated = ViewItems.Any(x=>x!=null);
            _canFireEvents = false;
            base.CreateView(parent);
            _canFireEvents = true;
            if (wasCreated) return;
            _value.OnNext(_value.Value);
            _onValueChanged.OnNext(_value.Value);
        }
        public BaseField<TValue> BindProperty<T>(R3.ReactiveProperty<TValue> property, ref T unRegister) where T : struct, IUnRegister
        {
            _onValueChanged.AsObservable().Subscribe(e => property.Value = e).AddTo(ref unRegister);
            property.Subscribe(e => _value.OnNext(e)).AddTo(ref unRegister);
            _value.OnNext(property.Value);
            return this;
        }
        public override void Dispose()
        {
            _value.Dispose();
            _onValueChanged.Dispose();
            base.Dispose();
        }

        private readonly ReactiveProperty<TValue> _value;
        private readonly Subject<TValue> _onValueChanged;
        private bool _canFireEvents;
    }
}
