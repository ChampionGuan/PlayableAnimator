using UnityEngine;
using UnityEditor;
using System.IO;

public static class AnimationExportTool
{
    [MenuItem("Assets/动画数据处理/RootMotion/导出选中", true, 0)]
    private static bool ExportSelectionRootMotionIsValidate()
    {
        var fbxs = RootMotionExportTool.GetSelectionAnimationFBXs();
        return null != fbxs && fbxs.Count > 0;
    }

    [MenuItem("Assets/动画数据处理/RootMotion/导出选中", false, 0)]
    public static void ExportSelectionRootMotion()
    {
        RootMotionExportTool.Export(RootMotionExportTool.GetSelectionAnimationFBXs());
        EditorUtility.DisplayDialog("RootMotion导出", "导出完成！！", "ok");
    }

    [MenuItem("Assets/动画数据处理/RootMotion/导出所有", false, 0)]
    public static void ExportAllRootMotion()
    {
        RootMotionExportTool.Export(RootMotionExportTool.GetAllAnimationFBXs());
        EditorUtility.DisplayDialog("RootMotion导出", "导出完成！！", "ok");
    }

    [MenuItem("Assets/动画数据处理/AnimatorController/导出选中", true, 0)]
    private static bool ExportSelectionAnimatorControllerIsValidate()
    {
        var ctrls = AnimatorControllerExportTool.GetSelectionAnimatorControllers();
        var paths = AnimatorControllerExportTool.GetSelectionAnimationRootPaths();
        return (null != ctrls && ctrls.Count > 0) || (null != paths && paths.Count > 0);
    }

    [MenuItem("Assets/动画数据处理/AnimatorController/导出选中", false, 0)]
    public static void ExportSelectionAnimatorController()
    {
        var isOverride = true;
        var paths = AnimatorControllerExportTool.GetSelectionAnimationRootPaths();
        var ctrls = AnimatorControllerExportTool.GetSelectionAnimatorControllers();

        if (paths.Count > 0)
        {
            isOverride = EditorUtility.DisplayDialog("AnimatorController导出", "是否覆盖导出？", "覆盖", "叠加");
        }

        AnimatorControllerExportTool.Export(ctrls, paths);
        EditorUtility.DisplayDialog("AnimatorController导出", "导出完成！！", "ok");
    }

    [MenuItem("Assets/动画数据处理/AnimatorController/导出所有", false, 0)]
    public static void ExportAllAnimatorController()
    {
        var isOverride = EditorUtility.DisplayDialog("AnimatorController导出", "是否覆盖导出？", "覆盖", "叠加");

        AnimatorControllerExportTool.Export(AnimatorControllerExportTool.GetAllAnimationRootPaths());
        EditorUtility.DisplayDialog("AnimatorController导出", "导出完成！！", "ok");
    }

    [MenuItem("Assets/动画数据处理/设置三段Animator动画")]
    public static void AssignAnimation()
    {
        AnimatorSettingTool.OnAssignAnimation();
    }

    [MenuItem("Assets/动画数据处理/导出所有", false, 0)]
    public static void ExportAll()
    {
        var isOverride = EditorUtility.DisplayDialog("AnimatorController导出", "是否覆盖导出？", "覆盖", "叠加");

        RootMotionExportTool.Export(RootMotionExportTool.GetAllAnimationFBXs());
        AnimatorControllerExportTool.Export(AnimatorControllerExportTool.GetAllAnimationRootPaths());
        EditorUtility.DisplayDialog("RootMotion&AnimatorConfig导出", "导出完成！！", "ok");
    }

    [MenuItem("Assets/动画数据处理/非压缩动画", true, 0)]
    private static bool ShowOriginalAnimationIsValidate()
    {
        var fbxs = RootMotionExportTool.GetSelectionAnimationFBXs();
        return null != fbxs && fbxs.Count > 0;
    }

    [MenuItem("Assets/动画数据处理/非压缩动画", false, 0)]
    public static void ShowOriginalAnimation()
    {
        var fbxs = RootMotionExportTool.GetSelectionAnimationFBXs();
        if (null == fbxs || fbxs.Count < 1)
        {
            return;
        }

        for (var i = 0; i < fbxs.Count; i++)
        {
            var path = AssetDatabase.GetAssetPath(fbxs[i]).Replace("\\", "/");

            EditorUtility.DisplayProgressBar("生成非压缩动画中...", path, (i + 1) / (float) fbxs.Count);

            var destPath = $"{Path.GetDirectoryName(path)}/_{Path.GetFileNameWithoutExtension(path)}___original{Path.GetExtension(path)}";
            AssetDatabase.CopyAsset(path, destPath);

            var modelImporter = ModelImporter.GetAtPath(destPath) as ModelImporter;
            modelImporter.animationCompression = ModelImporterAnimationCompression.Off;
            modelImporter.SaveAndReimport();
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
}