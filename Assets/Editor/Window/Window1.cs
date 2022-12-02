using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public class Window1 : EditorWindow
{
    private static Type DockAreaType = typeof(Editor).Assembly.GetType("UnityEditor.DockArea");
    private static MethodInfo AddTabMethod = typeof(Editor).Assembly.GetType("UnityEditor.DockArea").GetMethod("AddTab", new Type[] {typeof(EditorWindow), typeof(bool)});
    private static FieldInfo ParentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);

    private static EditorWindow win1;
    private static EditorWindow win2;

    [MenuItem("Tools/WindowAddTab", false, 0)]
    public static void Open()
    {
        win1 = EditorWindow.GetWindow<Window1>(false, "Window1");
        win2 = EditorWindow.GetWindow<Window2>(false, "Window2");

        // AddTabMethod.Invoke(ParentField.GetValue(EditorWindow.GetWindow<Window1>(false, "Window1")), new object[] {CreateInstance(typeof(Window2)), false});
    }

    
    private static MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetGetMethod(true);

    private void OnGUI()
    {
        if ((bool) isDockedMethod.Invoke(win1, null) == false && (bool) isDockedMethod.Invoke(win2, null) == false)
        {
            win1.DockWindow(win2, Docker.DockPosition.Right);
        }
    }
}