namespace Kurisu.Framework.Events
{
    public interface INotifyValueChanged<T>
    {
        T Value { get; set; }

        void SetValueWithoutNotify(T newValue);
    }
}