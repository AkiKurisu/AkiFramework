using UnityEngine;
namespace Chris.UI
{
    /// <summary>
    /// Field that draws a horizontal separator
    /// </summary>
    public class SeparatorField : BaseField
    {
        public class UIFactory : UIFactory<SeparatorField>
        {

        }
        public SeparatorField() : base(DefaultFactory)
        {
        }
        public SeparatorField(IUIFactory factory) : base(factory)
        {
        }
        
        private static readonly UIFactory DefaultFactory = new();
        
        protected override GameObject OnCreateView(Transform parent)
        {
            GameObject s = Instantiate(parent);
            s.name = nameof(SeparatorField);
            return s;
        }
    }
}