using UnityEngine;
using System;
using UnityEditor;

namespace ACDesigner
{
    public static class StoragePrefs
    {
        private static string[] _prefString;

        private static string[] PrefString
        {
            get
            {
                if (_prefString == null)
                {
                    _prefString = new string[(int) PrefsType.MaxCount];
                    for (var i = 0; i < _prefString.Length; i++)
                    {
                        _prefString[i] = $"__AIDesigner___{(PrefsType) i}";
                    }
                }

                return _prefString;
            }
        }

        public static void Restore()
        {
            SetPref(PrefsType.CtrlName, GetDefault(PrefsType.CtrlName));
            SetPref(PrefsType.AutoSave, GetDefault(PrefsType.AutoSave));
            SetPref(PrefsType.TopBarIndex, GetDefault(PrefsType.TopBarIndex));
            SetPref(PrefsType.CtrlScrollZoom, GetDefault(PrefsType.CtrlScrollZoom));
            SetPref(PrefsType.CtrlScrollPos, GetDefault(PrefsType.CtrlScrollPos));
            SetPref(PrefsType.CtrlScrollOffset, GetDefault(PrefsType.CtrlScrollOffset));
            SetPref(PrefsType.QuickSearchTaskPanelShortcut, GetDefault(PrefsType.QuickSearchTaskPanelShortcut));
            SetPref(PrefsType.TaskVariableDebug, GetDefault(PrefsType.TaskVariableDebug));
            SetPref(PrefsType.TaskDebugHighlighting, GetDefault(PrefsType.TaskDebugHighlighting));
            SetPref(PrefsType.TaskDebugStopAtTheLastFrame, GetDefault(PrefsType.TaskDebugStopAtTheLastFrame));
            SetPref(PrefsType.QuickLocateToTreeShortcut, GetDefault(PrefsType.QuickLocateToTreeShortcut));
        }

        public static void DrawPref(PrefsType pref, bool autoSave, string text, Action<PrefsType, object> preCallback, Action<PrefsType, object> aftCallback)
        {
            var type = GetPrefType(pref);
            var value = GetPref(pref);

            object flag = null;
            if (typeof(int) == type)
            {
                flag = EditorGUILayout.IntField(text, (int) value);
            }
            else if (typeof(float) == type)
            {
                flag = EditorGUILayout.FloatField(text, (float) value);
            }
            else if (typeof(string) == type)
            {
                flag = EditorGUILayout.TextField(text, (string) value);
            }
            else if (typeof(bool) == type)
            {
                flag = GUILayout.Toggle((bool) value, text);
            }
            else if (typeof(Vector2) == type)
            {
                flag = EditorGUILayout.Vector2Field(text, (Vector2) value);
            }

            if (!value.Equals(flag))
            {
                preCallback?.Invoke(pref, value);
                if (autoSave)
                {
                    SetPref(pref, flag);
                }

                aftCallback?.Invoke(pref, flag);
            }
        }

        public static void DrawPopupPref(PrefsType pref, bool autoSave, Type eType, string text, Action<PrefsType, object> preCallback, Action<PrefsType, object> aftCallback)
        {
            var @string = (string) GetPref(pref);

            var selectedIndex = 0;
            var options = Enum.GetNames(eType);
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == @string)
                {
                    selectedIndex = i;
                    break;
                }
            }

            var flag = EditorGUILayout.Popup(text, selectedIndex, options);
            if (flag != selectedIndex)
            {
                preCallback?.Invoke(pref, @string);
                if (autoSave)
                {
                    SetPref(pref, options[flag]);
                }

                aftCallback?.Invoke(pref, options[flag]);
            }
        }

        public static bool HasPref(PrefsType pref)
        {
            return EditorPrefs.HasKey(PrefString[(int) pref]);
        }

        public static object GetPref(PrefsType pref)
        {
            if (!HasPref(pref))
            {
                return GetDefault(pref);
            }

            var type = GetPrefType(pref);
            if (typeof(int) == type)
            {
                return EditorPrefs.GetInt(PrefString[(int) pref]);
            }
            else if (typeof(float) == type)
            {
                return EditorPrefs.GetFloat(PrefString[(int) pref]);
            }
            else if (typeof(string) == type)
            {
                return EditorPrefs.GetString(PrefString[(int) pref]);
            }
            else if (typeof(bool) == type)
            {
                return EditorPrefs.GetBool(PrefString[(int) pref]);
            }
            else if (typeof(Vector2) == type)
            {
                var v2 = EditorPrefs.GetString(PrefString[(int) pref]).Split('=');
                return new Vector2(float.Parse(v2[0]), float.Parse(v2[1]));
            }
            else if (typeof(Vector3) == type)
            {
                var v3 = EditorPrefs.GetString(PrefString[(int) pref]).Split('=');
                return new Vector3(float.Parse(v3[0]), float.Parse(v3[1]), float.Parse(v3[2]));
            }

            return null;
        }

        public static void SetPref(PrefsType pref, object value)
        {
            var type = GetPrefType(pref);
            if (typeof(int) == type)
            {
                EditorPrefs.SetInt(PrefString[(int) pref], (int) value);
            }
            else if (typeof(float) == type)
            {
                EditorPrefs.SetFloat(PrefString[(int) pref], (float) value);
            }
            else if (typeof(string) == type)
            {
                EditorPrefs.SetString(PrefString[(int) pref], (string) value);
            }
            else if (typeof(bool) == type)
            {
                EditorPrefs.SetBool(PrefString[(int) pref], (bool) value);
            }
            else if (typeof(Vector2) == type)
            {
                var v2 = (Vector2) value;
                EditorPrefs.SetString(PrefString[(int) pref], $"{v2.x}={v2.y}");
            }
            else if (typeof(Vector3) == type)
            {
                var v3 = (Vector3) value;
                EditorPrefs.SetString(PrefString[(int) pref], $"{v3.x}={v3.y}={v3.z}");
            }
        }

        public static object GetDefault(PrefsType pref)
        {
            switch (pref)
            {
                case PrefsType.CtrlName: return null;
                case PrefsType.AutoSave: return true;
                case PrefsType.TopBarIndex: return 1;
                case PrefsType.CtrlScrollZoom: return 1f;
                case PrefsType.CtrlScrollPos:
                case PrefsType.CtrlScrollOffset: return Vector2.zero;
                case PrefsType.QuickSearchTaskPanelShortcut: return KeyCode.Space.ToString();
                case PrefsType.UsedForType: return UsedForType.Lua.ToString();
                case PrefsType.TaskVariableDebug: return true;
                case PrefsType.TaskDebugHighlighting: return 2f;
                case PrefsType.TaskDebugStopAtTheLastFrame: return true;
                case PrefsType.QuickLocateToTreeShortcut: return KeyCode.L.ToString();
            }

            return null;
        }

        public static Type GetPrefType(PrefsType pref)
        {
            switch (pref)
            {
                case PrefsType.CtrlName: return typeof(string);
                case PrefsType.AutoSave: return typeof(bool);
                case PrefsType.TopBarIndex: return typeof(int);
                case PrefsType.CtrlScrollPos: return typeof(Vector2);
                case PrefsType.CtrlScrollZoom: return typeof(float);
                case PrefsType.CtrlScrollOffset: return typeof(Vector2);
                case PrefsType.ConfigPath: return typeof(string);
                case PrefsType.EditorConfigPath: return typeof(string);
                case PrefsType.QuickSearchTaskPanelShortcut: return typeof(string);
                case PrefsType.TaskVariableDebug: return typeof(bool);
                case PrefsType.UsedForType: return typeof(string);
                case PrefsType.TreeDirectory: return typeof(string);
                case PrefsType.TaskDebugHighlighting: return typeof(float);
                case PrefsType.TaskDebugStopAtTheLastFrame: return typeof(bool);
                case PrefsType.QuickLocateToTreeShortcut: return typeof(string);
            }

            return null;
        }
    }
}