using System;
using UnityEngine;
namespace Kurisu.Framework
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PopupSelector : PropertyAttribute
    {
        private readonly Type mType;
        private readonly string mTitle;
        public PopupSelector(Type type)
        {
            mType = type;
        }
        public PopupSelector(Type type, string title)
        {
            mType = type;
            mTitle = title;
        }
        public PopupSelector()
        {
            mType = typeof(PopupSet);
        }
        public PopupSelector(string title)
        {
            mType = typeof(PopupSet);
            mTitle = title;
        }
        public Type PopupType => mType;
        public string PopupTitle => mTitle;
    }
}