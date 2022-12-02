using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Animations;
using System.Reflection;

public static class AnimatorControllerExportTool
{
    public static List<string> ANIMATOR_CTRL_CLIP_DIRECTORY_PATH = new List<string>() {"Assets/Build/Art/Animations/Roles", "Assets/Build/Art/Animations/Monsters"};
    public static string ANIMATOR_CTRL_DIRECTORY_LUA_PATH = "Assets/LuaSourceCode/Battle/Config/RMAnimatorConfig/Animator";
    public static string ANIMATOR_CTRL_DIRECTORY_TEMPLATE_PATH = "Assets/Build/Art/Animations/AnimatorController/Template";
    public static string ANIMATOR_CTRL_DIRECTORY_PATH = "Assets/Build/Art/Animations/AnimatorController";
    public static string ANIMATOR_CTRL_EXT = ".controller";

    public static void Export(List<AnimatorController> ctrls, List<string> paths)
    {
        Export(ctrls);
        Export(paths);
    }

    public static void Export(List<string> paths)
    {
        if (null == paths)
        {
            return;
        }

        for (var i = 0; i < paths.Count; i++)
        {
            Export(paths[i], true, (i + 1) / (float) paths.Count);
        }

        EditorUtility.ClearProgressBar();
    }

    public static void Export(string path, bool showProgressBar = false, float progress = 1)
    {
        if (!IsValidatePath(Path.GetDirectoryName(path).Replace("\\", "/")))
        {
            return;
        }

        path = path.Replace("\\", "/");
        var parentName = string.Empty;
        var selectedName = string.Empty;
        foreach (var dic in ANIMATOR_CTRL_CLIP_DIRECTORY_PATH)
        {
            if (path.Contains(dic))
            {
                parentName = dic.Substring(dic.LastIndexOf("/") + 1);
                while (true)
                {
                    selectedName = path.Substring(path.LastIndexOf("/") + 1);
                    path = Path.GetDirectoryName(path).Replace("\\", "/");
                    if (path == dic)
                    {
                        break;
                    }
                }

                break;
            }
        }

        if (showProgressBar)
        {
            EditorUtility.DisplayProgressBar("AnimatorController生成中...", $"{path}/{selectedName}", progress);
        }

        Export(BattleModelEditor.CreateAnimator(parentName, selectedName));
    }

    public static void Export(List<AnimatorController> ctrls)
    {
        if (null == ctrls)
        {
            return;
        }

        for (int i = 0; i < ctrls.Count; i++)
        {
            Export(ctrls[i], false, true, (i + 1) / (float) ctrls.Count);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    public static void Export(AnimatorController ctrl, bool refresh = true, bool showProgressBar = false, float progress = 1)
    {
        if (null == ctrl)
        {
            return;
        }

        var path = AssetDatabase.GetAssetPath(ctrl);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        path = path.Replace("\\", "/");
        if (!path.StartsWith(ANIMATOR_CTRL_DIRECTORY_PATH) || path.StartsWith(ANIMATOR_CTRL_DIRECTORY_TEMPLATE_PATH))
        {
            return;
        }

        if (showProgressBar)
        {
            EditorUtility.DisplayProgressBar("AnimatorController导出中...", path, progress);
        }

        ExportAnimatorCtrlClips(ctrl);
        ExportAnimatorCtrlConfig(ctrl);

        if (refresh)
        {
            AssetDatabase.Refresh();
        }
    }

    public static List<string> GetAllAnimationRootPaths()
    {
        var result = new List<string>();
        foreach (var path in ANIMATOR_CTRL_CLIP_DIRECTORY_PATH)
        {
            var dirPaths = Directory.GetDirectories(path);
            foreach (var subPath in dirPaths)
            {
                result.Add(subPath.Replace("\\", "/"));
            }
        }

        return result;
    }

    public static List<string> GetSelectionAnimationRootPaths()
    {
        var result = new List<string>();
        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (!IsValidatePath(Path.GetDirectoryName(path).Replace("\\", "/")) || result.Contains(path))
            {
                continue;
            }

            result.Add(path);
        }

        return result;
    }

    public static List<AnimatorController> GetSelectionAnimatorControllers()
    {
        var result = new List<AnimatorController>();
        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (!path.StartsWith(ANIMATOR_CTRL_DIRECTORY_PATH) || path.StartsWith(ANIMATOR_CTRL_DIRECTORY_TEMPLATE_PATH))
            {
                continue;
            }

            if (Directory.Exists(path))
            {
                foreach (var filePath in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(filePath).ToLower() == ANIMATOR_CTRL_EXT)
                    {
                        var file = AssetDatabase.LoadAssetAtPath<AnimatorController>(filePath);
                        if (file && !result.Contains(file))
                        {
                            result.Add(file);
                        }
                    }
                }
            }
            else if (File.Exists(path) && Path.GetExtension(path).ToLower() == ANIMATOR_CTRL_EXT)
            {
                var ctrl = obj as AnimatorController;
                if (ctrl && !result.Contains(ctrl))
                {
                    result.Add(ctrl);
                }
            }
        }

        return result;
    }

    private static bool IsValidatePath(string path)
    {
        var isValid = false;
        if (string.IsNullOrEmpty(path))
        {
            return isValid;
        }

        path = path.Replace("\\", "/");
        foreach (var v in ANIMATOR_CTRL_CLIP_DIRECTORY_PATH)
        {
            if (path.StartsWith(v))
            {
                isValid = true;
                break;
            }
        }

        return isValid;
    }

    private static void SetFieldInfo(object ins, string fieldName, object fieldValue)
    {
        if (null == ins)
        {
            return;
        }

        var fieldInfo = ins.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (null == fieldInfo)
        {
            return;
        }

        fieldInfo.SetValue(ins, fieldValue);
    }

    private static void ExportAnimatorCtrlClips(AnimatorController ctrl)
    {
        // if (null == ctrl)
        // {
        //     return;
        // }
        //
        // var path = $"{ANIMATOR_CTRL_DIRECTORY_PATH}/{ctrl.name}.asset";
        // var asset = ScriptableObject.CreateInstance<AnimatorControllerClips>();
        //
        // var keys = new List<string>();
        // var clips = new List<AnimationClip>();
        // foreach (var clip in ctrl.animationClips)
        // {
        //     keys.Add(RootMotionExportTool.ClipFullPathHashID(clip));
        //     clips.Add(clip);
        // }
        //
        // if (File.Exists(path))
        // {
        //     AssetDatabase.DeleteAsset(path);
        // }
        //
        // if (keys.Count == clips.Count)
        // {
        //     SetFieldInfo(asset, "m_keys", keys);
        //     SetFieldInfo(asset, "m_clips", clips);
        //     AssetDatabase.CreateAsset(asset, path);
        // }
    }

    private static void ExportAnimatorCtrlConfig(AnimatorController ctrl)
    {
        var sb = new StringBuilder();
        sb.Append("local animatorController = {");

        sb.Append($"name='{ctrl.name}',");

        sb.Append("parameters={");
        foreach (var parameter in ctrl.parameters)
        {
            sb.Append("{");
            sb.Append($"name='{parameter.name}',");
            sb.Append($"type={(int) parameter.type},");
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    sb.Append($"value={parameter.defaultBool.ToString().ToLower()},");
                    break;
                case AnimatorControllerParameterType.Int:
                    sb.Append($"value=({parameter.defaultInt}),");
                    break;
                case AnimatorControllerParameterType.Float:
                    sb.Append($"value=({parameter.defaultFloat}),");
                    break;
            }

            sb.Append("},");
        }

        sb.Append("},");

        sb.Append("layers={");
        for (var i = 0; i < ctrl.layers.Length; i++)
        {
            var layer = ctrl.layers[i];
            sb.Append("{");

            sb.Append($"name='{layer.name}',");
            sb.Append($"defaultSpeed=(1),");
            sb.Append($"defaultWeight=({(i == 0 ? 1 : layer.defaultWeight)}),");
            sb.Append($"defaultStateName='{layer.stateMachine.defaultState?.name}',");
            sb.Append($"blendingType={(int) layer.blendingMode},");

            sb.Append("states={");
            foreach (var state in layer.stateMachine.states)
            {
                var isBlendTree = state.state.motion is BlendTree;

                sb.Append("{");

                sb.Append($"name='{state.state.name}',");
                sb.Append($"tag='{state.state.tag}',");
                sb.Append($"defaultSpeed=({state.state.speed}),");
                sb.Append($"speedParameterName='{state.state.speedParameter}',");
                sb.Append($"speedParameterActive={state.state.speedParameterActive.ToString().ToLower()},");

                if (isBlendTree)
                {
                    var blendTree = state.state.motion as BlendTree;

                    sb.Append("blendTree={");
                    sb.Append($"blendParameterName='{blendTree.blendParameter}',");
                    sb.Append($"blendType={(int) blendTree.blendType},");
                    sb.Append($"minThreshold=({blendTree.minThreshold}),");
                    sb.Append($"maxThreshold=({blendTree.maxThreshold}),");

                    sb.Append("childMotions={");
                    foreach (var child in blendTree.children)
                    {
                        sb.Append("{");
                        sb.Append($"threshold={child.threshold},");
                        sb.Append($"clip={ExportClipInfo(child.motion)},");
                        sb.Append("},");
                    }

                    sb.Append("},");

                    sb.Append("},");
                }
                else
                {
                    sb.Append($"soloClip={ExportClipInfo(state.state.motion)},");
                }

                sb.Append("transitions={");
                foreach (var transition in state.state.transitions)
                {
                    sb.Append("{");
                    sb.Append($"solo={transition.solo.ToString().ToLower()},");
                    sb.Append($"mute={transition.mute.ToString().ToLower()},");
                    sb.Append($"hasExitTime={transition.hasExitTime.ToString().ToLower()},");
                    sb.Append($"hasFixedDuration={transition.hasFixedDuration.ToString().ToLower()},");
                    sb.Append($"offset=({transition.offset}),");
                    sb.Append($"exitTime=({transition.exitTime}),");
                    sb.Append($"duration=({transition.duration}),");
                    sb.Append($"destinationStateName='{transition.destinationState.name}',");

                    sb.Append("conditions={");
                    foreach (var condition in transition.conditions)
                    {
                        sb.Append("{");
                        sb.Append($"type={(int) condition.mode},");
                        sb.Append($"parameterName='{condition.parameter}',");
                        sb.Append($"threshold=({condition.threshold}),");
                        sb.Append("},");
                    }

                    sb.Append("},");

                    sb.Append("},");
                }

                sb.Append("},");

                sb.Append("},");
            }

            sb.Append("},");

            sb.Append("},");
        }

        sb.Append("}");

        sb.Append("}");

        sb.Append("return animatorController");

        var fullPath = $"{ANIMATOR_CTRL_DIRECTORY_LUA_PATH}/AnimatorConfig_{ctrl.name}.lua";
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        }

        File.Create(fullPath).Dispose();

        using (var streamWriter = new StreamWriter(fullPath, false, new UTF8Encoding(true, false)))
        {
            streamWriter.Write(sb.ToString());
        }
    }
    
    private static string ExportClipInfo(Motion motion)
    {
        StringBuilder sb = new StringBuilder();

        var clip = motion as AnimationClip;
        sb.Append("{");
        sb.Append($"name='{clip?.name}',");
        sb.Append($"path='{(RootMotionExportTool.IsRootMotionClip(clip) ? RootMotionExportTool.ClipRelativePath(clip) : System.String.Empty)}',");
        sb.Append($"hashID='{RootMotionExportTool.ClipFullPathHashID(clip)}',");
        sb.Append($"length={(motion is AnimationClip ? clip.length : 1)},");
        sb.Append($"isLoop={(null != motion && motion.isLooping).ToString().ToLower()},");
        sb.Append("}");

        return sb.ToString();
    }
}