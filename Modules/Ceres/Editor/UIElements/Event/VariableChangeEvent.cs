using UnityEngine.UIElements;
namespace Ceres.Editor
{
    public enum VariableChangeType
    {
        Create,
        ValueChange,
        NameChange,
        Delete
    }
    public class VariableChangeEvent : EventBase<VariableChangeEvent>
    {
        public SharedVariable Variable { get; private set; }
        public VariableChangeType ChangeType { get; private set; }
        public static VariableChangeEvent GetPooled(SharedVariable notifyVariable, VariableChangeType changeType)
        {
            VariableChangeEvent changeEvent = GetPooled();
            changeEvent.Variable = notifyVariable;
            changeEvent.ChangeType = changeType;
            return changeEvent;
        }
    }
}
