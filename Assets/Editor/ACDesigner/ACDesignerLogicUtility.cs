using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;

namespace ACDesigner
{
    public static class ACDesignerLogicUtility
    {
        public static string LuaFilePathLegalization(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return path.Replace(@"/", ".").Replace(@"\", ".");
        }

        public static List<string> GetAllFilesPathInDir(string path, string suffix)
        {
            List<string> filePathList = new List<string>();
            if (!Directory.Exists(path))
            {
                return filePathList;
            }

            filePathList.AddRange(Directory.GetFiles(path, suffix));

            string[] subDirs = Directory.GetDirectories(path);
            foreach (string subPath in subDirs)
            {
                filePathList.AddRange(GetAllFilesPathInDir(subPath, suffix));
            }

            return filePathList;
        }

        public static List<string> GetAllFilesNameInDir(string path, string suffix)
        {
            List<string> filePathList = GetAllFilesPathInDir(path, suffix);
            List<string> fileNameList = new List<string>();
            foreach (var filePath in filePathList)
            {
                fileNameList.Add(GetFileNameWithoutSuffix(filePath));
            }

            return fileNameList;
        }

        public static string GetFileNameWithoutSuffix(string path, char separator = '/')
        {
            string name = path;
            if (name.Contains("/"))
            {
                name = name.Substring(name.LastIndexOf(separator) + 1);
            }

            if (name.Contains("."))
            {
                return name.Substring(0, name.LastIndexOf("."));
            }
            else
            {
                return name;
            }
        }

        public static string StringReplace(string str, string f, string t)
        {
            return str.Replace(f, t);
        }

        public static float CalcMaxWidth(string str)
        {
            float max = 0;
            if (string.IsNullOrEmpty(str))
            {
                return max;
            }

            string[] splits = str.Split('\n');
            foreach (var v in splits)
            {
                string[] splits2 = v.Split('\r');
                foreach (var v2 in splits2)
                {
                    if (v2.Length > max)
                    {
                        max = v2.Length;
                    }
                }
            }

            return max;
        }

        public static string FileRead(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var content = string.Empty;
            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            {
                content = sr.ReadToEnd();
            }

            return content;
        }

        public static string BuildMD5ByFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var sb = new StringBuilder();
            var fs = new FileStream(filePath, FileMode.Open);
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(fs);
            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            fs.Close();
            fs.Dispose();

            return sb.ToString();
        }

        public static string BuildMD5ByBytes(byte[] value)
        {
            if (null == value)
            {
                return null;
            }

            var destString = "";
            var md5 = new MD5CryptoServiceProvider();
            var md5Data = md5.ComputeHash(value, 0, value.Length);
            md5.Clear();
            for (var i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }

            return destString.PadLeft(32, '0');
        }

        public static string BuildMD5ByString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return BuildMD5ByBytes(Encoding.UTF8.GetBytes(value));
        }

        public static string ToUpperFirst(string str)
        {
            var a = str.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static string ToReplaceEntersymbol(string str)
        {
            return string.IsNullOrEmpty(str) ? null : str.Replace(System.Convert.ToChar(10).ToString(), "\\n").Replace(System.Convert.ToChar(13).ToString(), "\\n");
        }

        public static bool IsOnlyLetters(string str)
        {
            return Regex.IsMatch(str, @"^[A-Za-z]+$");
        }

        public static bool IsStartWithNumber(string str)
        {
            return Regex.IsMatch(str, @"^\d+[\w\W]*$");
        }

        public static bool IsEditorForBattle()
        {
            return (string)StoragePrefs.GetPref(PrefsType.UsedForType) == UsedForType.Lua.ToString();
        }

        public static void OpenConfigPathFolder(ref string openDirectory)
        {
            var rootConfigPath = $"{Application.dataPath}/{Define.ConfigFullPath}";
            var rootEditorConfigPath = $"{Application.dataPath}/{Define.EditorConfigFullPath}";

            var fromPath = $"{rootConfigPath}{openDirectory}";
            var toPath = $"{EditorUtility.OpenFolderPanel("Config Path", fromPath, "")}/";

            if (toPath.Contains(rootConfigPath) && !toPath.Contains(rootEditorConfigPath))
            {
                var directory = toPath.Replace(rootConfigPath, "");
                if (!directory.Contains("."))
                {
                    openDirectory = directory;
                }
            }

            if (!string.IsNullOrEmpty(openDirectory))
            {
                var editorConfigPath = $"{Application.dataPath}/{Define.EditorConfigFullPath}{openDirectory}";
                if (!Directory.Exists(editorConfigPath))
                {
                    Directory.CreateDirectory(editorConfigPath);
                }
            }
        }
    }
}