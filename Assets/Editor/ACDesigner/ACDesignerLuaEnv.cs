using UnityEngine;
using XLua;
using System.IO;
using System;

namespace ACDesigner
{
    public class ACDesignerLuaEnv : Singleton<ACDesignerLuaEnv>
    {
        LuaEnv luaEnv;

        public void Init()
        {
            luaEnv?.Dispose();
            luaEnv = new LuaEnv();
            luaEnv.AddLoader(LoadLua);
        }

        public object[] DoString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return luaEnv.DoString(str);
        }

        protected override void OnDispose()
        {
            luaEnv?.Dispose();
            luaEnv = null;
        }

        private byte[] LoadLua(ref string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            try
            {
                string luaPath = "";
                byte[] luaBytes = null;

                filePath = filePath.ToLower();
                filePath = filePath.Replace(@".", "/");
                filePath = filePath.Replace(@"\", "/");
                luaPath = filePath + ".lua";

                luaBytes = ReadFileToBytes($"{Application.dataPath}/{Define.CustomSettings.LuaRootPath}{luaPath}");
                if (null == luaBytes) luaBytes = ReadFileToBytes($"{Application.dataPath}/{luaPath}");

                return luaBytes;
            }
            catch
            {
                return null;
            }
        }

        private byte[] ReadFileToBytes(string path)
        {
            path = path.Replace(@"//", "/");
            if (!File.Exists(path))
            {
                //Debug.Log("[warning] 文件不存在: " + path);
                return null;
            }

            byte[] bytes = null;
            try
            {
                //using (FileStream fs = File.Open(path, FileMode.Open))
                //{
                //    bytes = new byte[fs.Length];
                //    fs.Read(bytes, 0, bytes.Length);
                //}
                bytes = System.Text.Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning(path + "[error] 读取文件错误: " + e);
                return null;
            }

            return bytes;
        }
    }
}