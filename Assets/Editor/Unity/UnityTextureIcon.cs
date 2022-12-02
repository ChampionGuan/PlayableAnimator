using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;

public class UnityTextureIcon : EditorWindow
{
    [MenuItem(("Tools/UnityTextureIcon"))]
    static void Init()
    {
        GetWindow<UnityTextureIcon>("UnityTextureIcon");
    }

    Vector2 m_Scroll;
    List<string> m_Icons = null;

    private void Awake()
    {
        string line;
        m_Icons = new List<string>();
        var sr = new StreamReader("Assets/Resources/icon.txt", Encoding.UTF8);
        while ((line = sr.ReadLine()) != null)
        {
            line = line.Replace("\t", "");
            line = line.Substring(0, line.Length - 19);
            if (line.EndsWith("-"))
            {
                line = line.Substring(0, line.Length - 1);
            }

            line.TrimStart();
            line.TrimEnd();

            m_Icons.Add(line);
        }
    }

    void OnGUI()
    {
        m_Scroll = GUILayout.BeginScrollView(m_Scroll);
        for (var i = 0; i < m_Icons.Count; i += 4)
        {
            GUILayout.BeginHorizontal();
            for (var j = 0; j < 4; j++)
            {
                var index = i * 4 + j;
                if (index >= m_Icons.Count)
                {
                    break;
                }

                GUILayout.Button(EditorGUIUtility.IconContent(m_Icons[index]));
                EditorGUILayout.TextField(m_Icons[index], GUI.skin.label);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}