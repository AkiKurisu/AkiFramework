using Kurisu.Framework.DataDriven.Editor;
using Kurisu.Framework.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.Framework.Editor
{
    [FilePath("ProjectSettings/AkiFrameworkSettings.asset")]
    internal class AkiFrameworkSettings : ScriptableSingleton<AkiFrameworkSettings>
    {
        public bool SchdulerStackTrace = true;
        public SerializedType<IDataTableJsonSerializer> DataTableJsonSerializer = SerializedType<IDataTableJsonSerializer>.FromType(typeof(DataTableJsonSerializer));
    }

    internal class AkiFrameworkSettingsProvider : SettingsProvider
    {
        private SerializedObject settingsObject;
        private class Styles
        {
            public static GUIContent s_StackTraceScheduler = new("Stack Trace", "Allow trace scheduled task in editor");
            public static GUIContent s_DataTableJsonSerializer = new("Json Serializer", "Set DataTable json serializer type");
        }
        public AkiFrameworkSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }
        private const string StackTraceSchedulerDisableSymbol = "AF_SCHEDULER_STACK_TRACE_DISABLE";
        private AkiFrameworkSettings settings;
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            settingsObject = new(settings = AkiFrameworkSettings.Instance);
        }
        public override void OnGUI(string searchContext)
        {
            DrawSchedulerSettings();
            DrawDataTableSettings();
        }
        private void DrawSchedulerSettings()
        {
            GUILayout.BeginVertical("Scheduler Settings", GUI.skin.box);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(settingsObject.FindProperty(nameof(AkiFrameworkSettings.SchdulerStackTrace)), Styles.s_StackTraceScheduler);
            if (settingsObject.ApplyModifiedPropertiesWithoutUndo())
            {
                if (settings.SchdulerStackTrace)
                    ScriptingSymbol.RemoveScriptingSymbol(StackTraceSchedulerDisableSymbol);
                else
                    ScriptingSymbol.AddScriptingSymbol(StackTraceSchedulerDisableSymbol);
                AkiFrameworkSettings.Save();
            }
            GUILayout.EndVertical();
        }
        private void DrawDataTableSettings()
        {
            GUILayout.BeginVertical("DataTable Settings", GUI.skin.box);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(settingsObject.FindProperty(nameof(AkiFrameworkSettings.DataTableJsonSerializer)), Styles.s_DataTableJsonSerializer);
            if (settingsObject.ApplyModifiedPropertiesWithoutUndo())
            {
                if (AkiFrameworkSettings.Instance.DataTableJsonSerializer.GetType() == null)
                {
                    AkiFrameworkSettings.Instance.DataTableJsonSerializer = SerializedType<IDataTableJsonSerializer>.FromType(typeof(DataTableJsonSerializer));
                }
                AkiFrameworkSettings.Save();
            }
            GUILayout.EndVertical();
        }
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new AkiFrameworkSettingsProvider("Project/AkiFramework Settings", SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            return provider;
        }
    }
}
