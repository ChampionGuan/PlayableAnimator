using System.IO;
using UnityEngine;
using UnityEditor;

namespace ACDesigner
{
    public class GraphCreate : Singleton<GraphCreate>, IToggle
    {
        public bool IsShow { get; set; }

        private Rect m_graphRect;

        private string m_name;
        private string m_directory;
        private int m_copyFromTreeIndex;

        private string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(m_directory))
                {
                    return m_name;
                }

                return $"{m_directory}{m_name}";
            }
        }

        public GraphCreate()
        {
            m_directory = (string) StoragePrefs.GetPref(PrefsType.TreeDirectory);
            if (!string.IsNullOrEmpty(m_directory) && !Directory.Exists($"{Application.dataPath}/{Define.ConfigFullPath}{m_directory}"))
            {
                m_directory = null;
                StoragePrefs.SetPref(PrefsType.TreeDirectory, m_directory);
            }
        }

        public void Show()
        {
            IsShow = true;
        }

        public void Hide()
        {
            IsShow = false;
        }

        public void OnGUI()
        {
            if (ACDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(ACDesignerWindow.Instance.ScreenSizeWidth - 300f - 15f, (float) (18 + (EditorGUIUtility.isProSkin ? 1 : 2)), 300f, 35 + 20 * 3);
            }

            if (!IsShow)
            {
                return;
            }

            GUILayout.BeginArea(m_graphRect, ACDesignerUIUtility.PreferencesPaneGUIStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Space(m_graphRect.width * 0.5f - 40);
            EditorGUILayout.LabelField("Create Tree", ACDesignerUIUtility.LabelTitleGUIStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(ACDesignerUIUtility.DeleteButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(16)))
            {
                Hide();
                return;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Directory", GUILayout.Width(90)))
            {
                ACDesignerLogicUtility.OpenConfigPathFolder(ref m_directory);
                StoragePrefs.SetPref(PrefsType.TreeDirectory, m_directory);
            }

            EditorGUILayout.TextField(m_directory);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}