using System;
using UnityEngine;
namespace Chris
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PopupSelector : PropertyAttribute
    {
        public Type PopupType { get; }
        
        public string PopupTitle  { get; }
        
        public PopupSelector(Type type)
        {
            PopupType = type;
        }
        
        public PopupSelector(Type type, string title)
        {
            PopupType = type;
            PopupTitle = title;
        }
        
        public PopupSelector()
        {
            PopupType = typeof(PopupSet);
        }
        
        public PopupSelector(string title)
        {
            PopupType = typeof(PopupSet);
            PopupTitle = title;
        }
    }
}