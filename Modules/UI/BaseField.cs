using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Kurisu.Framework.Events;
using Kurisu.Framework.React;
using Kurisu.Framework.Resource;
using R3;
using UnityEngine.UI;
namespace Kurisu.Framework.UI
{
    public static class UIEntry
    {
        private static Transform root;
        public static Transform VisualRoot
        {
            get
            {
                if (!root)
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
    public abstract class BaseField : CallbackEventHandler, IDisposable
    {
        public abstract class UIFactory<T> : UI.UIFactory<T> where T : BaseField
        {

        }
        public BaseField()
        {
            Visible.Subscribe(b =>
            {
                foreach (GameObject viewItem in ViewItems)
                {
                    viewItem.SetActive(b);
                }
            });
        }
        public static readonly Color DefaultControlTextColor = new(0.922f, 0.886f, 0.843f);
        public Color TextColor { get; set; } = DefaultControlTextColor;
        public readonly List<GameObject> ViewItems = new(1);
        public ReactiveProperty<bool> Visible { get; } = new(true);
        internal virtual void CreateView(Transform parent, BaseField parentField)
        {
            GameObject view = OnCreateView(parent);
            if (parentField != null)
            {
                Parent = parentField;
            }
            view.SetActive(Visible.Value);
            ViewItems.Add(view);
        }
        protected abstract GameObject OnCreateView(Transform parent);

        public virtual void Dispose()
        {
            Visible.Dispose();
        }
        public TField Cast<TField>() where TField : BaseField
        {
            return this as TField;
        }
        #region Events Implementation
        public override void SendEvent(EventBase e, DispatchMode dispatchMode = DispatchMode.Default)
        {
            e.Target = this;
            EventSystem.Instance.Dispatch(e, dispatchMode, MonoDispatchType.Update);
        }
        public override IEventCoordinator Root => EventSystem.Instance;
        #endregion
    }
    /// <summary>
    /// UI generic base element
    /// </summary>
    public abstract class BaseField<TValue> : BaseField, INotifyValueChanged<TValue>
    {
        protected BaseField(TValue initialValue) : base()
        {
            _value = new ReactiveProperty<TValue>(initialValue);
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

        protected Observable<TValue> OnNotifyViewChanged
        {
            get
            {
                return _value;
            }
        }

        public void SetValue(TValue newValue)
        {
            TValue lastValue = _value.Value;
            if (Equals(newValue, lastValue))
            {
                return;
            }
            _value.OnNext(newValue);
            if (_canFireEvents)
            {
                using var evt = ChangeEvent<TValue>.GetPooled(lastValue, newValue);
                SendEvent(evt);
            }
        }

        public void SetValueWithoutNotify(TValue newValue)
        {
            if (Equals(newValue, _value.Value))
            {
                return;
            }
            _value.OnNext(newValue);
        }

        internal override void CreateView(Transform parent, BaseField parentField)
        {
            bool wasCreated = ViewItems.Any(x => x != null);
            _canFireEvents = false;
            base.CreateView(parent, parentField);
            _canFireEvents = true;
            if (wasCreated) return;
            _value.OnNext(_value.Value);
            using var evt = ChangeEvent<TValue>.GetPooled(default, _value.Value);
            SendEvent(evt);
        }
        public BaseField<TValue> BindProperty<T>(ReactiveProperty<TValue> property, ref T unRegister) where T : struct, IUnRegister
        {
            this.AsObservable<ChangeEvent<TValue>>().SubscribeSafe(e => property.Value = e.NewValue).AddTo(ref unRegister);
            property.Subscribe(e => _value.OnNext(e)).AddTo(ref unRegister);
            _value.OnNext(property.Value);
            return this;
        }
        public override void Dispose()
        {
            _value.Dispose();
            base.Dispose();
        }

        private readonly ReactiveProperty<TValue> _value;
        private bool _canFireEvents;
    }
    #region UI Elements
    public class PanelField : BaseField
    {
        protected override GameObject OnCreateView(Transform parent)
        {
            _isInitialized = true;
            foreach (var field in _fields)
            {
                field.CreateView(Panel.transform, this);
            }
            return Panel.gameObject;
        }
        public UIPanel Panel { get; }
        public PanelField(UIPanel panelObject) : base()
        {
            Panel = panelObject;
        }
        private bool _isInitialized;
        private readonly List<BaseField> _fields = new();
        /// <summary>
        /// Add a field to the panel
        /// </summary>
        /// <param name="field"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Add<T>(T field) where T : BaseField
        {
            _fields.Add(field);
            if (_isInitialized)
            {
                field.CreateView(Panel.transform, this);
            }
            return field;
        }
        /// <summary>
        /// Add fields to the panel
        /// </summary>
        /// <param name="fields"></param>
        public void AddRange(IEnumerable<BaseField> fields)
        {
            _fields.AddRange(fields);
            if (_isInitialized)
            {
                foreach (var field in fields)
                {
                    field.CreateView(Panel.transform, this);
                }
            }
        }
        public override void Dispose()
        {
            foreach (var field in _fields)
            {
                field.Dispose();
            }
            _fields.Clear();
            base.Dispose();
        }
    }
    /// <summary>
    /// Field that draws empty space
    /// </summary>
    public class SpaceField : BaseField
    {
        public class UIFactory : UIFactory<SpaceField>
        {

        }
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject s = UIFactory.Instantiate(parent);
            s.name = nameof(SpaceField);
            s.GetComponent<LayoutElement>().minHeight = Space;
            return s;
        }
        public const int DefaultSpace = 18;
        public int Space { get; }
        public SpaceField(int space = DefaultSpace) : base()
        {
            Space = space;
        }
    }
    /// <summary>
    /// Field that draws a toggle
    /// </summary>
    public class ToggleField : BaseField<bool>
    {
        public class UIFactory : UIFactory<ToggleField>
        {

        }
        public ToggleField(string displayName) : this(displayName, false)
        {
        }

        public ToggleField(string displayName, bool initialValue) : base(initialValue)
        {
            DisplayName = displayName;
        }

        /// <summary>
        /// Text shown next to the checkbox
        /// </summary>
        public string DisplayName { get; }
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject tr = UIFactory.Instantiate(parent);
            Toggle tgl = tr.GetComponentInChildren<Toggle>();
            tgl.onValueChanged.AddListener(SetValue);
            OnNotifyViewChanged.Subscribe(b =>
            {
                tgl.isOn = b;
            });
            Text text = tr.GetComponentInChildren<Text>();
            text.text = DisplayName;
            text.color = TextColor;
            text.SetAutosize();
            return tr;
        }
    }
    /// <summary>
    /// Field that draws a horizontal separator
    /// </summary>
    public class SeparatorField : BaseField
    {
        public class UIFactory : UIFactory<SeparatorField>
        {

        }
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject s = UIFactory.Instantiate(parent);
            s.name = nameof(SeparatorField);
            return s;
        }
        public SeparatorField() : base()
        {
        }
    }
    /// <summary>
    /// Field that draws a label
    /// </summary>
    public class LabelField : BaseField
    {
        public class UIFactory : UIFactory<LabelField>
        {

        }
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject label = UIFactory.Instantiate(parent);
            label.name = nameof(LabelField);
            Text text = label.GetComponentInChildren<Text>();
            text.text = DisplayName;
            text.color = TextColor;
            text.SetAutosize();
            return label;
        }
        /// <summary>
        /// Text shown in the label
        /// </summary>
        public string DisplayName { get; }
        public LabelField(string displayName) : base()
        {
            DisplayName = displayName;
        }
    }
    #endregion
}
