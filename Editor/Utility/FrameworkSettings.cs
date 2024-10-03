using Kurisu.Framework.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.Framework.Editor
{
    [FilePath("ProjectSettings/AkiFrameworkSettings.asset")]
    internal class FrameworkSettings : ScriptableSingleton<FrameworkSettings>
    {
        public bool schdulerStackTrace = true;
    }

    internal class FrameworkSettingsProvider : SettingsProvider
    {
        private SerializedObject settingsObject;
        private class Styles
        {
            public static GUIContent s_StackTraceScheduler = new("Stack Trace", "Allow trace scheduled task in editor");
        }
        public FrameworkSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }
        private const string StackTraceSchedulerDisableSymbol = "AF_SCHEDULER_STACK_TRACE_DISABLE";
        private FrameworkSettings settings;
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            settingsObject = new(settings = FrameworkSettings.Instance);
        }
        public override void OnGUI(string searchContext)
        {
            GUILayout.BeginVertical("Scheduler Settings", GUI.skin.box);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(settingsObject.FindProperty("schdulerStackTrace"), Styles.s_StackTraceScheduler);
            if (settingsObject.ApplyModifiedPropertiesWithoutUndo())
            {
                if (settings.schdulerStackTrace)
                    ScriptingSymbol.RemoveScriptingSymbol(StackTraceSchedulerDisableSymbol);
                else
                    ScriptingSymbol.AddScriptingSymbol(StackTraceSchedulerDisableSymbol);
                FrameworkSettings.Save();
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Serialization Settings", GUI.skin.box);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label($"Current Global Objects: {GlobalObjectManager.GetObjectNum()}");
            GUILayout.EndVertical();
        }
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new FrameworkSettingsProvider("Project/AkiFramework Settings", SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            return provider;
        }
    }
}
