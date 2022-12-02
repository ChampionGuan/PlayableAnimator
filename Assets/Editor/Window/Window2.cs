using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Window2 : EditorWindow
{
    private static Window2 _Instance;

    public static Window2 Instance
    {
        get
        {
            if (null == _Instance)
            {
                _Instance = EditorWindow.GetWindow<Window2>(false, "Window2");
                _Instance.wantsMouseMove = true;
                _Instance.minSize = new Vector2(700f, 100f);
            }

            return _Instance;
        }
    }
    public void Test()
    {
        
    }

}