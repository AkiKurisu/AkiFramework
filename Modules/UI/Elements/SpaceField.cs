using UnityEngine;
using UnityEngine.UI;
namespace Chris.UI
{
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
}
