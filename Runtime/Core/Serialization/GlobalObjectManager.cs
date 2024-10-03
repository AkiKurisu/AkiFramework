using Kurisu.Framework.Collections;
using UObject = UnityEngine.Object;
namespace Kurisu.Framework.Serialization
{
    /// <summary>
    /// Class for managing dynamically load/created <see cref="UObject"/> 
    /// </summary>
    public static class GlobalObjectManager
    {
        /// <summary>
        /// Cleanup global objects
        /// </summary>
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        public static void Cleanup()
        {
            serialNum = 1;
            GlobalObjects.Clear();
        }
        /// <summary>
        /// Container for UObject
        /// </summary>
        private class ObjectStructure
        {
            public UObject Object;
            public SoftObjectHandle Handle;
        }
        private static ulong serialNum = 1;
        private static readonly SparseList<ObjectStructure> GlobalObjects = new(10, SoftObjectHandle.MaxIndex);
        public static UObject GetObject(SoftObjectHandle handle)
        {
            int index = handle.GetIndex();
            if (handle.IsValid() && GlobalObjects.IsAllocated(index))
            {
                var structure = GlobalObjects[index];
                if (structure.Handle.GetSerialNumber() != handle.GetSerialNumber()) return null;
                return structure.Object;
            }
            return null;
        }
        public static void RegisterObject(UObject uObject, ref SoftObjectHandle handle)
        {
            if (GetObject(handle) != null)
            {
                return;
            }
            var structure = new ObjectStructure() { Object = uObject };
            int index = GlobalObjects.Add(structure);
            handle = new SoftObjectHandle(serialNum, index);
            structure.Handle = handle;
        }
        public static void UnregisterObject(SoftObjectHandle handle)
        {
            int index = handle.GetIndex();
            if (GlobalObjects.IsAllocated(index))
            {
                var current = GlobalObjects[index];
                if (current.Handle.GetSerialNumber() != handle.GetSerialNumber())
                {
                    return;
                }
                // increase serial num as version update
                ++serialNum;
                GlobalObjects.RemoveAt(index);
            }
        }
    }
}