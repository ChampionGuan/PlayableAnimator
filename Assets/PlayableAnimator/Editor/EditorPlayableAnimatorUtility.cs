using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

namespace X3.PlayableAnimator.Editor
{
    public class EditorPlayableAnimatorUtility
    {
        public static void RemoveAbandonedLayers(List<AnimatorController> ctrls, List<string> abandonedLayers)
        {
            if (null == ctrls || null == abandonedLayers || abandonedLayers.Count <= 0)
            {
                return;
            }

            foreach (var ctrl in ctrls)
            {
                RemoveAbandonedLayer(ctrl, abandonedLayers);
            }

            AssetDatabase.SaveAssets();
        }

        public static void RemoveAbandonedLayer(AnimatorController ctrl, List<string> abandonedLayers, bool autoSaved = false)
        {
            if (null == ctrl || null == abandonedLayers || abandonedLayers.Count <= 0)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(ctrl);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            for (var i = ctrl.layersCount - 1; i >= 0; i--)
            {
                var layer = ctrl.GetLayer(i);
                if (abandonedLayers.Contains(layer.name))
                {
                    ctrl.RemoveLayer(layer.name);
                }

                ctrl.SetDirty();
            }

            if (autoSaved)
            {
                AssetDatabase.SaveAssets();
            }
        }

        public static void ToPlayableAnimatorCtrl(List<UnityEditor.Animations.AnimatorController> controllers, string path)
        {
            if (null == controllers)
            {
                return;
            }

            foreach (var controller in controllers)
            {
                ToPlayableAnimatorCtrl(controller, path);
            }
        }

        public static AnimatorController ToPlayableAnimatorCtrl(UnityEditor.Animations.AnimatorController ctrl, string path = null, bool autoSaved = true)
        {
            // var path = AssetDatabase.GetAssetPath(ctrl);
            // string playablePath = $"{path.Substring(0, path.LastIndexOf("."))}.asset";
            if (string.IsNullOrEmpty(path))
            {
                path = AssetDatabase.GetAssetPath(ctrl);
                if (string.IsNullOrEmpty(path)) return null;
                path = path.Substring(0, path.LastIndexOf("/"));
            }

            string playablePath = $"{path}/{ctrl.name}.asset";
            var xCtrl = AssetDatabase.LoadAssetAtPath<X3.PlayableAnimator.AnimatorController>(playablePath);
            if (null != xCtrl)
            {
                xCtrl.Clear();
            }
            else
            {
                xCtrl = ScriptableObject.CreateInstance<X3.PlayableAnimator.AnimatorController>();
                AssetDatabase.CreateAsset(xCtrl, playablePath);
            }

            SetFieldInfo(xCtrl, "m_UnityInsID", ctrl.GetInstanceID());
            SetFieldInfo(xCtrl, "m_PlayableInsID", xCtrl.GetInstanceID());
            foreach (var parameter in ctrl.parameters)
            {
                var xParameter = new X3.PlayableAnimator.AnimatorControllerParameter(parameter.name, (X3.PlayableAnimator.AnimatorControllerParameterType) parameter.type);
                SetFieldInfo(xParameter, "m_DefaultFloat", parameter.defaultFloat);
                SetFieldInfo(xParameter, "m_DefaultBool", parameter.defaultBool);
                SetFieldInfo(xParameter, "m_DefaultInt", parameter.defaultInt);
                xCtrl.AddParameter(xParameter);
            }

            for (int i = 0; i < ctrl.layers.Length; i++)
            {
                var layer = ctrl.layers[i];
                var xLayer = new X3.PlayableAnimator.AnimatorControllerLayer(layer.name, i == 0 ? 1 : layer.defaultWeight, (X3.PlayableAnimator.AnimatorControllerLayerBlendingType) layer.blendingMode);
                SetFieldInfo(xLayer, "m_IKPass", layer.iKPass);
                SetFieldInfo(xLayer, "m_AvatarMask", layer.avatarMask);
                SetFieldInfo(xLayer, "m_SyncedLayerAffectsTiming", layer.syncedLayerAffectsTiming);
                SetFieldInfo(xLayer, "m_SyncedLayerIndex", layer.syncedLayerIndex);
                SetFieldInfo(xLayer, "m_DefaultStateName", layer.stateMachine.defaultState?.name);
                ToPlayableAnimatorLayer(xLayer, layer.stateMachine);

                xCtrl.AddLayer(xLayer);
            }

            xCtrl.SetDirty();
            if (autoSaved)
            {
                AssetDatabase.SaveAssets();
            }

            return xCtrl;
        }

        private static void ToPlayableAnimatorLayer(X3.PlayableAnimator.AnimatorControllerLayer xLayer, UnityEditor.Animations.AnimatorStateMachine stateMachine)
        {
            ToPlayableAnimatorState(xLayer, stateMachine.states);
            foreach (var childMachine in stateMachine.stateMachines)
            {
                ToPlayableAnimatorLayer(xLayer, childMachine);
            }
        }

        private static void ToPlayableAnimatorLayer(X3.PlayableAnimator.AnimatorControllerLayer xLayer, UnityEditor.Animations.ChildAnimatorStateMachine stateMachine)
        {
            ToPlayableAnimatorState(xLayer, stateMachine.stateMachine.states);
            foreach (var childMachine in stateMachine.stateMachine.stateMachines)
            {
                ToPlayableAnimatorLayer(xLayer, childMachine);
            }
        }

        private static void ToPlayableAnimatorState(X3.PlayableAnimator.AnimatorControllerLayer xLayer, UnityEditor.Animations.ChildAnimatorState[] states)
        {
            foreach (var state in states)
            {
                X3.PlayableAnimator.ClipMotion xMotion = null;
                X3.PlayableAnimator.BlendTree xBlendTree = null;
                if (state.state.motion is AnimationClip)
                {
                    xMotion = (new X3.PlayableAnimator.ClipMotion(state.state.motion as AnimationClip));
                }
                else if (state.state.motion is UnityEditor.Animations.BlendTree)
                {
                    var blendTree = state.state.motion as UnityEditor.Animations.BlendTree;
                    xBlendTree = new X3.PlayableAnimator.BlendTree();
                    SetFieldInfo(xBlendTree, "m_BlendParameterName", blendTree.blendParameter);
                    SetFieldInfo(xBlendTree, "m_BlendType", (X3.PlayableAnimator.BlendTreeType) blendTree.blendType);
                    SetFieldInfo(xBlendTree, "m_MaxThreshold", blendTree.maxThreshold);
                    SetFieldInfo(xBlendTree, "m_MinThreshold", blendTree.minThreshold);
                    SetFieldInfo(xBlendTree, "m_UseAutomaticThresholds", blendTree.useAutomaticThresholds);
                    foreach (var child in blendTree.children)
                    {
                        var xChildMotion = new X3.PlayableAnimator.BlendTreeChild(child.motion as AnimationClip);
                        SetFieldInfo(xChildMotion, "m_Threshold", child.threshold);
                        xBlendTree.AddChild(xChildMotion);
                    }
                }

                var isBlendTree = null != xBlendTree;
                var xState = new X3.PlayableAnimator.StateMotion(state.position, state.state.name, state.state.tag);
                if (isBlendTree)
                {
                    SetFieldInfo(xState, "m_BlendTree", xBlendTree);
                }
                else
                {
                    SetFieldInfo(xState, "m_ClipMotion", xMotion);
                }

                SetFieldInfo(xState, "m_MotionType", isBlendTree ? 1 : 0);
                SetFieldInfo(xState, "m_Tag", state.state.tag);
                SetFieldInfo(xState, "m_DefaultSpeed", state.state.speed);
                SetFieldInfo(xState, "m_SpeedParameterName", state.state.speedParameter);
                SetFieldInfo(xState, "m_SpeedParameterActive", state.state.speedParameterActive);
                SetFieldInfo(xState, "m_WriteDefaultValues", state.state.writeDefaultValues);
                SetFieldInfo(xState, "m_FootIK", state.state.iKOnFeet);
                xLayer.AddState(xState);
            }

            foreach (var state in states)
            {
                foreach (var transition in state.state.transitions)
                {
                    var xTransition = new X3.PlayableAnimator.Transition();
                    SetFieldInfo(xTransition, "m_DestinationStateName", transition.destinationState.name);
                    SetFieldInfo(xTransition, "m_Solo", transition.solo);
                    SetFieldInfo(xTransition, "m_Mute", transition.mute);
                    SetFieldInfo(xTransition, "m_HasExitTime", transition.hasExitTime);
                    SetFieldInfo(xTransition, "m_HasFixedDuration", transition.hasFixedDuration);
                    SetFieldInfo(xTransition, "m_Duration", transition.duration);
                    SetFieldInfo(xTransition, "m_Offset", transition.offset);
                    SetFieldInfo(xTransition, "m_ExitTime", transition.exitTime);
                    SetFieldInfo(xTransition, "m_InterruptionSource", (X3.PlayableAnimator.TransitionInterruptionSource) transition.interruptionSource);
                    SetFieldInfo(xTransition, "m_OrderedInterruption", transition.orderedInterruption);
                    foreach (var condition in transition.conditions)
                    {
                        var xCondition = new X3.PlayableAnimator.Condition();
                        SetFieldInfo(xCondition, "m_ParameterName", condition.parameter);
                        SetFieldInfo(xCondition, "m_Type", (X3.PlayableAnimator.ConditionType) condition.mode);
                        SetFieldInfo(xCondition, "m_Threshold", condition.threshold);
                        xTransition.AddCondition(xCondition);
                    }

                    xLayer.GetState<StateMotion>(state.state.name).AddTransition(xTransition);
                }
            }
        }

        public static void SetFieldInfo(object ins, string fieldName, object fieldValue)
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

        public static object GetFieldInfo(object ins, string fieldName)
        {
            if (null == ins)
            {
                return null;
            }

            var fieldInfo = ins.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (null == fieldInfo)
            {
                return null;
            }

            return fieldInfo.GetValue(ins);
        }
    }
}
