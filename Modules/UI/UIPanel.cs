using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kurisu.Framework.Serialization;
using UnityEngine;
namespace Kurisu.Framework.UI
{
    /// <summary>
    /// Virtual panel item data structure
    /// </summary>
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
    /// <summary>
    /// Interface to create virtual panel item
    /// </summary>
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
        /// <summary>
        /// Add space between each <see cref="initialItems"/> 
        /// </summary>
        public bool AddSpaceForInitialItems = true;
        /// <summary>
        /// Space amount between each <see cref="initialItems"/> 
        /// </summary>
        public int ItemSpace = SpaceField.DefaultSpace;
        [SerializeField, Tooltip("Set to override content container, default use self transform")]
        private Transform contentContainer;
        private PanelField panelField;
        public Transform ContentContainer
        {
            get
            {
                if (contentContainer)
                {
                    return contentContainer;
                }
                return transform;
            }
        }
        protected virtual void Awake()
        {
            panelField ??= new PanelField(this);
        }
        protected virtual void Start()
        {
            InitializePanel().Forget();
        }
        protected async UniTask InitializePanel()
        {
            await UniTask.Yield();
            CreateFields();
        }
        private void OnDestroy()
        {
            panelField.Dispose();
        }
        public PanelField GetRootPanel()
        {
            panelField ??= new PanelField(this);
            return panelField;
        }
        protected void CreateFields()
        {
            // Inject virtual panel items
            var items = new PanelItem[initialItems.Length];
            for (var i = 0; i < items.Length; ++i)
            {
                items[i].Fields = new List<BaseField>();
                // use default array order
                items[i].Order = i;
                initialItems[i].GetObject().CreatePanelItem(this, ref items[i]);
            }
            items = items.OrderBy(x => x.Order).ToArray();

            // Layout
            bool isFirst = true;
            foreach (var item in items)
            {
                if (!isFirst && AddSpaceForInitialItems)
                {
                    panelField.Add(new SpaceField(ItemSpace));
                }
                panelField.AddRange(item.Fields);
                isFirst = false;
            }

            // Create view
            if (!panelField.IsInitialized)
            {
                panelField.CreateView(transform, null);
            }
        }
        public void ClearFields()
        {
            if (panelField == null) return;
            panelField.Clear();
        }
        public TPanel Cast<TPanel>() where TPanel : UIPanel
        {
            return this as TPanel;
        }
    }
}