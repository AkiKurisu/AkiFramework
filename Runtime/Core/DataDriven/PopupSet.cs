using System;
using UnityEngine;
namespace Chris.DataDriven
{
    public class PopupSet : ScriptableObject
    {
        [SerializeField]
        private string[] values = Array.Empty<string>();
       
        public string[] Values => values;
        
        public int GetStateID(string state)
        {
            if (values == null) return -1;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == state) return i;
            }
            return -1;
        }
    }
}