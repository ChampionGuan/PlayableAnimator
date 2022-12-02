using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace ACDesigner
{
    public class GraphTopBar : Singleton<GraphTopBar>
    {
        private readonly string[] m_topBarStrings = new string[2] {"Layers", "Parameters"};

        private Rect m_graphLeftTopBarRect;
        private Rect m_graphDetailRect;
        private Rect m_graphRightTopBarRect;

        private int m_topBarMenuIndex = 1;
        private ToggleGraph m_toggleGraphs = new ToggleGraph();

        public int TopBarMenuIndex
        {
            get => m_topBarMenuIndex;
            private set
            {
                m_topBarMenuIndex = value;
                StoragePrefs.SetPref(PrefsType.TopBarIndex, value);
            }
        }

        protected override void OnInstance()
        {
            m_topBarMenuIndex = (int) StoragePrefs.GetPref(PrefsType.TopBarIndex);
            m_toggleGraphs.Add(GraphHelp.Instance);
            m_toggleGraphs.Add(GraphCreate.Instance);
            m_toggleGraphs.Add(GraphPreferences.Instance);
        }

        public void OnGUI()
        {
            if (ACDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphLeftTopBarRect = new Rect(0f, 0f, 300f, 18f);
                m_graphDetailRect = new Rect(0f, m_graphLeftTopBarRect.height, 300f, ACDesignerWindow.Instance.ScreenSizeHeight - m_graphLeftTopBarRect.height - 18);
                m_graphRightTopBarRect = new Rect(300f, 0f, ACDesignerWindow.Instance.ScreenSizeWidth - 300f, 18);
            }

            GUILayout.BeginArea(m_graphLeftTopBarRect, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            var index = GUILayout.Toolbar(TopBarMenuIndex, m_topBarStrings, EditorStyles.toolbarButton);
            if (index != TopBarMenuIndex)
            {
                TopBarMenuIndex = index;
            }

            GUILayout.Space(150f);
            GUILayout.EndHorizontal();
            ACDesignerUIUtility.DrawContentSeperator();
            GUILayout.EndArea();

            GUILayout.BeginArea(m_graphDetailRect, ACDesignerUIUtility.PropertyBoxGUIStyle);
            if (TopBarMenuIndex == 0)
            {
                GraphLayers.Instance.OnGUI();
            }
            else if (TopBarMenuIndex == 1)
            {
                GraphParameters.Instance.OnGUI();
            }

            GUILayout.EndArea();

            GUILayout.BeginArea(m_graphRightTopBarRect, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(ACDesignerUIUtility.HistoryBackwardTexture, EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
            }

            if (GUILayout.Button(ACDesignerUIUtility.HistoryForwardTexture, EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
            }

            if (GUILayout.Button("...", EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
            }

            if (GUILayout.Button("(None Selected)", EditorStyles.toolbarPopup, GUILayout.Width(140f)))
            {
            }

            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
            }

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
                m_toggleGraphs.Toggle(GraphCreate.Instance);
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(42f)))
            {
            }

            if (GUILayout.Button("Help", EditorStyles.toolbarButton, GUILayout.Width(40f)))
            {
                m_toggleGraphs.Toggle(GraphHelp.Instance);
            }

            if (GUILayout.Button(ACDesignerUIUtility.LocationTexture, EditorStyles.toolbarButton, GUILayout.Width(20f)))
            {
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Preferences", EditorStyles.toolbarButton, GUILayout.Width(80f)))
            {
                m_toggleGraphs.Toggle(GraphPreferences.Instance);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public void OnEvent()
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.Repaint || currentEvent.type == EventType.Layout)
            {
                return;
            }

            var mousePos = Vector2.zero;
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (!IsThePointInGraph(out mousePos))
                    {
                        break;
                    }

                    if (currentEvent.button == 0 && !currentEvent.control)
                    {
                        if (TopBarMenuIndex == 0 && GraphLayers.Instance.OnLeftMouseDown(mousePos))
                        {
                            currentEvent.Use();
                        }
                        else if (TopBarMenuIndex == 1 && GraphParameters.Instance.OnLeftMouseDown(mousePos))
                        {
                            currentEvent.Use();
                        }
                    }
                    else if (currentEvent.button == 1)
                    {
                        if (TopBarMenuIndex == 0 && GraphLayers.Instance.OnRightMouseDown(mousePos))
                        {
                            currentEvent.Use();
                        }
                        else if (TopBarMenuIndex == 1 && GraphParameters.Instance.OnRightMouseDown(mousePos))
                        {
                            currentEvent.Use();
                        }
                    }

                    break;
            }
        }

        private bool IsThePointInGraph(out Vector2 point)
        {
            point = ACDesignerWindow.Instance.CurrMousePos;
            if (!m_graphDetailRect.Contains(point))
            {
                return false;
            }

            point.x -= m_graphDetailRect.xMin;
            point.y -= m_graphDetailRect.yMin;
            return true;
        }
    }
}