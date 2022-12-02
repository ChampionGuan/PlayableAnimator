using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

namespace X3.PlayableAnimator.Editor
{
    [UnityEditor.CustomEditor(typeof(PlayableAnimator))]
    public class EditorPlayableAnimator : UnityEditor.Editor
    {
        private static class Styles
        {
            public static GUIContent AnimatorController = UnityEditor.EditorGUIUtility.TrTextContent("Controller");
            public static GUIContent ApplyRootMotion = UnityEditor.EditorGUIUtility.TrTextContent("Apply Root Motion");
            public static GUIContent CullingMode = UnityEditor.EditorGUIUtility.TrTextContent("Culling Mode");
            public static GUIContent UpdateMode = UnityEditor.EditorGUIUtility.TrTextContent("Update Mode");
            public static GUIContent Avatar = UnityEditor.EditorGUIUtility.TrTextContent("Avatar");
            public static GUIContent Speed = UnityEditor.EditorGUIUtility.TrTextContent("Speed");
        }

        private System.Text.StringBuilder m_Text = new System.Text.StringBuilder();
        private UnityEditor.SerializedProperty m_DefaultAnimatorController;
        private UnityEditor.SerializedProperty m_ApplyRootMotion;
        private UnityEditor.SerializedProperty m_CullingMode;
        private UnityEditor.SerializedProperty m_UpdateMode;
        private UnityEditor.SerializedProperty m_Speed;
        private UnityEditor.SerializedProperty m_Avatar;

        private GameObject m_Root;

        private PlayableAnimator playableAnimator
        {
            get => target as PlayableAnimator;
        }

        public void OnEnable()
        {
            m_DefaultAnimatorController = serializedObject.FindProperty("m_DefaultAnimatorController");
            m_ApplyRootMotion = serializedObject.FindProperty("m_ApplyRootMotion");
            m_CullingMode = serializedObject.FindProperty("m_CullingMode");
            m_UpdateMode = serializedObject.FindProperty("m_UpdateMode");
            m_Speed = serializedObject.FindProperty("m_Speed");
            m_Avatar = serializedObject.FindProperty("m_Avatar");
            m_Root = playableAnimator.gameObject;
            playableAnimator.animator.runtimeAnimatorController = null;
            playableAnimator.animator.hideFlags = HideFlags.HideInInspector;
        }

        public void OnDestroy()
        {
            if (null == m_Root || null != m_Root.GetComponent<PlayableAnimator>())
            {
                return;
            }

            if (Application.isPlaying)
            {
                playableAnimator.animator.hideFlags = HideFlags.None;
            }
            else
            {
                DestroyImmediate(playableAnimator.animator, true);
            }
        }

        public override void OnInspectorGUI()
        {
            m_Text.Clear();
            var ctrl = playableAnimator.runtimeAnimatorController;

            UnityEditor.EditorGUI.BeginChangeCheck();
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.PropertyField(m_DefaultAnimatorController, Styles.AnimatorController);
            if (UnityEditor.EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                playableAnimator.runtimeAnimatorController = (AnimatorController) m_DefaultAnimatorController.objectReferenceValue;
            }

            GUILayout.Space(2);
            if (null != ctrl && GUILayout.Button("○", GUILayout.Width(22)))
            {
                var uCtrl = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(ctrl.unityInsID));
                if (null != uCtrl)
                {
                    EditorGUIUtility.PingObject(uCtrl);
                }
            }

            UnityEditor.EditorGUILayout.EndHorizontal();

            if (null == ctrl)
            {
                UnityEditor.EditorGUILayout.HelpBox("Please set a playable animator controller", UnityEditor.MessageType.Info);
            }

            UnityEditor.EditorGUI.BeginChangeCheck();
            UnityEditor.EditorGUILayout.PropertyField(m_Avatar, Styles.Avatar);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                playableAnimator.avatar = (Avatar) m_Avatar.objectReferenceValue;
            }

            UnityEditor.EditorGUI.BeginChangeCheck();
            UnityEditor.EditorGUILayout.PropertyField(m_ApplyRootMotion, Styles.ApplyRootMotion);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                playableAnimator.applyRootMotion = m_ApplyRootMotion.boolValue;
            }

            if (null != ctrl && !m_ApplyRootMotion.boolValue)
            {
                UnityEditor.EditorGUILayout.HelpBox("Root position or rotation are controlled by curves", UnityEditor.MessageType.Info);
            }

            UnityEditor.EditorGUI.BeginChangeCheck();
            UnityEditor.EditorGUILayout.PropertyField(m_CullingMode, Styles.CullingMode);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                playableAnimator.cullingMode = (AnimatorCullingMode) m_CullingMode.intValue;
            }

            UnityEditor.EditorGUILayout.PropertyField(m_UpdateMode, Styles.UpdateMode);
            UnityEditor.EditorGUILayout.PropertyField(m_Speed, Styles.Speed);

            if (null != ctrl)
            {
                LayersInformation(ctrl);
                ParametersInformation(ctrl);
                UnityEditor.EditorGUILayout.Separator();
                UnityEditor.EditorGUILayout.HelpBox(m_Text.ToString(), UnityEditor.MessageType.Info);
                ShowAnimationClips(ctrl);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void LayersInformation(AnimatorController ctrl)
        {
            if (ctrl.layersCount < 1)
            {
                return;
            }

            for (int i = 0; i < ctrl.layersCount; i++)
            {
                var layer = ctrl.GetLayer(i);
                m_Text.AppendLine($"layer: {layer.name}  defaultWeight: {layer.defaultWeight}   defaultSpeed: {EditorPlayableAnimatorUtility.GetFieldInfo(layer, "m_DefaultSpeed")}   blendingType: {layer.blendingType.ToString()}");
                m_Text.AppendLine($"default state: {EditorPlayableAnimatorUtility.GetFieldInfo(layer, "m_DefaultStateName")}");

                string text = "current state: ";
                if (null != layer.currState && !string.IsNullOrEmpty(layer.currState.name))
                {
                    if ((float) EditorPlayableAnimatorUtility.GetFieldInfo(layer, "m_BlendTick") > 0)
                    {
                        text += $"{layer.prevState.name} --> {layer.currState.name}";
                    }
                    else
                    {
                        text += layer.currState.name;
                    }

                    text += $"  length: {layer.currState.length}  normalizedTime: {(int) (layer.currState.normalizedTime * 100)}%";
                }

                m_Text.AppendLine(text);

                // text = "all states:";
                // for (int j = 0; j < layer.statesCount; j++)
                // {
                //     var state = layer.GetState(j);
                //     text += $"  {state.name}";
                // }
                // m_Text.AppendLine(text);

                m_Text.AppendLine();
            }
        }

        private void ParametersInformation(AnimatorController ctrl)
        {
            if (ctrl.parametersCount < 1)
            {
                return;
            }

            m_Text.AppendLine("parameters");

            string text = "";
            for (int i = 0; i < ctrl.parametersCount; i++)
            {
                var parameter = ctrl.GetParameter(i);
                text += $"{parameter.name}: ";
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                    case AnimatorControllerParameterType.Trigger:
                        text += parameter.defaultBool;
                        break;
                    case AnimatorControllerParameterType.Float:
                        text += parameter.defaultFloat;
                        break;
                    case AnimatorControllerParameterType.Int:
                        text += parameter.defaultInt;
                        break;
                }

                text += "  ";
            }

            m_Text.AppendLine(text);
            // m_Text.AppendLine();
        }

        private void ShowAnimationClips(AnimatorController ctrl)
        {
            if (ctrl.animationClips.Count < 1)
            {
                return;
            }

            UnityEditor.EditorGUI.indentLevel += 2;
            UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            foreach (var clip in ctrl.animationClips)
            {
                UnityEditor.EditorGUILayout.ObjectField(new GUIContent(clip.name), clip, typeof(AnimationClip));
            }

            UnityEditor.EditorGUILayout.EndVertical();
            UnityEditor.EditorGUI.indentLevel -= 2;
        }

        private AnimatorController GetController()
        {
            AnimatorController ctrl = null;
            if (Application.isPlaying)
            {
                ctrl = EditorPlayableAnimatorUtility.GetFieldInfo(playableAnimator, "m_RuntimeAnimatorController") as AnimatorController;
            }
            else
            {
                ctrl = EditorPlayableAnimatorUtility.GetFieldInfo(playableAnimator, "m_DefaultAnimatorController") as AnimatorController;
            }

            return ctrl;
        }
    }
}