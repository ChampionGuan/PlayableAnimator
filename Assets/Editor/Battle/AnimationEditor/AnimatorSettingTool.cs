using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorSettingTool
{
    static string templateAnimatorPath = Application.dataPath + "/Editor/Battle/AnimationEditor/Template/T_Effect.controller";

    public static void OnAssignAnimation()
    {
        string[] assetGUIDs = Selection.assetGUIDs;
        for (int i = 0; i < assetGUIDs.Length; ++i)
        {
            string selectPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

            string dataPath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));//不要Assets
            string selectDic = selectPath.Substring(0, selectPath.LastIndexOf("/") + 1);//去掉后面的文件名 只留path
            string fileName = FilePathToSimpleName(selectPath);
            fileName = fileName.Substring(0, fileName.LastIndexOf("_")) + ".controller";
            string newPath = dataPath + selectDic + fileName;

            File.Copy(templateAnimatorPath, newPath, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //Debug.LogError(selectDic + fileName);
            AnimatorController ac = AssetDatabase.LoadMainAssetAtPath(selectDic + fileName) as AnimatorController;
            AssignAnimation(selectPath, ac);
        }
    }

    public static void AssignAnimation(string aniCtlPath, AnimatorController ac)
    {
        if (ac == null)
        {
            Debug.LogErrorFormat("file is not a AnimatorController");
            return;
        }
        string animResPath = FilePathToDirPath(aniCtlPath);
        Debug.Log("Select Animator: " + ac.name);

        //获得合适的Animation
        string[] curTarFiles = GetDirFiles(animResPath, "anim");
        List<string> tarFiles = new List<string>();
        tarFiles.AddRange(curTarFiles);
        Dictionary<string, AnimationClip> stateToAniClip = new Dictionary<string, AnimationClip>();
        Debug.LogFormat("has found animation file:{0}", tarFiles.Count);
        for (int i = 0; i < tarFiles.Count; ++i)
        {
            string aniFilePath = tarFiles[i];
            string aniFileName = FilePathToSimpleName(aniFilePath);//前面的名字
            string aniStateName = GetStateName(aniFileName);//获得最后的名字

            AnimationClip clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(aniFilePath, typeof(AnimationClip));
            if (aniFileName.Contains(ac.name))
            {
                stateToAniClip[aniStateName] = clip;
                Debug.LogFormat("id:{0}/{1}, aniFileName:{2}, stateName:{3}", i + 1, tarFiles.Count, aniFileName, aniStateName);
            }
        }

        //处理Animator State
        AnimatorStateMachine  asm = ac.layers[0].stateMachine; //找到对应层的动画状态机
        ChildAnimatorState[] cas = asm.states;//获取那一层上面的动画状态（动画 ）list（这个list里不是直接装的动画 你要通过state获得动画）
        for (int i = 0; i < cas.Length; i++)
        {
            Debug.LogFormat("处理state name: {0}", cas[i].state.name);
            cas[i].state.motion = stateToAniClip[cas[i].state.name];
        }
    }

    static string GetStateName(string aniFileName)
    {
        string[] nameParts = aniFileName.Split('_');
        string aniStateName = null;
        if (nameParts.Length >= 1)
        {
            aniStateName = nameParts[nameParts.Length - 1];
        }

        return aniStateName;
    }
    public static string FilePathToDirPath(string filePath)
    {
        filePath = ToBackslashPath(filePath);
        int ssLen = filePath.LastIndexOf('\\') + 1;
        string fileName = filePath.Substring(0, ssLen);
        return fileName;
    }

    public static string[] GetDirFiles(string dirPath, string suffix)
    {
        if (!Directory.Exists(dirPath))
        {
            return new string[0];
        }

        string[] files = Directory.GetFiles(dirPath, "*." + suffix, SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            files[i] = ToSlashPath(files[i]);
        }
        return files;
    }

    public static string FilePathToSimpleName(string filePath)
    {
        filePath = ToBackslashPath(filePath);
        int ssIndex = filePath.LastIndexOf('\\') + 1;
        int ssLen = filePath.LastIndexOf('.') - ssIndex;
        if (ssLen != 0)
        {
            return filePath.Substring(ssIndex, ssLen);
        }
        else
        {
            Debug.LogWarningFormat("can't find'.' in {0}", filePath);
            return filePath;
        }
    }

    public static string ToBackslashPath(string str)
    {
        return str.Replace('/', '\\');
    }

    public static string ToSlashPath(string str)
    {
        return str.Replace('\\', '/');
    }
}
