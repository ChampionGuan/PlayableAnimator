using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ACDesigner
{
    public class GraphDebug : Singleton<GraphDebug>
    {
        private Color m_runingColor = new Color(0.3207992f, 0.4932138f, 0.764151f, 1f);

        public Rect m_graphRect { get; private set; }

        protected override void OnInstance()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected override void OnDispose()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        public void OnGUI()
        {
            if (ACDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(300, ACDesignerWindow.Instance.ScreenSizeHeight - 18f - 21f, ACDesignerWindow.Instance.ScreenSizeWidth - 300f, 18f);
            }

            GUILayout.BeginArea(m_graphRect, EditorStyles.toolbar);

            GUILayout.BeginHorizontal();
            if (EditorApplication.isPlaying)
            {
                GUI.color = m_runingColor;
            }

            if (GUILayout.Button(ACDesignerUIUtility.PlayTexture, !EditorApplication.isPlaying ? EditorStyles.toolbarButton : ACDesignerUIUtility.ToolbarButtonSelectionGUIStyle, GUILayout.Width(40f)))
            {
                EditorApplication.isPlaying = !EditorApplication.isPlaying;
            }

            GUI.color = Color.white;

            if (EditorApplication.isPaused)
            {
                GUI.color = Color.gray;
            }

            if (GUILayout.Button(ACDesignerUIUtility.PauseTexture, !EditorApplication.isPaused ? EditorStyles.toolbarButton : ACDesignerUIUtility.ToolbarButtonSelectionGUIStyle, GUILayout.Width(40f)))
            {
                EditorApplication.isPaused = !EditorApplication.isPaused;
            }

            GUI.color = Color.white;

            if (!EditorApplication.isPlaying)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button(ACDesignerUIUtility.StepTexture, EditorStyles.toolbarButton, GUILayout.Width(40f)))
            {
                EditorApplication.Step();
            }

            GUI.enabled = true;

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Runtime Controllers", EditorStyles.toolbarPopup, GUILayout.Width(140f)))
                {
                }

                GUILayout.Button("(None GameObject)", EditorStyles.toolbarButton, GUILayout.Width(125f));
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.TextField("animator controller path", ACDesignerUIUtility.PathTitleGUIStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            if (mode == PlayModeStateChange.ExitingPlayMode)
            {
            }
        }
    }
}