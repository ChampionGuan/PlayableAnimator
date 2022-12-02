using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class BattleModelEditor
{
    private static string _artDirectory = "Assets/Build/Art/Character/Prefabs/Battle";
    private static string _prefabDirectory = "Assets/ResourcesWorkspace/Battle/Actors";
    private static string _animationDirectory = "Assets/Build/Art/Animations";
    private static string _animatorDirectory = "Assets/Build/Art/Animations/AnimatorController";
    private static string _monsterTag = "Monsters";
    private static string _roleTag = "Roles";
    private static string _templateTag = "Template";
    private static List<AnimationClip> _clipList = new List<AnimationClip>();
    private static string _assetRoot;
    private static List<AnimationClip> _usedAnims = new List<AnimationClip>();
    private static List<AnimationClip> _unusedAnims = new List<AnimationClip>();
    private static string _templateAnimatorPath;
    private static ResourceType _resourceType;

    private static string _assetPath
    {
        get
        {
            if(_assetRoot == null)
            {
                _assetRoot = Application.dataPath.Replace("Assets", "");
            }
            return _assetRoot;
        }
    }

    struct AnimatorData
    {
        //public string parentName;
        public string name;
        public List<AnimationClip> clipList;
    }

    struct TransitionData
    {
        public AnimatorStateTransition transition;
        public string destinationStateName;
    }

    enum ResourceType
    {
        Female,
        Male,
        Monster,
    }

    [MenuItem("Assets/Battle/Create Animations", false, 0)]
    [MenuItem("Battle/Create Animations")]
    private static void Do()
    {
        Object[] objects = Selection.objects;
        for(int i = 0; i < objects.Length; i++)
        {
            DoOne(objects[i]);
        }
    }

    public static List<AnimatorController> CreateAnimator(string parentName, string name)
    {
        string assetPath = string.Format("{0}/{1}/{2}", _animationDirectory, parentName, name);
        return _Do(assetPath, name);
    }

    private static void DoOne(Object obj)
    {
        string assetPath = AssetDatabase.GetAssetPath(obj);
        string assetName = obj.name;
        _Do(assetPath, assetName);
    }

    private static List<AnimatorController> _Do(string assetPath, string assetName)
    {
        List<AnimatorController> controllers = new List<AnimatorController>();
        List<AnimatorData> animatorDatas = new List<AnimatorData>();
        if(assetPath.Contains(_monsterTag))
        {
            _resourceType = ResourceType.Monster;
            string animationRoleDiretory = string.Format("{0}/{1}", _animationDirectory, _monsterTag);
            if (!assetPath.Contains(animationRoleDiretory))
            {
                return null;
            }
            DirectoryInfo dir = new DirectoryInfo(assetPath);
            if (!dir.Parent.Name.Equals(_monsterTag))
            {
                return null;
            }
            animatorDatas.AddRange(GetDatas(dir, assetName));
        }
        else
        {
            string animationRoleDiretory = string.Format("{0}/{1}", _animationDirectory, _roleTag);
            if (!assetPath.Contains(animationRoleDiretory))
            {
                return null;
            }
            DirectoryInfo dir = new DirectoryInfo(assetPath);
            if (!dir.Parent.Name.Equals(_roleTag))
            {
                return null;
            }
            
            if (assetName.Equals("PL"))
            {
                _resourceType = ResourceType.Female;
                DirectoryInfo[] dirs = dir.GetDirectories();
                List<AnimationClip> commonClipList = new List<AnimationClip>();
                List<AnimatorData> weaponAnimatorDatas = new List<AnimatorData>();
                List<AnimatorData> maleAnimatorDatas = new List<AnimatorData>();
                for (int i = 0; i < dirs.Length; i++)
                {
                    DirectoryInfo childDir = dirs[i];
                    if (childDir.Name.Equals("Common"))
                    {
                        GetClips(childDir, commonClipList);
                    }
                    else if (childDir.Name.Equals("Weapon"))
                    {
                        DirectoryInfo[] weaponDirs = childDir.GetDirectories();
                        for (int j = 0; j < weaponDirs.Length; j++)
                        {
                            DirectoryInfo weaponDir = weaponDirs[j];
                            AnimatorData data = new AnimatorData();
                            data.name = weaponDir.Name;
                            data.clipList = new List<AnimationClip>();
                            GetClips(weaponDir, data.clipList);
                            weaponAnimatorDatas.Add(data);
                        }
                    }
                    else if (childDir.Name.Equals("Male"))
                    {
                        DirectoryInfo[] maleDirs = childDir.GetDirectories();
                        for (int j = 0; j < maleDirs.Length; j++)
                        {
                            DirectoryInfo maleDir = maleDirs[j];
                            maleAnimatorDatas.AddRange(GetDatas(maleDir, assetName));
                        }
                    }
                }
                for (int i = 0; i < maleAnimatorDatas.Count; i++)
                {
                    AnimatorData maleData = maleAnimatorDatas[i];
                    for (int j = 0; j < weaponAnimatorDatas.Count; j++)
                    {
                        AnimatorData weaponData = weaponAnimatorDatas[j];
                        AnimatorData data = new AnimatorData();
                        data.name = string.Format("{0}_{1}", maleData.name, weaponData.name);
                        data.clipList = new List<AnimationClip>();
                        data.clipList.AddRange(maleData.clipList);
                        data.clipList.AddRange(weaponData.clipList);
                        data.clipList.AddRange(commonClipList);
                        animatorDatas.Add(data);
                    }
                }
            }
            else
            {
                _resourceType = ResourceType.Male;
                animatorDatas.AddRange(GetDatas(dir, assetName));
            }
        }
        for (int i = 0; i < animatorDatas.Count; i++)
        {
            AnimatorData data = animatorDatas[i];
            controllers.Add(CreateAnimator(data));
        }
        return controllers;
    }

    private static List<AnimatorData> GetDatas(DirectoryInfo dir, string assetName)
    {
        DirectoryInfo[] dirs = dir.GetDirectories();
        List<AnimatorData> animatorDatas = new List<AnimatorData>();
        List<AnimationClip> commonClipList = new List<AnimationClip>();
        for (int i = 0; i < dirs.Length; i++)
        {
            DirectoryInfo childDir = dirs[i];
            if (childDir.Name.Equals("Common"))
            {
                GetClips(childDir, commonClipList);
            }
            else
            {
                AnimatorData data = new AnimatorData();
                //data.parentName = dir.Parent.Name;
                switch (_resourceType)
                {
                    case ResourceType.Female:
                        data.name = string.Format("{0}_{1}{2}", assetName, dir.Name, childDir.Name);
                        break;
                    case ResourceType.Male:
                        data.name = string.Format("{0}_{1}", assetName, childDir.Name);
                        break;
                    default:
                        data.name = string.Format("{0}{1}", assetName, childDir.Name);
                        break;
                }
                data.clipList = new List<AnimationClip>();
                GetClips(childDir, data.clipList);
                animatorDatas.Add(data);
            }
        }
        for (int i = 0; i < animatorDatas.Count; i++)
        {
            AnimatorData data = animatorDatas[i];
            data.clipList.AddRange(commonClipList);
        }
        return animatorDatas;
    }

    private static void BindClips(AnimatorController animatorController, List<AnimationClip> clipList)
    {
        AnimatorControllerLayer animatorLayer = animatorController.layers[0];
        _usedAnims.Clear();
        _unusedAnims.Clear();
        RecursiveStateMachine(animatorLayer.stateMachine, clipList);
        for (int i = 0; i < clipList.Count; i++)
        {
            AnimationClip anim = clipList[i];
            if (!_usedAnims.Contains(anim))
            {
                _unusedAnims.Add(anim);
            }
        }

        for (int i = 0; i < _unusedAnims.Count; i++)
        {
            AnimationClip anim = _unusedAnims[i];
            AnimatorState state = animatorLayer.stateMachine.AddState(anim.name);
            state.motion = anim;
        }
    }

    private static AnimatorController CreateAnimator(AnimatorData data)
    {
        string animatorPath = string.Format("{0}/{1}.controller", _animatorDirectory, data.name);
           
        switch(_resourceType)
        {
            case ResourceType.Female:
                _templateAnimatorPath = string.Format("{0}/{1}/PL.controller", _animatorDirectory, _templateTag);
                break;
            case ResourceType.Male:
                _templateAnimatorPath = string.Format("{0}/{1}/Male.controller", _animatorDirectory, _templateTag);
                break;
            default:
                _templateAnimatorPath = string.Format("{0}/{1}/Monster.controller", _animatorDirectory, _templateTag);
                break;
        }

        AnimatorController animatorController;
        if (!File.Exists(animatorPath) || _resourceType == ResourceType.Female)
        {
            AssetDatabase.CopyAsset(_templateAnimatorPath, animatorPath);
            animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorPath);
        }
        else
        {
            AnimatorController templateController = AssetDatabase.LoadAssetAtPath<AnimatorController>(_templateAnimatorPath);
            AnimatorStateMachine templateStateMachine = templateController.layers[0].stateMachine;
            ChildAnimatorState[] templateChildStates = templateStateMachine.states;
            string defaultStateName = templateStateMachine.defaultState.name;

            animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorPath);
            AnimatorStateMachine targetStateMachine = animatorController.layers[0].stateMachine;
            ChildAnimatorState[] targetChildStates = targetStateMachine.states;
            List<AnimatorState> removeStates = new List<AnimatorState>();
            for (int i = 0; i < templateChildStates.Length; i++)
            {
                ChildAnimatorState templateState = templateChildStates[i];
                for (int j = 0; j < targetChildStates.Length; j++)
                {
                    ChildAnimatorState animatorState = targetChildStates[j];
                    if (animatorState.state.name == templateState.state.name)
                    {
                        removeStates.Add(animatorState.state);
                        break;
                    }
                }
            }

            for(int i = 0; i < removeStates.Count; i++)
            {
                targetStateMachine.RemoveState(removeStates[i]);
            }
            AnimatorState defaultState = null;
            List<AnimatorState> states = new List<AnimatorState>();
            List<TransitionData> transitionDatas = new List<TransitionData>();
            for (int i = 0; i < templateChildStates.Length; i++)
            {
                ChildAnimatorState templateState = templateChildStates[i];
                AnimatorState state = targetStateMachine.AddState(templateState.state.name, templateState.position);
                if(templateState.state.motion is BlendTree)
                {
                    state.motion = templateState.state.motion;
                }
                states.Add(state);
                if(state.name == defaultStateName)
                {
                    defaultState = state;
                }
                AnimatorStateTransition[] templateTransitions = templateState.state.transitions;
                for(int j = 0; j < templateTransitions.Length; j++)
                {
                    AnimatorStateTransition templateTransition = templateTransitions[j];
                    AnimatorStateTransition targetTransition = new AnimatorStateTransition();
                    targetTransition.duration = templateTransition.duration;
                    targetTransition.offset = templateTransition.offset;
                    targetTransition.interruptionSource = templateTransition.interruptionSource;
                    targetTransition.orderedInterruption = templateTransition.orderedInterruption;
                    targetTransition.exitTime = templateTransition.exitTime;
                    targetTransition.hasExitTime = templateTransition.hasExitTime;
                    targetTransition.hasFixedDuration = templateTransition.hasFixedDuration;
                    targetTransition.canTransitionToSelf = templateTransition.canTransitionToSelf;

                    targetTransition.solo = templateTransition.solo;
                    targetTransition.mute = templateTransition.mute;
                    targetTransition.isExit = templateTransition.isExit;
                    targetTransition.destinationStateMachine = null;
                    if(templateTransition.destinationState != null)
                    {
                        TransitionData transitionData = new TransitionData();
                        transitionData.transition = targetTransition;
                        transitionData.destinationStateName = templateTransition.destinationState.name;
                        transitionDatas.Add(transitionData);
                    }
                    targetTransition.conditions = templateTransition.conditions;
                    state.AddTransition(targetTransition);
                }
            }

            for(int i = 0; i < transitionDatas.Count; i++)
            {
                TransitionData transitionData = transitionDatas[i];
                for(int j = 0; j < states.Count; j++)
                {
                    AnimatorState state = states[j];
                    if(transitionData.destinationStateName == state.name)
                    {
                        transitionData.transition.destinationState = state;
                        break;
                    }
                }
            }
            if(defaultState != null)
            {
                targetStateMachine.defaultState = defaultState;
            }
            AssetDatabase.Refresh();
        }
        
        BindClips(animatorController, data.clipList);
        if(_resourceType == ResourceType.Monster)
        {
            string prefabAssetPath = string.Format("{0}/{1}/{2}.prefab", _prefabDirectory, _monsterTag, data.name);
            string fullPrefabAssetPath = string.Format("{0}{1}" , _assetPath, prefabAssetPath);
            if (File.Exists(fullPrefabAssetPath))
            {
                GameObject selectedGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                Animator animator = selectedGameObject.GetComponent<Animator>();
                animator.runtimeAnimatorController = animatorController;
            }
        }
        AssetDatabase.SaveAssets();
        return animatorController;
    }

    private static void RecursiveStateMachine(AnimatorStateMachine sm, List<AnimationClip> clipList)
    {
        ChildAnimatorState[] childStates = sm.states;
        for (int i = 0; i < childStates.Length; i++)
        {
            AnimatorState state = childStates[i].state;
            string stateName = state.name;
            if (state.motion is BlendTree)
            {
                BlendTree blendTree = state.motion as BlendTree;
                RecursiveBlendTree(blendTree, clipList);
            }
            else
            {
                for (int j = 0; j < clipList.Count; j++)
                {
                    AnimationClip clip = clipList[j];
                    if (stateName == clip.name)
                    {
                        _usedAnims.Add(clip);
                        state.motion = clip;
                    }
                }
            }
        }
        ChildAnimatorStateMachine[] childSms = sm.stateMachines;
        for (int i = 0; i < childSms.Length; i++)
        {
            RecursiveStateMachine(childSms[i].stateMachine, clipList);
        }
    }

    private static void RecursiveBlendTree(BlendTree blendTree, List<AnimationClip> clipList)
    {
        for (int i = 0; i < blendTree.children.Length; i++)
        {
            ChildMotion childMotion = blendTree.children[i];
            if (childMotion.motion is AnimationClip)
            {
                for (int j = 0; j < clipList.Count; j++)
                {
                    AnimationClip clip = clipList[j];
                    if (childMotion.motion.name == clip.name)
                    {
                        _usedAnims.Add(clip);
                        childMotion.motion = clip;
                    }
                }
            }
            else if(childMotion.motion is BlendTree)
            {
                BlendTree childBlendTree = childMotion.motion as BlendTree;
                RecursiveBlendTree(childBlendTree, clipList);
            }
        }
    }

    private static void GetClips(DirectoryInfo dir, List<AnimationClip> clipList)
    {
        FileInfo[] fileInfos = dir.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            string animPath = fileInfos[i].FullName.Replace("\\", "/").Replace(_assetPath, "");
            if (animPath.ToLower().EndsWith(".fbx"))
            {
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(animPath);
                for (int j = 0; j < objs.Length; j++)
                {
                    if (!objs[j].name.StartsWith("_") && objs[j] is AnimationClip)
                    {
                        AnimationClip clip = objs[j] as AnimationClip;
                        clipList.Add(clip);
                    }
                }
            }
            else if(animPath.ToLower().EndsWith(".anim"))
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
                clipList.Add(clip);
            }
        }
    }


    [MenuItem("Battle/Create Bezier")]
    private static void CreateBezier()
    {
        // GameObject go = new GameObject("Bezier");
        // BezierDrawLine bezier = go.AddComponent<BezierDrawLine>();
        // bezier.wayPoint = new List<Transform>();
        //
        // for (int i = 0; i < 3; i++)
        // {
        //     GameObject point = new GameObject("p (" + (i+1).ToString() + ")");
        //     point.transform.position = new Vector3(0, i * 5, i * 5);
        //     if(i == 2)
        //         point.transform.position = new Vector3(0, 0, i * 5);
        //
        //     point.transform.parent = go.transform;
        //     bezier.wayPoint.Add(point.transform);
        // }
    }
}