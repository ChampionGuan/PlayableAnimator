using System;
using UnityEngine;
using UnityEditor;

namespace ACDesigner
{
    [CreateAssetMenu(fileName = "AICustomSettings", menuName = "ScriptableObjects/ACCustomSettings", order = 1)]
    public class CustomSettings : ScriptableObject
    {
        public UsedForType UsedFor = UsedForType.Lua;
        public string LuaRootPath = "LuaSourceCode/";
        public string[] DefinePath = new String[0];

        public string BattleConfigPath = "Battle/Config/AITree/";
        public string BattleEditorConfigPath = "BattleEditor/AITree/";
        public string[] BattleTaskPath = new string[1] {"Battle/ACDesigner/Task"};

        public string SystemConfigPath = "ACDesigner/Config/";
        public string SystemEditorConfigPath = "ACDesigner/Config/Editor/";
        public string[] SystemTaskPath = new string[4] {"Battle/ACDesigner/Task/Entry", "Battle/ACDesigner/Task/Composite", "Battle/ACDesigner/Task/Decorator", "ACDesigner/Task"};

        public string AIVarPath = "Battle/ACDesigner/Base/AIVar";
        public string AIDefinePath = "Battle/ACDesigner/Base/AIDefine";
        public string AITreeCenterPath = "ACDesigner/AITreeMgr";

        public string TreeReaderFilePath = "Editor/ACDesigner/Lua/TreeReader.lua";
        public string TreeWriterFilePath = "Editor/ACDesigner/Lua/TreeWriter.lua";
        public string TreeDebugFilePath = "Editor/ACDesigner/Lua/TreeDebug.lua";
        public string OptionReaderFilePath = "Editor/ACDesigner/Lua/OptionReader.lua";

        public string BattleConfigFullPath
        {
            get => LuaRootPath + BattleConfigPath;
        }

        public string BattleEditorConfigFullPath
        {
            get => LuaRootPath + BattleEditorConfigPath;
        }

        public string SystemConfigFullPath
        {
            get => LuaRootPath + SystemConfigPath;
        }

        public string SystemEditorConfigFullPath
        {
            get => LuaRootPath + SystemEditorConfigPath;
        }
    }

    [CustomEditor(typeof(CustomSettings))]
    public class CustomSettingsEditor : Editor
    {
        public CustomSettings m_settings
        {
            get => target as CustomSettings;
        }

        private SerializedProperty rootPath;
        private SerializedProperty definePath;

        private SerializedProperty battleConfigPath;
        private SerializedProperty battleEditorConfigPath;
        private SerializedProperty battleTaskPath;

        private SerializedProperty systemConfigPath;
        private SerializedProperty systemEditorConfigPath;
        private SerializedProperty systemTaskPath;

        public void OnEnable()
        {
            rootPath = serializedObject.FindProperty("LuaRootPath");
            definePath = serializedObject.FindProperty("DefinePath");

            battleTaskPath = serializedObject.FindProperty("BattleTaskPath");
            battleConfigPath = serializedObject.FindProperty("BattleConfigPath");
            battleEditorConfigPath = serializedObject.FindProperty("BattleEditorConfigPath");

            systemTaskPath = serializedObject.FindProperty("SystemTaskPath");
            systemConfigPath = serializedObject.FindProperty("SystemConfigPath");
            systemEditorConfigPath = serializedObject.FindProperty("SystemEditorConfigPath");
        }

        public override void OnInspectorGUI()
        {
            m_settings.UsedFor = (UsedForType) EditorGUILayout.EnumPopup("Used For", m_settings.UsedFor);
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(rootPath, new GUIContent("Root Path"));
            EditorGUILayout.PropertyField(definePath, new GUIContent("Define Path"));
            if (m_settings.UsedFor == UsedForType.Lua)
            {
                EditorGUILayout.PropertyField(battleTaskPath, new GUIContent("Task Path"));
                EditorGUILayout.PropertyField(battleConfigPath, new GUIContent("Config Path"));
                EditorGUILayout.PropertyField(battleEditorConfigPath, new GUIContent("Config Editor Path"));
            }
            else
            {
                EditorGUILayout.PropertyField(systemTaskPath, new GUIContent("Task Path"));
                EditorGUILayout.PropertyField(systemConfigPath, new GUIContent("Config Path"));
                EditorGUILayout.PropertyField(systemEditorConfigPath, new GUIContent("Config Editor Path"));
            }

            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            serializedObject.ApplyModifiedProperties();
        }
    }
}