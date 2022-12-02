using UnityEngine;
using System;
using UnityEditor;

namespace ACDesigner
{
    public class GraphPreferences : Singleton<GraphPreferences>, IToggle
    {
        public bool IsShow { get; set; }
        public UsedForType UsedForType { get; private set; }
        public KeyCode QuickSearchTaskPanelShortcut { get; private set; }
        public KeyCode QuickLocateToTreeShortcut { get; private set; }
        public float TaskDebugHighlightTime { get; private set; }
        public bool IsAutoSave { get; private set; }
        public bool IsTaskVariableDebug { get; private set; }
        public bool IsTaskDebugStopAtTheLastFrame { get; private set; }

        private Rect m_graphRect;

        public void Show()
        {
            IsShow = true;
        }

        public void Hide()
        {
            IsShow = false;
        }

        public GraphPreferences()
        {
            Refresh();
        }

        private void Refresh()
        {
            IsAutoSave = (bool) StoragePrefs.GetPref(PrefsType.AutoSave);
            IsTaskVariableDebug = (bool) StoragePrefs.GetPref(PrefsType.TaskVariableDebug);
            IsTaskDebugStopAtTheLastFrame = (bool) StoragePrefs.GetPref(PrefsType.TaskDebugStopAtTheLastFrame);
            TaskDebugHighlightTime = (float) StoragePrefs.GetPref(PrefsType.TaskDebugHighlighting);

            if (Enum.TryParse((string) StoragePrefs.GetPref(PrefsType.QuickSearchTaskPanelShortcut), out KeyCode keyCodeS))
            {
                QuickSearchTaskPanelShortcut = keyCodeS;
            }

            if (Enum.TryParse((string) StoragePrefs.GetPref(PrefsType.QuickLocateToTreeShortcut), out KeyCode keyCodeL))
            {
                QuickLocateToTreeShortcut = keyCodeL;
            }

            if (Enum.TryParse((string) StoragePrefs.GetPref(PrefsType.UsedForType), out UsedForType forType))
            {
                UsedForType = forType;
            }
        }

        public void OnGUI()
        {
            if (ACDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(ACDesignerWindow.Instance.ScreenSizeWidth - 300f - 15f, 18 + (EditorGUIUtility.isProSkin ? 1 : 2), 300f, 35 + 20 * 14);
            }

            if (!IsShow)
            {
                return;
            }

            GUILayout.BeginArea(m_graphRect, ACDesignerUIUtility.PreferencesPaneGUIStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Space(m_graphRect.width * 0.5f - 40);
            EditorGUILayout.LabelField("Preferences", ACDesignerUIUtility.LabelTitleGUIStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(ACDesignerUIUtility.DeleteButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(16)))
            {
                Hide();
                return;
            }

            GUILayout.EndHorizontal();

            StoragePrefs.DrawPref(PrefsType.AutoSave, true, "Auto Save", PrefChangePreHandler, PrefChangeAftHandler);
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPref(PrefsType.TaskVariableDebug, true, "Task Variable Debug", PrefChangePreHandler, PrefChangeAftHandler);
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPref(PrefsType.TaskDebugStopAtTheLastFrame, true, "Task Debug Stop At The Last Frame", PrefChangePreHandler, PrefChangeAftHandler);
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            EditorGUIUtility.labelWidth = 190;

            StoragePrefs.DrawPref(PrefsType.TaskDebugHighlighting, true, "Highlight Time During Debugging", PrefChangePreHandler, PrefChangeAftHandler);
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPopupPref(PrefsType.QuickLocateToTreeShortcut, true, typeof(KeyCode), "Quick Locate To Tree Shortcut", PrefChangePreHandler, PrefChangeAftHandler);
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPopupPref(PrefsType.QuickSearchTaskPanelShortcut, true, typeof(KeyCode), "Quick Search Task Shortcut ", PrefChangePreHandler, PrefChangeAftHandler);
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPopupPref(PrefsType.UsedForType, false, typeof(UsedForType), "Used For", PrefChangePreHandler, PrefChangeAftHandler);
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            EditorGUIUtility.labelWidth = 120;

            EditorGUILayout.TextField("Config Path", Define.ConfigFullPath, ACDesignerUIUtility.PathTitleGUIStyle, GUILayout.Height(30));
            EditorGUILayout.TextField("Config Editor Path", Define.EditorConfigFullPath, ACDesignerUIUtility.PathTitleGUIStyle, GUILayout.Height(30));
            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            if (GUILayout.Button("Restore to Defaults"))
            {
                StoragePrefs.Restore();
                Refresh();
            }

            ACDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            if (GUILayout.Button("Reopen"))
            {
                ACDesignerWindow.Instance.LateUpdate += (sender, e) => { ACDesignerWindow.Open(); };
            }

            GUILayout.EndArea();
        }

        private void PrefChangeAftHandler(PrefsType pref, object value)
        {
            switch (pref)
            {
                case PrefsType.AutoSave:
                    IsAutoSave = (bool) value;
                    break;
                case PrefsType.TaskVariableDebug:
                    IsTaskVariableDebug = (bool) value;
                    break;
                case PrefsType.TaskDebugStopAtTheLastFrame:
                    IsTaskDebugStopAtTheLastFrame = (bool) value;
                    break;
                case PrefsType.TaskDebugHighlighting:
                    TaskDebugHighlightTime = (float) value;
                    break;
                case PrefsType.QuickSearchTaskPanelShortcut:
                    if (Enum.TryParse((string) value, out KeyCode keyCodeS))
                    {
                        QuickSearchTaskPanelShortcut = keyCodeS;
                    }

                    break;
                case PrefsType.QuickLocateToTreeShortcut:
                    if (Enum.TryParse((string) value, out KeyCode keyCodeL))
                    {
                        QuickLocateToTreeShortcut = keyCodeL;
                    }

                    break;
                case PrefsType.UsedForType:
                    if (Enum.TryParse((string) value, out UsedForType forType))
                    {
                        // ACDesignerWindow.Instance.LateUpdate += (sender, e) => { ACDesignerWindow.Open(forType); };
                    }

                    return;
            }
        }

        private void PrefChangePreHandler(PrefsType pref, object value)
        {
            switch (pref)
            {
                default: return;
            }
        }
    }
}