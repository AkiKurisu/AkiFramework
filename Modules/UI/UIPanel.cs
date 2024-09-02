using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Kurisu.Framework.UI
{
    public struct PanelItem
    {
        /// <summary>
        /// Overwrite to set item order
        /// </summary>
        public int Order;
        /// <summary>
        /// Panel item included fields
        /// </summary>
        public List<BaseField> Fields;
    }
    public interface IPanelItem
    {
        /// <summary>
        /// Create items attached to target <see cref="UIPanel"/>
        /// </summary>
        /// <param name="panel">Parent panel</param>
        /// <param name="panelItem">Panel item slot that will be ordered</param>
        void CreatePanelItem(UIPanel panel, ref PanelItem panelItem);
    }
    public class UIPanel : MonoBehaviour
    {
        /// <summary>
        /// Initial panel items
        /// </summary>
        [SerializeField]
        private SerializedType<IPanelItem>[] initialItems;
        private readonly List<BaseField> _fields = new();
        private bool _isInitialized;
#pragma warning disable IDE0051, UNT0006
        private async UniTaskVoid Start()
        {
            await UniTask.Yield();
            CreateFields();
            _isInitialized = true;
        }
#pragma warning restore IDE0051, UNT0006
        private void OnDestroy()
        {
            foreach (var field in _fields)
            {
                field.Dispose();
            }
            _fields.Clear();
        }
        private void CreateFields()
        {
            var items = new PanelItem[initialItems.Length];
            for (var i = 0; i < items.Length; ++i)
            {
                items[i].Fields = new List<BaseField>();
                // use default array order
                items[i].Order = i;
                initialItems[i].GetObject().CreatePanelItem(this, ref items[i]);
            }
            items = items.OrderBy(x => x.Order).ToArray();
            foreach (var item in items)
            {
                _fields.AddRange(item.Fields);
            }
            
            foreach (var field in _fields)
            {
                field.CreateView(transform);
            }
        }
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
                field.CreateView(transform);
            }
            return field;
        }
    }
}