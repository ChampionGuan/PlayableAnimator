using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ACDesigner
{
    public class TestLayer
    {
        public string m_Name;
        public float m_Weight = 0.5f;
        public AnimationBlendMode m_Blend;
        public AvatarMask m_Mask;
        public bool m_Selected;
        public float PosMaxY;

        public TestLayer(string name)
        {
            m_Name = name;
        }
    }

    public class GraphLayers : Singleton<GraphLayers>
    {
        private float m_headerPosY;
        private float m_weightBoxWidth = 255f;
        private Vector2 m_scrollPosition;
        private GenericMenu m_rightMouseDownMenu;

        private TestLayer m_deletedLayer;
        private TestLayer m_moveDownLayer;
        private TestLayer m_moveUpLayer;
        private TestLayer m_copyLayer;
        private List<TestLayer> m_testList = new List<TestLayer>();

        public void OnGUI()
        {
            m_deletedLayer = null;
            m_moveDownLayer = null;
            m_moveUpLayer = null;

            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
            DrawHeader();
            DrawLayers();
            Delete(m_deletedLayer);
            MoveDown(m_moveDownLayer);
            MoveUp(m_moveUpLayer);
            GUILayout.EndScrollView();
        }

        public bool OnLeftMouseDown(Vector2 mousePos)
        {
            TestLayer layer = null;
            foreach (var value in m_testList)
            {
                if (mousePos.y > m_headerPosY && mousePos.y < value.PosMaxY - m_scrollPosition.y)
                {
                    if (value.m_Selected)
                    {
                        return false;
                    }

                    layer = value;
                    break;
                }
            }

            Selected(layer);
            return true;
        }

        public bool OnRightMouseDown(Vector2 mousePos)
        {
            var result = false;

            for (var i = 0; i < m_testList.Count; i++)
            {
                if (mousePos.y > m_headerPosY && mousePos.y < m_testList[i].PosMaxY - m_scrollPosition.y)
                {
                    m_rightMouseDownMenu = new GenericMenu();
                    if (i > 0)
                    {
                        m_rightMouseDownMenu.AddItem(new GUIContent("Move Up"), false, (var) => { MoveUp(var as TestLayer); }, m_testList[i]);
                    }

                    if (i < m_testList.Count - 1)
                    {
                        m_rightMouseDownMenu.AddItem(new GUIContent("Move Down"), false, (var) => { MoveDown(var as TestLayer); }, m_testList[i]);
                    }

                    m_rightMouseDownMenu.AddSeparator(string.Empty);
                    m_rightMouseDownMenu.AddItem(new GUIContent("Copy"), false, (var) => { m_copyLayer = var as TestLayer; }, m_testList[i]);
                    if (null == m_copyLayer)
                    {
                        m_rightMouseDownMenu.AddDisabledItem(new GUIContent("Paste"));
                    }
                    else
                    {
                        m_rightMouseDownMenu.AddItem(new GUIContent("Paste"), false, () => { Paste(m_copyLayer); });
                    }

                    m_rightMouseDownMenu.AddSeparator(string.Empty);
                    m_rightMouseDownMenu.AddItem(new GUIContent("Delete"), false, (var) => { Delete(var as TestLayer); }, m_testList[i]);

                    m_rightMouseDownMenu.ShowAsContext();
                    result = true;
                    break;
                }
            }

            if (!result && null != m_copyLayer)
            {
                m_rightMouseDownMenu = new GenericMenu();
                m_rightMouseDownMenu.AddItem(new GUIContent("Paste Layer"), false, () => { Paste(m_copyLayer); });
                m_rightMouseDownMenu.ShowAsContext();
                result = true;
            }

            return result;
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(ACDesignerUIUtility.PlusSignTexture, ACDesignerUIUtility.TransparentGUIStyle, GUILayout.Width(16f), GUILayout.Height(16f)))
            {
                var index = 0;
                var name = string.Empty;
                while (!IsValidName(name))
                {
                    name = "New Layer " + index;
                    index++;
                }

                var layer = new TestLayer(name);
                m_testList.Add(layer);
                Selected(layer);
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            ACDesignerUIUtility.DrawContentSeperator();
            m_headerPosY = GUILayoutUtility.GetLastRect().yMax;
        }

        private void DrawLayers()
        {
            if (m_testList.Count < 1)
            {
                GUILayout.Label("List is Empty.", ACDesignerUIUtility.LabelWrapGUIStyle, GUILayout.Width(285f));
                return;
            }

            for (int index = 0; index < m_testList.Count; index++)
            {
                GUILayout.Space(4f);
                if (m_testList[index].m_Selected)
                {
                    GUILayout.BeginVertical(ACDesignerUIUtility.SelectedBackgroundGUIStyle);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name", GUILayout.Width(70f));
                    var name = EditorGUILayout.TextField(m_testList[index].m_Name, GUILayout.Width(140f));
                    if (name != m_testList[index].m_Name && IsValidName(name))
                    {
                        m_testList[index].m_Name = name;
                    }

                    GUILayout.FlexibleSpace();
                    if (index < m_testList.Count - 1 && GUILayout.Button(ACDesignerUIUtility.DownArrowButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_moveDownLayer = m_testList[index];
                    }

                    if (index > 0 && GUILayout.Button(ACDesignerUIUtility.UpArrowButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_moveUpLayer = m_testList[index];
                    }

                    if (GUILayout.Button(ACDesignerUIUtility.VariableDeleteButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_deletedLayer = m_testList[index];
                    }

                    GUILayout.Space(12f);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2f);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Weight", GUILayout.Width(70f));
                    var weight = EditorGUILayout.Slider(m_testList[index].m_Weight, 0, 1);
                    if (weight != m_testList[index].m_Weight)
                    {
                        m_testList[index].m_Weight = weight;
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Blending", GUILayout.Width(70f));
                    var blending = (AnimationBlendMode) EditorGUILayout.EnumPopup(m_testList[index].m_Blend);
                    if (blending != m_testList[index].m_Blend)
                    {
                        m_testList[index].m_Blend = blending;
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.Space(3f);
                    ACDesignerUIUtility.DrawContentSeperator(2, 300);
                    GUILayout.EndVertical();
                }
                else
                {
                    ACDesignerUIUtility.DrawBox(3f, GUILayoutUtility.GetLastRect().yMax + 7f, 12f, 1.5f, ACDesignerUIUtility.GrayGUIStyle);
                    ACDesignerUIUtility.DrawBox(3f, GUILayoutUtility.GetLastRect().yMax + 11f, 12f, 1.5f, ACDesignerUIUtility.GrayGUIStyle);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    GUILayout.Label(m_testList[index].m_Name);
                    GUILayout.FlexibleSpace();
                    if (index > 0 && GUILayout.Button(ACDesignerUIUtility.VariableDeleteButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_deletedLayer = m_testList[index];
                    }

                    GUILayout.Space(12f);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5f);
                    ACDesignerUIUtility.DrawBox(25f, GUILayoutUtility.GetLastRect().yMax, m_weightBoxWidth, 2f, ACDesignerUIUtility.BlackGUIStyle);
                    ACDesignerUIUtility.DrawBox(25f, GUILayoutUtility.GetLastRect().yMax, m_testList[index].m_Weight * m_weightBoxWidth, 2f, ACDesignerUIUtility.GrayGUIStyle);
                    GUILayout.Space(8f);
                    ACDesignerUIUtility.DrawContentSeperator(2, 300);
                }

                m_testList[index].PosMaxY = GUILayoutUtility.GetLastRect().yMax + 7;
            }
        }

        private void Selected(TestLayer layer)
        {
            foreach (var value in m_testList)
            {
                value.m_Selected = value == layer;
            }
        }

        private void MoveDown(TestLayer layer)
        {
            if (null == layer)
            {
                return;
            }

            for (var i = m_testList.Count - 1; i >= 0; i--)
            {
                if (layer == m_testList[i])
                {
                    if (i == m_testList.Count - 1)
                    {
                        break;
                    }

                    var temp = m_testList[i + 1];
                    m_testList[i + 1] = layer;
                    m_testList[i] = temp;
                    break;
                }
            }
        }

        private void MoveUp(TestLayer layer)
        {
            if (null == layer)
            {
                return;
            }

            for (var i = 0; i < m_testList.Count; i++)
            {
                if (layer == m_testList[i])
                {
                    if (i == 0)
                    {
                        break;
                    }

                    var temp = m_testList[i - 1];
                    m_testList[i - 1] = layer;
                    m_testList[i] = temp;
                    break;
                }
            }
        }

        private void Delete(TestLayer layer)
        {
            if (null == layer)
            {
                return;
            }

            m_testList.Remove(layer);
        }

        private void Paste(TestLayer layer)
        {
            if (null == layer)
            {
                return;
            }
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name) && null == m_testList.Find(x => x.m_Name == name);
        }
    }
}