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
    public interface IUIFactory
    {
        GameObject Instantiate(Transform parent);
        ref UIStyle GetUIStyle();
    }
    public struct UIStyle
    {
        private static readonly Color DefaultTextColor = new(0.922f, 0.886f, 0.843f);
        public Color TextColor;
        public static UIStyle DefaultSyle = new()
        {
            TextColor = DefaultTextColor
        };
    }
    /// <summary>
    /// UI factory loading prefab by type name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UIFactory<T> : IUIFactory
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
        private static string address;
        private static ResourceHandle<GameObject> resourceHandle;
        private UIStyle _uiStyle;
        static UIFactory()
        {
            SetAddress(typeof(T).Name);
        }
        public UIFactory(UIStyle style)
        {
            _uiStyle = style;
        }
        public UIFactory()
        {
            _uiStyle = UIStyle.DefaultSyle;
        }
        public ref UIStyle GetUIStyle()
        {
            return ref _uiStyle;
        }
        protected static void SetAddress(string inAddress)
        {
            address = inAddress;
        }
        private static void LoadPrefab()
        {
            resourceHandle = ResourceSystem.AsyncInstantiate(address, UIEntry.VisualRoot);
            prefab = resourceHandle.WaitForCompletion();
            Disposable.Create(ReleasePrefab).AddTo(prefab);
        }
        private static void ReleasePrefab()
        {
            // Release (decrease ref count) after destroy
            prefab = null;
            resourceHandle.Dispose();
        }
        public GameObject Instantiate(Transform parent)
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
            public UIFactory() : base()
            {

            }
            public UIFactory(UIStyle style) : base(style)
            {

            }
        }
        public BaseField(IUIFactory factory)
        {
            _factory = factory;
            Visible.Subscribe(b =>
            {
                foreach (GameObject viewItem in ViewItems)
                {
                    viewItem.SetActive(b);
                }
            });
        }
        public readonly List<GameObject> ViewItems = new(1);
        public ReactiveProperty<bool> Visible { get; } = new(true);
        private readonly IUIFactory _factory;
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
        public GameObject Instantiate(Transform parent)
        {
            return _factory.Instantiate(parent);
        }
        public ref UIStyle GetUIStyle()
        {
            return ref _factory.GetUIStyle();
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
        protected BaseField(TValue initialValue, IUIFactory factory) : base(factory)
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
                field.CreateView(Panel.ContentContainer, this);
            }
            return Panel.gameObject;
        }
        public UIPanel Panel { get; }
        public PanelField(UIPanel panelObject) : base(null)
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
                field.CreateView(Panel.ContentContainer, this);
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
                    field.CreateView(Panel.ContentContainer, this);
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
        public SpaceField(IUIFactory factory, int space = DefaultSpace) : base(factory)
        {
            Space = space;
        }
        public SpaceField(int space = DefaultSpace) : base(defaultFactory)
        {
            Space = space;
        }
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject s = Instantiate(parent);
            s.name = nameof(SpaceField);
            s.GetComponent<LayoutElement>().minHeight = Space;
            return s;
        }
        private static readonly UIFactory defaultFactory = new();
        public const int DefaultSpace = 18;
        public int Space { get; }
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

        public ToggleField(string displayName, bool initialValue) : base(initialValue, defaultFactory)
        {
            DisplayName = displayName;
        }
        public ToggleField(IUIFactory factory, string displayName, bool initialValue) : base(initialValue, factory)
        {
            DisplayName = displayName;
        }

        /// <summary>
        /// Text shown next to the checkbox
        /// </summary>
        public string DisplayName { get; }
        private Toggle toggle;
        private static readonly UIFactory defaultFactory = new();
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject tr = Instantiate(parent);
            toggle = tr.GetComponentInChildren<Toggle>();
            toggle.onValueChanged.AddListener(SetValue);
            OnNotifyViewChanged.Subscribe(b =>
            {
                toggle.isOn = b;
            });
            Text text = tr.GetComponentInChildren<Text>();
            text.text = DisplayName;
            text.color = GetUIStyle().TextColor;
            text.SetAutosize();
            return tr;
        }
        public override void Dispose()
        {
            if (toggle) toggle.onValueChanged.RemoveListener(SetValue);
            base.Dispose();
        }
    }
    /// <summary>
    /// Field that draws a button
    /// </summary>
    public class ButtonField : BaseField
    {
        public class UIFactory : UIFactory<ButtonField>
        {

        }
        public ButtonField(string displayName, Action onClicked) : base(defaultFactory)
        {
            DisplayName = displayName;
            OnClicked = onClicked;
        }
        public ButtonField(IUIFactory factory, string displayName, Action onClicked) : base(factory)
        {
            DisplayName = displayName;
            OnClicked = onClicked;
        }

        /// <summary>
        /// Text shown next to the checkbox
        /// </summary>
        public string DisplayName { get; }
        public Action OnClicked { get; }
        private Button button;
        private static readonly UIFactory defaultFactory = new();
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject tr = Instantiate(parent);
            button = tr.GetComponentInChildren<Button>();
            button.onClick.AddListener(OnButtonClicked);
            Text text = tr.GetComponentInChildren<Text>();
            text.text = DisplayName;
            text.color = GetUIStyle().TextColor;
            text.SetAutosize();
            return tr;
        }
        private void OnButtonClicked()
        {
            OnClicked?.Invoke();
        }
        public override void Dispose()
        {
            if (button) button.onClick.RemoveListener(OnButtonClicked);
            base.Dispose();
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
        public SeparatorField() : base(defaultFactory)
        {
        }
        public SeparatorField(IUIFactory factory) : base(factory)
        {
        }
        private static readonly UIFactory defaultFactory = new();
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject s = Instantiate(parent);
            s.name = nameof(SeparatorField);
            return s;
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
        public LabelField(string displayName) : base(defaultFactory)
        {
            DisplayName = displayName;
        }
        public LabelField(IUIFactory factory, string displayName) : base(factory)
        {
            DisplayName = displayName;
        }
        private static readonly UIFactory defaultFactory = new();
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject label = Instantiate(parent);
            label.name = nameof(LabelField);
            Text text = label.GetComponentInChildren<Text>();
            text.text = DisplayName;
            text.color = GetUIStyle().TextColor;
            text.SetAutosize();
            return label;
        }
        /// <summary>
        /// Text shown in the label
        /// </summary>
        public string DisplayName { get; }
    }
    #endregion
}
