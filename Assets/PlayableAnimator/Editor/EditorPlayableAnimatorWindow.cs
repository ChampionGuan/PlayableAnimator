using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

namespace X3.PlayableAnimator.Editor
{
    public class EditorPlayableAnimatorWindow : EditorWindow
    {
        public static EditorPlayableAnimatorWindow Instance { get; private set; }
        public float ScreenSizeWidth { get; private set; }
        public float ScreenSizeHeight { get; private set; }


        [MenuItem("X3Tool/PlayableAnimator工具", false, 10)]
        public static void OpenWindow()
        {
            Instance = GetWindow<EditorPlayableAnimatorWindow>(false, "Animator Tool");
            Instance.wantsMouseMove = true;
            Instance.minSize = new Vector2(500f, 400f);

            Instance.ScreenSizeWidth = -1;
            Instance.ScreenSizeHeight = -1;
        }

        private int m_topBarMenuIndex = 0;
        private bool m_topBarIndexChange = true;
        private string[] m_topBarStrings = new string[3] {"Animator Controller", "Animator Prefab", "Runtime"};

        private void OnEnable()
        {
            m_animatorCtrlPath = EditorPrefs.GetString(m_animatorCtrlPathEditorPrefsKey);
            m_animatorCtrlPath = string.IsNullOrEmpty(m_animatorCtrlPath) ? Application.dataPath : m_animatorCtrlPath;
        }

        private void OnGUI()
        {
            if (null == Instance)
            {
                OpenWindow();
            }

            float width = position.width;
            float height = position.height;
            if (ScreenSizeWidth != width || ScreenSizeHeight != height)
            {
                ScreenSizeWidth = width;
                ScreenSizeHeight = height;
            }

            int index = GUILayout.Toolbar(m_topBarMenuIndex, m_topBarStrings, EditorStyles.toolbarButton);
            if (index != m_topBarMenuIndex)
            {
                m_topBarMenuIndex = index;
            }

            GUILayout.Space(5f);
            switch (m_topBarMenuIndex)
            {
                case 0:
                    DrawAnimatorCtrl();
                    break;
                case 1:
                    DrawAnimatorPrefab();
                    break;
                case 2:
                    break;
            }

            GUILayout.Space(10f);
        }

        #region AnimatorCtrl Convert

        private static List<UnityEditor.Animations.AnimatorController> m_animatorControllers = new List<UnityEditor.Animations.AnimatorController>();

        [MenuItem("Assets/Convert Animator Controller", true)]
        public static bool IsValidateCtrl()
        {
            m_animatorControllers.Clear();
            foreach (var obj in Selection.objects)
            {
                if (obj is UnityEditor.Animations.AnimatorController)
                {
                    m_animatorControllers.Add(obj as UnityEditor.Animations.AnimatorController);
                }
            }

            return m_animatorControllers.Count > 0;
        }

        [MenuItem("Assets/Convert Animator Controller")]
        public static void AnimatorCtrlConvert()
        {
            if (!IsValidateCtrl())
            {
                return;
            }

            foreach (var ctrl in m_animatorControllers)
            {
                string path = AssetDatabase.GetAssetPath(ctrl);
                EditorPlayableAnimatorUtility.ToPlayableAnimatorCtrl(ctrl, path.Substring(0, path.LastIndexOf("/") + 1));
            }

            m_animatorControllers.Clear();
        }

        #endregion

        #region Animator Controller

        private string m_animatorCtrlPathEditorPrefsKey = "__PlayableAnimatorControllerPath__";
        private List<UnityEditor.Animations.AnimatorController> m_selectedAnimatorCtrls = new List<UnityEditor.Animations.AnimatorController>();
        private Vector2 m_scrollPositionAnimatorCtrl = Vector2.zero;
        private string m_animatorCtrlPath;

        private void DrawAnimatorCtrl()
        {
            m_selectedAnimatorCtrls.Clear();
            foreach (var obj in Selection.objects)
            {
                if (obj is UnityEditor.Animations.AnimatorController)
                {
                    m_selectedAnimatorCtrls.Add(obj as UnityEditor.Animations.AnimatorController);
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUIUtility.labelWidth = 65;

                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Convert To", m_animatorCtrlPath);
                if (GUILayout.Button("Change", GUILayout.Width(55)))
                {
                    string toPath = EditorUtility.OpenFolderPanel("Select Folder", m_animatorCtrlPath, null);
                    if (!string.IsNullOrEmpty(toPath) && m_animatorCtrlPath != toPath)
                    {
                        m_animatorCtrlPath = toPath;
                        EditorPrefs.SetString(m_animatorCtrlPathEditorPrefsKey, toPath);
                    }
                }

                GUILayout.EndHorizontal();
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUILayout.Label("转换为 Playable Animator Controller  （请在Project面板选中需要检索的对象）");

                m_scrollPositionAnimatorCtrl = GUILayout.BeginScrollView(m_scrollPositionAnimatorCtrl, false, false);
                foreach (var ctrl in m_selectedAnimatorCtrls)
                {
                    EditorGUILayout.ObjectField(ctrl, typeof(Transform), false);
                }

                GUILayout.EndScrollView();

                if (m_selectedAnimatorCtrls.Count > 0 && !string.IsNullOrEmpty(m_animatorCtrlPath) && m_animatorCtrlPath.StartsWith(Application.dataPath) && GUILayout.Button("Convert"))
                {
                    string path = m_animatorCtrlPath.Replace(Application.dataPath, "Assets");
                    foreach (var ctrl in m_selectedAnimatorCtrls)
                    {
                        EditorPlayableAnimatorUtility.ToPlayableAnimatorCtrl(ctrl, path);
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Animator Controller", "转换已完成！", "Ok");
                }
            }
        }

        #endregion

        #region AnimatorPrefab

        private List<GameObject> m_selectedAnimatorPrefabs = new List<GameObject>();
        private Vector2 m_scrollPositionAnimatorPrefab = Vector2.zero;

        private void DrawAnimatorPrefab()
        {
            m_selectedAnimatorPrefabs.Clear();
            foreach (var go in Selection.gameObjects)
            {
                if (null != go.GetComponent<Animator>() && null == go.GetComponent<PlayableAnimator>())
                {
                    m_selectedAnimatorPrefabs.Add(go);
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUILayout.Label("替换预设中的 Animator组件为PlayableAnimator  （请在Project面板选中需要检索的对象）");

                m_scrollPositionAnimatorPrefab = GUILayout.BeginScrollView(m_scrollPositionAnimatorPrefab, false, false);
                foreach (var ctrl in m_selectedAnimatorPrefabs)
                {
                    EditorGUILayout.ObjectField(ctrl, typeof(GameObject), false);
                }

                GUILayout.EndScrollView();

                if (m_selectedAnimatorPrefabs.Count > 0 && GUILayout.Button("Convert"))
                {
                    foreach (var go in m_selectedAnimatorPrefabs)
                    {
                        var animator = go.GetComponent<Animator>();
                        if (null != animator && null == go.GetComponent<PlayableAnimator>())
                        {
                            var avatar = animator.avatar;
                            var applyRootMotion = animator.applyRootMotion;
                            var cullingMode = animator.cullingMode;
                            var playableAnimator = go.AddComponent<PlayableAnimator>();
                            playableAnimator.avatar = avatar;
                            playableAnimator.applyRootMotion = applyRootMotion;
                            playableAnimator.cullingMode = cullingMode;
                        }
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Animator Prefab", "替换已完成！", "Ok");
                }
            }
        }

        #endregion
    }
}