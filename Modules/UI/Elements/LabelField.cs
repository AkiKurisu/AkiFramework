using UnityEngine;
using UnityEngine.UI;
namespace Chris.UI
{
    /// <summary>
    /// Field that draws a label
    /// </summary>
    public class LabelField : BaseField
    {
        public class UIFactory : UIFactory<LabelField>
        {

        }
        
        public LabelField(string displayName) : base(DefaultFactory)
        {
            DisplayName = displayName;
        }
        
        public LabelField(IUIFactory factory, string displayName) : base(factory)
        {
            DisplayName = displayName;
        }
        
        private static readonly UIFactory DefaultFactory = new();
        
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject label = Instantiate(parent);
            label.name = nameof(LabelField);
            Text text = label.GetComponentInChildren<Text>();
            text.text = DisplayName;
            text.color = GetUIStyle().TextColor;
            text.AutoResize();
            return label;
        }
        /// <summary>
        /// Text shown in the label
        /// </summary>
        public string DisplayName { get; }
    }
}