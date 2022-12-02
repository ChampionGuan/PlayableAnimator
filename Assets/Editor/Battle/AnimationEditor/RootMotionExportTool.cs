using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class RootMotionExportTool
{
    public static string ANIMATION_FBX_DIRECTORY_ROOT_TEMP_PATH = "Assets/Build/Art/____temp";
    public static string ANIMATION_FBX_DIRECTORY_ROOT_PATH = "Assets/Build/Art/Animations";
    public static List<string> ANIMATION_FBX_DIRECTORY_PATH = new List<string>() {"Assets/Build/Art/Animations/Roles", "Assets/Build/Art/Animations/Monsters"};
    public static string ANIMATION_FBX_EXT = ".fbx";

    public static int ANIMATION_CLIP_FPS = 30;
    public static string ANIMATION_CLIP_ROOTMOTION_RECORDER = "RootMotionRecorder";
    public static string ANIMATION_CLIP_ROOTMOTION_DIRECTORY_PATH = "Assets/Build/Art/Animations/RootMotion";

    public static int ANIMATION_DATA_UNIT = 10000;

    public static void Export(List<Object> objs)
    {
        if (null == objs)
        {
            return;
        }

        for (var i = 0; i < objs.Count; i++)
        {
            Export(objs[i], false, true, (i + 1) / (float) objs.Count);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    public static void Export(Object obj, bool refresh = true, bool showProgressBar = false, float progress = 1)
    {
        if (null == obj)
        {
            return;
        }

        var path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path) || Path.GetExtension(path).ToLower() != ANIMATION_FBX_EXT)
        {
            return;
        }

        if (showProgressBar)
        {
            EditorUtility.DisplayProgressBar("RootMotion导出中...", path, progress);
        }

        var oldPath = path;
        path = path.Replace(ANIMATION_FBX_DIRECTORY_ROOT_PATH, ANIMATION_FBX_DIRECTORY_ROOT_TEMP_PATH);
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        AssetDatabase.CopyAsset(oldPath, path);

        var modelImporter = ModelImporter.GetAtPath(path) as ModelImporter;
        if (modelImporter.animationCompression != ModelImporterAnimationCompression.Off)
        {
            modelImporter.animationCompression = ModelImporterAnimationCompression.Off;
            modelImporter.SaveAndReimport();
        }

        var clips = new List<AnimationClip>();
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var asset in allAssets)
        {
            var clip = asset as AnimationClip;
            if (clip)
            {
                if ((asset.hideFlags & HideFlags.HideInInspector) == HideFlags.HideInInspector)
                {
                    continue;
                }

                clips.Add(clip);
            }
        }

        foreach (var clip in clips)
        {
            if (clip.frameRate != ANIMATION_CLIP_FPS)
            {
                Debug.LogError($"RootMotion导出错误，动画:{clip.name}未按约定帧率30FPS设置，请检查！！");
                continue;
            }

            foreach (var modelClip in modelImporter.clipAnimations)
            {
                if (modelClip.name != clip.name)
                {
                    continue;
                }

                var posX = new List<float>();
                var posY = new List<float>();
                var posZ = new List<float>();
                var rotX = new List<float>();
                var rotY = new List<float>();
                var rotZ = new List<float>();
                var rotW = new List<float>();

                // 一个clip对应多个bind 一个bind对应一个curve 而每个curve对应多个keyframe 
                // 拿到clip对应的绑定数组 Legacy要使用GetObjectReferenceCurveBindings 非Legacy动画使用GetCurveBindings
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    // 约定的骨骼名称
                    if (!binding.path.Contains(ANIMATION_CLIP_ROOTMOTION_RECORDER) || binding.type != typeof(Transform))
                    {
                        continue;
                    }

                    var curve = AnimationUtility.GetEditorCurve(clip, binding);

                    var startFrame = 0;
                    var lastFrame = curve.length;

                    //clip帧率与curve采样帧率并不一定一致
                    var interval = 1;
                    if (curve.length != clip.length)
                    {
                        interval = curve.length / (int) (clip.length * ANIMATION_CLIP_FPS);
                    }

                    if (interval <= 0)
                    {
                        continue;
                    }

                    switch (binding.propertyName)
                    {
                        case "m_LocalPosition.x":
                            CollectCurveKey(path, clip.name, curve, startFrame, lastFrame, interval, ref posX);
                            break;
                        case "m_LocalPosition.y":
                            CollectCurveKey(path, clip.name, curve, startFrame, lastFrame, interval, ref posY);
                            break;
                        case "m_LocalPosition.z":
                            CollectCurveKey(path, clip.name, curve, startFrame, lastFrame, interval, ref posZ);
                            break;
                        case "m_LocalRotation.x":
                            CollectCurveKey(path, clip.name, curve, startFrame, lastFrame, interval, ref rotX);
                            break;
                        case "m_LocalRotation.y":
                            CollectCurveKey(path, clip.name, curve, startFrame, lastFrame, interval, ref rotY);
                            break;
                        case "m_LocalRotation.z":
                            CollectCurveKey(path, clip.name, curve, startFrame, lastFrame, interval, ref rotZ);
                            break;
                        case "m_LocalRotation.w":
                            CollectCurveKey(path, clip.name, curve, startFrame, lastFrame, interval, ref rotW);
                            break;
                    }
                }

                var datas = new List<RMCurve>();
                for (var i = 0; i < posX.Count; i++)
                {
                    var rot = Quaternion.identity;
                    if (i < rotX.Count && i < rotY.Count && i < rotZ.Count && i < rotW.Count)
                    {
                        rot = new Quaternion(rotX[i], rotY[i], rotZ[i], rotW[i]);
                    }

                    datas.Add(new RMCurve(
                        (int) (posX[i] * ANIMATION_DATA_UNIT),
                        (int) (posY[i] * ANIMATION_DATA_UNIT),
                        (int) (posZ[i] * ANIMATION_DATA_UNIT),
                        (int) (rot.eulerAngles.y * ANIMATION_DATA_UNIT)
                    ));
                }

                var fullPath = Path.Combine($"{Application.dataPath}/../", $"{ANIMATION_CLIP_ROOTMOTION_DIRECTORY_PATH}/{ClipRelativePath(clip)}.bytes");
                var dirPath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                if (datas.Count < 1)
                {
                    continue;
                }

                using (var writer = new BinaryWriter(File.Open(fullPath, FileMode.Create)))
                {
                    writer.Write(datas.Count);
                    foreach (var item in datas)
                    {
                        writer.Write(item.posX);
                        writer.Write(item.eulerAnglesY);
                        writer.Write(item.posZ);
                    }
                }
            }

            if (refresh)
            {
                AssetDatabase.Refresh();
            }
        }

        Directory.Delete(ANIMATION_FBX_DIRECTORY_ROOT_TEMP_PATH, true);
    }

    public static List<Object> GetAllAnimationFBXs()
    {
        var filesPath = new List<string>();
        foreach (var path in ANIMATION_FBX_DIRECTORY_PATH)
        {
            filesPath.AddRange(Directory.GetFiles(path, "*", SearchOption.AllDirectories));
        }

        var result = new List<Object>();
        foreach (var path in filesPath)
        {
            result.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
        }

        return result;
    }

    public static List<Object> GetSelectionAnimationFBXs()
    {
        var result = new List<Object>();
        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (!IsValidatePath(path))
            {
                continue;
            }

            if (Directory.Exists(path))
            {
                foreach (var filePath in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(filePath).ToLower() == ANIMATION_FBX_EXT)
                    {
                        var file = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                        if (!result.Contains(file))
                        {
                            result.Add(file);
                        }
                    }
                }
            }
            else if (File.Exists(path) && Path.GetExtension(path).ToLower() == ANIMATION_FBX_EXT && !result.Contains(obj))
            {
                result.Add(obj);
            }
        }

        return result;
    }

    public static string ClipRelativePath(AnimationClip clip)
    {
        if (null == clip)
        {
            return null;
        }

        var path = AssetDatabase.GetAssetPath(clip).Replace(ANIMATION_FBX_DIRECTORY_ROOT_TEMP_PATH, ANIMATION_FBX_DIRECTORY_ROOT_PATH);
        if (!path.StartsWith(ANIMATION_FBX_DIRECTORY_ROOT_PATH))
        {
            return null;
        }

        //"Roles/PL/Common/Idle"
        return $"{Path.GetDirectoryName(path).Replace("\\", "/").Replace(ANIMATION_FBX_DIRECTORY_ROOT_PATH, "").Substring(1)}/{clip.name}";
    }

    public static string ClipFullPathHashID(AnimationClip clip)
    {
        if (null == clip)
        {
            return null;
        }

        var path = $"{Path.GetDirectoryName(AssetDatabase.GetAssetPath(clip)).Replace("\\", "/")}/{clip.name}";
        return Animator.StringToHash(path).ToString();
    }

    public static bool IsRootMotionClip(AnimationClip clip)
    {
        if (null == clip)
        {
            return false;
        }

        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            // 约定的骨骼名称
            if (binding.path.Contains(ANIMATION_CLIP_ROOTMOTION_RECORDER) && binding.type == typeof(Transform))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsValidatePath(string path)
    {
        var isValid = false;
        if (string.IsNullOrEmpty(path))
        {
            return isValid;
        }

        path = path.Replace("\\", "/");
        foreach (var v in ANIMATION_FBX_DIRECTORY_PATH)
        {
            if (path.StartsWith(v))
            {
                isValid = true;
                break;
            }
        }

        return isValid;
    }

    private static void CollectCurveKey(string clipPath, string clipName, AnimationCurve curve, int startFrame, int lastFrame, int interval, ref List<float> array)
    {
        array.Clear();
        for (var i = startFrame; i < lastFrame; i += interval)
        {
            if (curve.keys.Length > i)
            {
                array.Add(curve.keys[i].value);
            }
            else
            {
                Debug.LogWarning($"[RootMotion] [clipPath:{clipPath}  clipName:{clipName} 长度不匹配]，clip.length:{curve.keys.Length}");
            }
        }
    }

    public class RMCurve
    {
        public int posX;
        public int posY;
        public int posZ;
        public int eulerAnglesY;

        public RMCurve(int posX, int posY, int posZ, int eulerAnglesY)
        {
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
            this.eulerAnglesY = eulerAnglesY;
        }
    }
}