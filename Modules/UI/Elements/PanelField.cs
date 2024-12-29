using System.Collections.Generic;
using UnityEngine;
namespace Chris.UI
{
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
        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }
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
        /// Clear all fields from the panel
        /// </summary>
        public void Clear()
        {
            foreach (var field in _fields)
            {
                field.DestroyView();
                field.Dispose();
            }
            _fields.Clear();
            _isInitialized = false;
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
}
