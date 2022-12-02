using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ACDesigner
{
    public enum ParameterType
    {
        Float = 0,
        Int = 1,
        Bool = 2,
        Trigger = 3,
    }

    public class TestParameter
    {
        public string m_Name;
        public ParameterType m_Type;
        public bool m_BoolValue;
        public int m_IntValue;
        public float m_FloatValue;

        public float m_Weight = 0.5f;
        public AnimationBlendMode m_Blend;
        public AvatarMask m_Mask;
        public bool m_Selected;
        public float PosMaxY;

        public TestParameter(string name, ParameterType type)
        {
            m_Name = name;
            m_Type = type;
        }
    }

    public class GraphParameters : Singleton<GraphParameters>
    {
        private float m_headerPosY;
        private float m_weightBoxWidth = 255f;
        private Vector2 m_scrollPosition;
        private GenericMenu m_addBtnDownMenu;
        private GenericMenu m_rightMouseDownMenu;

        private TestParameter m_deletedParameter;
        private TestParameter m_moveDownParameter;
        private TestParameter m_moveUpParameter;
        private TestParameter m_copyParameter;
        private List<TestParameter> m_testList = new List<TestParameter>();

        protected override void OnInstance()
        {
            m_addBtnDownMenu = new GenericMenu();
            m_addBtnDownMenu.AddItem(new GUIContent("Float"), false, AddParameter, ParameterType.Float);
            m_addBtnDownMenu.AddItem(new GUIContent("Int"), false, AddParameter, ParameterType.Int);
            m_addBtnDownMenu.AddItem(new GUIContent("Bool"), false, AddParameter, ParameterType.Bool);
            m_addBtnDownMenu.AddItem(new GUIContent("Trigger"), false, AddParameter, ParameterType.Trigger);
        }

        public void OnGUI()
        {
            m_deletedParameter = null;
            m_moveDownParameter = null;
            m_moveUpParameter = null;

            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
            DrawHeader();
            DrawParameters();
            Delete(m_deletedParameter);
            MoveDown(m_moveDownParameter);
            MoveUp(m_moveUpParameter);
            GUILayout.EndScrollView();
        }

        public bool OnLeftMouseDown(Vector2 mousePos)
        {
            TestParameter Parameter = null;
            foreach (var value in m_testList)
            {
                if (mousePos.y > m_headerPosY && mousePos.y < value.PosMaxY - m_scrollPosition.y)
                {
                    if (value.m_Selected)
                    {
                        return false;
                    }

                    Parameter = value;
                    break;
                }
            }

            Selected(Parameter);
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
                        m_rightMouseDownMenu.AddItem(new GUIContent("Move Up"), false, (var) => { MoveUp(var as TestParameter); }, m_testList[i]);
                    }

                    if (i < m_testList.Count - 1)
                    {
                        m_rightMouseDownMenu.AddItem(new GUIContent("Move Down"), false, (var) => { MoveDown(var as TestParameter); }, m_testList[i]);
                    }

                    m_rightMouseDownMenu.AddSeparator(string.Empty);
                    m_rightMouseDownMenu.AddItem(new GUIContent("Copy"), false, (var) => { m_copyParameter = var as TestParameter; }, m_testList[i]);
                    if (null == m_copyParameter)
                    {
                        m_rightMouseDownMenu.AddDisabledItem(new GUIContent("Paste"));
                    }
                    else
                    {
                        m_rightMouseDownMenu.AddItem(new GUIContent("Paste"), false, () => { Paste(m_copyParameter); });
                    }

                    m_rightMouseDownMenu.AddSeparator(string.Empty);
                    m_rightMouseDownMenu.AddItem(new GUIContent("Delete"), false, (var) => { Delete(var as TestParameter); }, m_testList[i]);

                    m_rightMouseDownMenu.ShowAsContext();
                    result = true;
                    break;
                }
            }

            if (!result && null != m_copyParameter)
            {
                m_rightMouseDownMenu = new GenericMenu();
                m_rightMouseDownMenu.AddItem(new GUIContent("Paste Parameter"), false, () => { Paste(m_copyParameter); });
                m_rightMouseDownMenu.ShowAsContext();
                result = true;
            }

            return result;
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(ACDesignerUIUtility.PlusArrowSignTexture, ACDesignerUIUtility.TransparentGUIStyle, GUILayout.Width(32f), GUILayout.Height(16f)))
            {
                m_addBtnDownMenu.ShowAsContext();
            }

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            ACDesignerUIUtility.DrawContentSeperator();
            m_headerPosY = GUILayoutUtility.GetLastRect().yMax;
        }

        private void DrawParameters()
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
                        m_moveDownParameter = m_testList[index];
                    }

                    if (index > 0 && GUILayout.Button(ACDesignerUIUtility.UpArrowButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_moveUpParameter = m_testList[index];
                    }

                    if (GUILayout.Button(ACDesignerUIUtility.VariableDeleteButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_deletedParameter = m_testList[index];
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
                    switch (m_testList[index].m_Type)
                    {
                        case ParameterType.Bool:
                            m_testList[index].m_BoolValue = EditorGUILayout.Toggle(new GUIContent(m_testList[index].m_Name), m_testList[index].m_BoolValue);
                            break;
                        case ParameterType.Float:
                            m_testList[index].m_FloatValue = EditorGUILayout.FloatField(new GUIContent(m_testList[index].m_Name), m_testList[index].m_FloatValue);
                            break;
                        case ParameterType.Int:
                            m_testList[index].m_IntValue = EditorGUILayout.IntField(new GUIContent(m_testList[index].m_Name), m_testList[index].m_IntValue);
                            break;
                        case ParameterType.Trigger:
                            m_testList[index].m_BoolValue = EditorGUILayout.Toggle(new GUIContent(m_testList[index].m_Name), m_testList[index].m_BoolValue);
                            break;
                    }

                    GUILayout.FlexibleSpace();
                    if (index > 0 && GUILayout.Button(ACDesignerUIUtility.VariableDeleteButtonTexture, ACDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_deletedParameter = m_testList[index];
                    }

                    GUILayout.Space(12f);
                    GUILayout.EndHorizontal();

                    ACDesignerUIUtility.DrawContentSeperator(2, 300);
                }

                m_testList[index].PosMaxY = GUILayoutUtility.GetLastRect().yMax + 7;
            }
        }

        private void AddParameter(object obj)
        {
            var tag = "";
            var type = (ParameterType) obj;
            switch (type)
            {
                case ParameterType.Bool:
                    tag = "New Bool ";
                    break;
                case ParameterType.Float:
                    tag = "New Float ";
                    break;
                case ParameterType.Int:
                    tag = "New Int ";
                    break;
                case ParameterType.Trigger:
                    tag = "New Trigger ";
                    break;
            }

            var index = 0;
            var name = string.Empty;
            while (!IsValidName(name))
            {
                name = tag + index;
                index++;
            }

            var parameter = new TestParameter(name, type);
            m_testList.Add(parameter);
            Selected(parameter);
        }

        private void Selected(TestParameter parameter)
        {
            foreach (var value in m_testList)
            {
                value.m_Selected = value == parameter;
            }
        }

        private void MoveDown(TestParameter parameter)
        {
            if (null == parameter)
            {
                return;
            }

            for (var i = m_testList.Count - 1; i >= 0; i--)
            {
                if (parameter == m_testList[i])
                {
                    if (i == m_testList.Count - 1)
                    {
                        break;
                    }

                    var temp = m_testList[i + 1];
                    m_testList[i + 1] = parameter;
                    m_testList[i] = temp;
                    break;
                }
            }
        }

        private void MoveUp(TestParameter parameter)
        {
            if (null == parameter)
            {
                return;
            }

            for (var i = 0; i < m_testList.Count; i++)
            {
                if (parameter == m_testList[i])
                {
                    if (i == 0)
                    {
                        break;
                    }

                    var temp = m_testList[i - 1];
                    m_testList[i - 1] = parameter;
                    m_testList[i] = temp;
                    break;
                }
            }
        }

        private void Delete(TestParameter parameter)
        {
            if (null == parameter)
            {
                return;
            }

            m_testList.Remove(parameter);
        }

        private void Paste(TestParameter parameter)
        {
            if (null == parameter)
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