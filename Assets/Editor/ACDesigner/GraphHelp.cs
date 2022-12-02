using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ACDesigner
{
    public class GraphHelp : Singleton<GraphHelp>, IToggle
    {
        public string FindTaskName { get; private set; }
        public string FindVariableName { get; private set; }
        public bool AllTasksFoldout { get; private set; }

        private Rect m_graphRect;
        private int m_copyFromTreeIndex;
        private int m_findVariableIndex;

        public bool IsShow { get; set; }

        public void Show()
        {
            IsShow = true;
        }

        public void Hide()
        {
            IsShow = false;
            FindTaskName = null;
            FindVariableName = null;
            AllTasksFoldout = false;
            m_findVariableIndex = 0;
            m_copyFromTreeIndex = 0;
        }

        public void OnGUI()
        {
            if (ACDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(ACDesignerWindow.Instance.ScreenSizeWidth - 300f - 15f, 18 + (EditorGUIUtility.isProSkin ? 1 : 2), 300f, 35 + 20 * 8);
            }

            if (!IsShow)
            {
                return;
            }

            GUILayout.BeginArea(m_graphRect, ACDesignerUIUtility.PreferencesPaneGUIStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Space(m_graphRect.width * 0.5f - 20);
            EditorGUILayout.LabelField("Help", ACDesignerUIUtility.LabelTitleGUIStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(ACDesignerUIUtility.DeleteButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(16)))
            {
                Hide();
                return;
            }

            GUILayout.EndHorizontal();

            var enable = true;
            GUI.enabled = enable;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Find Task", GUILayout.Width(60));
            FindTaskName = EditorGUILayout.TextField(FindTaskName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = !AllTasksFoldout && enable;
            if (GUILayout.Button("All Tasks Foldout"))
            {
                AllTasksFoldout = true;
            }

            if (AllTasksFoldout)
            {
                GUI.enabled = true && enable;
                GUILayout.Space(5);
                if (GUILayout.Button("Recover"))
                {
                    AllTasksFoldout = false;
                }
            }

            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.EndArea();
        }
    }
}