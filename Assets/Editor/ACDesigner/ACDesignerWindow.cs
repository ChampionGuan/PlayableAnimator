using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace ACDesigner
{
    public class ACDesignerWindow : EditorWindow
    {
        public static ACDesignerWindow Instance { get; private set; }

        public Vector2 CurrMousePos { get; private set; }
        public float ScreenSizeWidth { get; private set; }
        public float ScreenSizeHeight { get; private set; }
        public bool ScreenSizeChange { get; private set; }

        public event EventHandler LateUpdate;

        [MenuItem("Battle/ACDesigner/Editor  &F2", false, 0)]
        public static void Open()
        {
            if (null == Define.CustomSettings)
            {
                return;
            }

            Instance?.Close();
            Instance = GetWindow<ACDesignerWindow>(false, "Animator Controller");
            Instance.wantsMouseMove = true;
            Instance.minSize = new Vector2(700f, 100f);
            Instance.ScreenSizeWidth = -1;
            Instance.ScreenSizeHeight = -1;

            ACDesignerLuaEnv.Instance.Init();
        }

        public void OnDestroy()
        {
            GraphTopBar.Dispose();
            GraphCtrl.Dispose();
            GraphHelp.Dispose();
            GraphCreate.Dispose();
            GraphPreferences.Dispose();
            ACDesignerLuaEnv.Dispose();
        }

        public void OnEnable()
        {
        }

        public void OnFocus()
        {
        }

        public void Update()
        {
            // Repaint();
        }

        public void OnGUI()
        {
            if (null == Instance)
            {
                Open();
            }

            var width = position.width;
            var height = position.height + 22f;
            if (ScreenSizeWidth != width || ScreenSizeHeight != height)
            {
                ScreenSizeWidth = width;
                ScreenSizeHeight = height;
                ScreenSizeChange = true;
            }
            else
            {
                ScreenSizeChange = false;
            }

            CurrMousePos = Event.current.mousePosition;
            
            GraphCtrl.Instance.OnGUI();
            GraphTopBar.Instance.OnGUI();
            GraphHelp.Instance.OnGUI();
            GraphCreate.Instance.OnGUI();
            GraphPreferences.Instance.OnGUI();
            GraphDebug.Instance.OnGUI();
            GraphTopBar.Instance.OnEvent();
            GraphCtrl.Instance.OnEvent();

            LateUpdate?.Invoke(null, null);
            LateUpdate = null;
        }
    }
}