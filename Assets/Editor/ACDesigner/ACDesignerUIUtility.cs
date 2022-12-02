using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;

namespace ACDesigner
{
    public static class ACDesignerUIUtility
    {
        private static GUIStyle bigHeaderLabelGUIStyle;

        public static GUIStyle BigHeaderLabelGUIStyle
        {
            get
            {
                if (bigHeaderLabelGUIStyle == null)
                {
                    bigHeaderLabelGUIStyle = new GUIStyle();
                    bigHeaderLabelGUIStyle.padding = new RectOffset(4, 4, 4, 4);
                    bigHeaderLabelGUIStyle.margin = new RectOffset(4, 4, 4, 4);
                    bigHeaderLabelGUIStyle.fixedHeight = 0;
                    bigHeaderLabelGUIStyle.fontSize = 14;
                    bigHeaderLabelGUIStyle.alignment = TextAnchor.MiddleLeft;
                    bigHeaderLabelGUIStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
                    bigHeaderLabelGUIStyle.focused.textColor = Color.white;
                }

                if (bigHeaderLabelGUIStyle.normal.background == null)
                {
                    Color[] pix = new Color[1];
                    pix[0] = new Color(0.55f, 0.55f, 0.55f); // new Color(192.0f / 255.0f, 192.0f / 255.0f, 192.0f / 255.0f);
                    Texture2D result = new Texture2D(1, 1);
                    result.SetPixels(pix);
                    result.Apply();
                    bigHeaderLabelGUIStyle.normal.background = result;
                    bigHeaderLabelGUIStyle.focused.background = result;
                    bigHeaderLabelGUIStyle.hover.background = result;
                }

                return bigHeaderLabelGUIStyle;
            }
        }

        private static GUIStyle bigHeaderMiddleLabelGUIStyle;

        public static GUIStyle BigHeaderMiddleLabelGUIStyle
        {
            get
            {
                if (bigHeaderMiddleLabelGUIStyle == null)
                {
                    bigHeaderMiddleLabelGUIStyle = new GUIStyle();
                    bigHeaderMiddleLabelGUIStyle.padding = new RectOffset(4, 4, 4, 4);
                    bigHeaderMiddleLabelGUIStyle.margin = new RectOffset(4, 4, 4, 4);
                    bigHeaderMiddleLabelGUIStyle.fixedHeight = 0;
                    bigHeaderMiddleLabelGUIStyle.fontSize = 14;
                    bigHeaderMiddleLabelGUIStyle.alignment = TextAnchor.MiddleCenter;
                    bigHeaderMiddleLabelGUIStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
                    bigHeaderMiddleLabelGUIStyle.focused.textColor = Color.white;
                }

                if (bigHeaderMiddleLabelGUIStyle.normal.background == null)
                {
                    Color[] pix = new Color[1];
                    pix[0] = new Color(0.55f, 0.55f, 0.55f); // new Color(192.0f / 255.0f, 192.0f / 255.0f, 192.0f / 255.0f);
                    Texture2D result = new Texture2D(1, 1);
                    result.SetPixels(pix);
                    result.Apply();
                    bigHeaderMiddleLabelGUIStyle.normal.background = result;
                    bigHeaderMiddleLabelGUIStyle.focused.background = result;
                    bigHeaderMiddleLabelGUIStyle.hover.background = result;
                }

                return bigHeaderMiddleLabelGUIStyle;
            }
        }

        private static GUIStyle boldHeaderLabelGUIStyle;

        public static GUIStyle BoldHeaderLabelGUIStyle
        {
            get
            {
                if (boldHeaderLabelGUIStyle == null)
                {
                    boldHeaderLabelGUIStyle = new GUIStyle(EditorStyles.label);
                    boldHeaderLabelGUIStyle.fontStyle = FontStyle.Bold;
                }

                return boldHeaderLabelGUIStyle;
            }
        }

        private static GUIStyle toolbarButtonSelectionGUIStyle = null;

        public static GUIStyle ToolbarButtonSelectionGUIStyle
        {
            get
            {
                if (toolbarButtonSelectionGUIStyle == null)
                {
                    toolbarButtonSelectionGUIStyle = new GUIStyle(EditorStyles.toolbarButton);
                    toolbarButtonSelectionGUIStyle.normal.background = toolbarButtonSelectionGUIStyle.active.background;
                }

                return toolbarButtonSelectionGUIStyle;
            }
        }

        private static GUIStyle graphStatusGUIStyle = null;

        public static GUIStyle GraphStatusGUIStyle
        {
            get
            {
                if (graphStatusGUIStyle == null)
                {
                    graphStatusGUIStyle = new GUIStyle(GUI.skin.label);
                    graphStatusGUIStyle.alignment = TextAnchor.MiddleLeft;
                    graphStatusGUIStyle.fontSize = 20;
                    graphStatusGUIStyle.fontStyle = FontStyle.Bold;
                    if (EditorGUIUtility.isProSkin)
                    {
                        graphStatusGUIStyle.normal.textColor = new Color(0.7058f, 0.7058f, 0.7058f);
                    }
                    else
                    {
                        graphStatusGUIStyle.normal.textColor = new Color(0.8058f, 0.8058f, 0.8058f);
                    }
                }

                return graphStatusGUIStyle;
            }
        }

        private static GUIStyle selectionGUIStyle = null;

        public static GUIStyle SelectionGUIStyle
        {
            get
            {
                if (selectionGUIStyle == null)
                {
                    Texture2D val = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    Color val2 = (!EditorGUIUtility.isProSkin) ? new Color(0.243f, 0.5686f, 0.839f, 0.5f) : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
                    val.SetPixel(1, 1, val2);
                    ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
                    val.Apply();
                    selectionGUIStyle = new GUIStyle(GUI.skin.box);
                    selectionGUIStyle.normal.background = val;
                    selectionGUIStyle.active.background = val;
                    selectionGUIStyle.hover.background = val;
                    selectionGUIStyle.focused.background = val;
                    selectionGUIStyle.normal.textColor = Color.white;
                    selectionGUIStyle.active.textColor = Color.white;
                    selectionGUIStyle.hover.textColor = Color.white;
                    selectionGUIStyle.focused.textColor = Color.white;
                }

                return selectionGUIStyle;
            }
        }

        private static GUIStyle grayGUIStyle = null;

        public static GUIStyle GrayGUIStyle
        {
            get
            {
                if (grayGUIStyle == null)
                {
                    Texture2D val = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    Color val2 = (!EditorGUIUtility.isProSkin) ? new Color(0.545f, 0.545f, 0.545f, 1f) : new Color(0.545f, 0.545f, 0.5451f, 1f);
                    val.SetPixel(1, 1, val2);
                    ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
                    val.Apply();
                    grayGUIStyle = new GUIStyle(GUI.skin.box);
                    grayGUIStyle.normal.background = val;
                    grayGUIStyle.active.background = val;
                    grayGUIStyle.hover.background = val;
                    grayGUIStyle.focused.background = val;
                    grayGUIStyle.normal.textColor = Color.white;
                    grayGUIStyle.active.textColor = Color.white;
                    grayGUIStyle.hover.textColor = Color.white;
                    grayGUIStyle.focused.textColor = Color.white;
                }

                return grayGUIStyle;
            }
        }

        private static GUIStyle blackGUIStyle = null;

        public static GUIStyle BlackGUIStyle
        {
            get
            {
                if (blackGUIStyle == null)
                {
                    Texture2D val = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    Color val2 = (!EditorGUIUtility.isProSkin) ? new Color(0.33f, 0.33f, 0.33f, 1f) : new Color(0.33f, 0.33f, 0.334f, 1f);
                    val.SetPixel(1, 1, val2);
                    ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
                    val.Apply();
                    blackGUIStyle = new GUIStyle(GUI.skin.box);
                    blackGUIStyle.normal.background = val;
                    blackGUIStyle.active.background = val;
                    blackGUIStyle.hover.background = val;
                    blackGUIStyle.focused.background = val;
                    blackGUIStyle.normal.textColor = Color.white;
                    blackGUIStyle.active.textColor = Color.white;
                    blackGUIStyle.hover.textColor = Color.white;
                    blackGUIStyle.focused.textColor = Color.white;
                }

                return blackGUIStyle;
            }
        }

        private static GUIStyle taskIdentifyCompactGUIStyle = null;

        public static GUIStyle TaskIdentifyCompactGUIStyle
        {
            get
            {
                if (taskIdentifyCompactGUIStyle == null)
                {
                    taskIdentifyCompactGUIStyle = InitTaskGUIStyle(LoadTaskTexture("TaskIdentifyCompact.png"), new RectOffset(5, 4, 4, 5));
                }

                return taskIdentifyCompactGUIStyle;
            }
        }

        private static GUIStyle taskIdentifySelectedCompactGUIStyle = null;

        public static GUIStyle TaskIdentifySelectedCompactGUIStyle
        {
            get
            {
                if (taskIdentifySelectedCompactGUIStyle == null)
                {
                    taskIdentifySelectedCompactGUIStyle = InitTaskGUIStyle(LoadTaskTexture("TaskIdentifySelectedCompact.png"), new RectOffset(5, 4, 4, 4));
                }

                return taskIdentifySelectedCompactGUIStyle;
            }
        }

        private static GUIStyle[] taskGUIStyle = new GUIStyle[9];

        public static GUIStyle GetTaskGUIStyle(int colorIndex)
        {
            if (taskGUIStyle[colorIndex] == null)
            {
                taskGUIStyle[colorIndex] = InitTaskGUIStyle(LoadTaskTexture("Task" + ColorIndexToColorString(colorIndex) + ".png"), new RectOffset(5, 3, 3, 5));
            }

            return taskGUIStyle[colorIndex];
        }

        private static GUIStyle[] taskSelectedGUIStyle = new GUIStyle[9];

        public static GUIStyle GetTaskSelectedGUIStyle(int colorIndex)
        {
            if (taskSelectedGUIStyle[colorIndex] == null)
            {
                taskSelectedGUIStyle[colorIndex] = InitTaskGUIStyle(LoadTaskTexture("TaskSelected" + ColorIndexToColorString(colorIndex) + ".png"), new RectOffset(5, 4, 4, 4));
            }

            return taskSelectedGUIStyle[colorIndex];
        }

        private static GUIStyle taskRunningGUIStyle = null;

        public static GUIStyle TaskRunningGUIStyle
        {
            get
            {
                if (taskRunningGUIStyle == null)
                {
                    taskRunningGUIStyle = InitTaskGUIStyle(LoadTaskTexture("TaskRunning.png"), new RectOffset(5, 3, 3, 5));
                }

                return taskRunningGUIStyle;
            }
        }

        private static GUIStyle taskRunningSelectedGUIStyle = null;

        public static GUIStyle TaskRunningSelectedGUIStyle
        {
            get
            {
                if (taskRunningSelectedGUIStyle == null)
                {
                    taskRunningSelectedGUIStyle = InitTaskGUIStyle(LoadTaskTexture("TaskRunningSelected.png"), new RectOffset(5, 4, 4, 4));
                }

                return taskRunningSelectedGUIStyle;
            }
        }

        private static GUIStyle taskDescriptionGUIStyle = null;

        public static GUIStyle TaskDescriptionGUIStyle
        {
            get
            {
                if (taskDescriptionGUIStyle == null)
                {
                    Texture2D val = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    if (EditorGUIUtility.isProSkin)
                    {
                        val.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
                    }
                    else
                    {
                        val.SetPixel(1, 1, new Color(0.75f, 0.75f, 0.75f));
                    }

                    ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
                    val.Apply();
                    taskDescriptionGUIStyle = new GUIStyle(GUI.skin.box);
                    taskDescriptionGUIStyle.normal.background = val;
                    taskDescriptionGUIStyle.active.background = val;
                    taskDescriptionGUIStyle.hover.background = val;
                    taskDescriptionGUIStyle.focused.background = val;
                }

                return taskDescriptionGUIStyle;
            }
        }

        private static GUIStyle taskCommentGUIStyle = null;

        public static GUIStyle TaskCommentGUIStyle
        {
            get
            {
                if (taskCommentGUIStyle == null)
                {
                    taskCommentGUIStyle = new GUIStyle(GUI.skin.label);
                    taskCommentGUIStyle.alignment = TextAnchor.UpperCenter;
                    taskCommentGUIStyle.fontSize = 12;
                    taskCommentGUIStyle.fontStyle = FontStyle.Normal;
                    taskCommentGUIStyle.wordWrap = true;
                    taskCommentGUIStyle.alignment = TextAnchor.MiddleLeft;
                }

                return taskCommentGUIStyle;
            }
        }

        private static GUIStyle transparentButtonGUIStyle = null;

        public static GUIStyle TransparentButtonGUIStyle
        {
            get
            {
                if (transparentButtonGUIStyle == null)
                {
                    transparentButtonGUIStyle = new GUIStyle(GUI.skin.button);
                    transparentButtonGUIStyle.border = new RectOffset(0, 0, 0, 0);
                    transparentButtonGUIStyle.margin = new RectOffset(4, 4, 2, 2);
                    transparentButtonGUIStyle.padding = new RectOffset(2, 2, 1, 0);
                    transparentButtonGUIStyle.normal.background = null;
                    transparentButtonGUIStyle.active.background = null;
                    transparentButtonGUIStyle.hover.background = null;
                    transparentButtonGUIStyle.focused.background = null;
                    transparentButtonGUIStyle.normal.textColor = Color.white;
                    transparentButtonGUIStyle.active.textColor = Color.white;
                    transparentButtonGUIStyle.hover.textColor = Color.white;
                    transparentButtonGUIStyle.focused.textColor = Color.white;
                }

                return transparentButtonGUIStyle;
            }
        }

        private static GUIStyle taskInspectorCommentGUIStyle = null;

        public static GUIStyle TaskInspectorCommentGUIStyle
        {
            get
            {
                if (taskInspectorCommentGUIStyle == null)
                {
                    taskInspectorCommentGUIStyle = new GUIStyle(GUI.skin.textArea);
                    taskInspectorCommentGUIStyle.wordWrap = true;
                }

                return taskInspectorCommentGUIStyle;
            }
        }

        private static GUIStyle treeVariableDescTextGUIStyle = null;

        public static GUIStyle TreeVariableDescTextGUIStyle
        {
            get
            {
                if (treeVariableDescTextGUIStyle == null)
                {
                    treeVariableDescTextGUIStyle = new GUIStyle(GUI.skin.textArea);
                    treeVariableDescTextGUIStyle.wordWrap = true;
                }

                return treeVariableDescTextGUIStyle;
            }
        }

        private static GUIStyle treeVariableDescLabelGUIStyle = null;

        public static GUIStyle TreeVariableDescLabelGUIStyle
        {
            get
            {
                if (treeVariableDescLabelGUIStyle == null)
                {
                    treeVariableDescLabelGUIStyle = new GUIStyle(GUI.skin.label);
                    treeVariableDescLabelGUIStyle.wordWrap = true;
                    treeVariableDescLabelGUIStyle.alignment = TextAnchor.UpperRight;
                    treeVariableDescLabelGUIStyle.fontSize = 11;
                    treeVariableDescLabelGUIStyle.normal.textColor = Color.gray;
                }

                return treeVariableDescLabelGUIStyle;
            }
        }

        private static GUIStyle sharedVariableToolbarPopup = null;

        public static GUIStyle SharedVariableToolbarPopup
        {
            get
            {
                if (sharedVariableToolbarPopup == null)
                {
                    sharedVariableToolbarPopup = new GUIStyle(EditorStyles.toolbarPopup);
                    sharedVariableToolbarPopup.margin = new RectOffset(4, 4, 0, 0);
                }

                return sharedVariableToolbarPopup;
            }
        }

        private static GUIStyle selectedBackgroundGUIStyle = null;

        public static GUIStyle SelectedBackgroundGUIStyle
        {
            get
            {
                if (selectedBackgroundGUIStyle == null)
                {
                    Texture2D val = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    Color val2 = (!EditorGUIUtility.isProSkin) ? new Color(0.243f, 0.5686f, 0.839f, 0.5f) : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
                    val.SetPixel(1, 1, val2);
                    ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
                    val.Apply();
                    selectedBackgroundGUIStyle = new GUIStyle();
                    selectedBackgroundGUIStyle.border = new RectOffset(0, 0, 0, 0);
                    selectedBackgroundGUIStyle.margin = new RectOffset(0, 0, -2, 2);
                    selectedBackgroundGUIStyle.normal.background = val;
                    selectedBackgroundGUIStyle.active.background = val;
                    selectedBackgroundGUIStyle.hover.background = val;
                    selectedBackgroundGUIStyle.focused.background = val;
                }

                return selectedBackgroundGUIStyle;
            }
        }

        private static GUIStyle labelWrapGUIStyle = null;

        public static GUIStyle LabelWrapGUIStyle
        {
            get
            {
                if (labelWrapGUIStyle == null)
                {
                    labelWrapGUIStyle = new GUIStyle(GUI.skin.label);
                    labelWrapGUIStyle.wordWrap = true;
                    labelWrapGUIStyle.alignment = TextAnchor.MiddleLeft;
                }

                return labelWrapGUIStyle;
            }
        }

        private static GUIStyle pathTitleGUIStyle = null;

        public static GUIStyle PathTitleGUIStyle
        {
            get
            {
                if (pathTitleGUIStyle == null)
                {
                    pathTitleGUIStyle = new GUIStyle(GUI.skin.label);
                    pathTitleGUIStyle.wordWrap = true;
                    pathTitleGUIStyle.alignment = TextAnchor.UpperRight;
                    pathTitleGUIStyle.fontSize = 11;
                    pathTitleGUIStyle.normal.textColor = Color.gray;
                }

                return pathTitleGUIStyle;
            }
        }

        private static GUIStyle treeDirectoryGUIStyle = null;

        public static GUIStyle TreeDirectoryGUIStyle
        {
            get
            {
                if (treeDirectoryGUIStyle == null)
                {
                    treeDirectoryGUIStyle = new GUIStyle(GUI.skin.label);
                    treeDirectoryGUIStyle.wordWrap = true;
                    treeDirectoryGUIStyle.alignment = TextAnchor.MiddleRight;
                    treeDirectoryGUIStyle.fontSize = 9;
                    treeDirectoryGUIStyle.normal.textColor = Color.gray;
                }

                return treeDirectoryGUIStyle;
            }
        }

        private static GUIStyle transparentGUIStyle = null;

        public static GUIStyle TransparentGUIStyle
        {
            get
            {
                if (transparentGUIStyle == null)
                {
                    transparentGUIStyle = new GUIStyle(GUI.skin.label);
                    transparentGUIStyle.alignment = TextAnchor.MiddleCenter;
                    transparentGUIStyle.fontSize = 1;
                    transparentGUIStyle.normal.textColor = Color.white;
                    transparentGUIStyle.hover.textColor = Color.white;
                }

                return transparentGUIStyle;
            }
        }


        private static GUIStyle taskFoldoutGUIStyle = null;

        public static GUIStyle TaskFoldoutGUIStyle
        {
            get
            {
                if (taskFoldoutGUIStyle == null)
                {
                    taskFoldoutGUIStyle = new GUIStyle(EditorStyles.foldout);
                    taskFoldoutGUIStyle.alignment = TextAnchor.MiddleLeft;
                    taskFoldoutGUIStyle.fontSize = 13;
                    taskFoldoutGUIStyle.fontStyle = FontStyle.Bold;
                }

                return taskFoldoutGUIStyle;
            }
        }

        private static GUIStyle preferencesPaneGUIStyle = null;

        public static GUIStyle PreferencesPaneGUIStyle
        {
            get
            {
                if (preferencesPaneGUIStyle == null)
                {
                    preferencesPaneGUIStyle = new GUIStyle(GUI.skin.box);
                    preferencesPaneGUIStyle.normal.background = LoadTexture("Background.png");
                }

                return preferencesPaneGUIStyle;
            }
        }

        private static GUIStyle labelTitleGUIStyle = null;

        public static GUIStyle LabelTitleGUIStyle
        {
            get
            {
                if (labelTitleGUIStyle == null)
                {
                    labelTitleGUIStyle = new GUIStyle(GUI.skin.label);
                    labelTitleGUIStyle.wordWrap = true;
                    labelTitleGUIStyle.alignment = TextAnchor.MiddleCenter;
                    labelTitleGUIStyle.fontSize = 14;
                }

                return labelTitleGUIStyle;
            }
        }

        private static GUIStyle plainButtonGUIStyle = null;

        public static GUIStyle PlainButtonGUIStyle
        {
            get
            {
                if (plainButtonGUIStyle == null)
                {
                    plainButtonGUIStyle = new GUIStyle(GUI.skin.button);
                    plainButtonGUIStyle.border = new RectOffset(0, 0, 0, 0);
                    plainButtonGUIStyle.margin = new RectOffset(0, 0, 2, 2);
                    plainButtonGUIStyle.padding = new RectOffset(0, 0, 1, 0);
                    plainButtonGUIStyle.normal.background = null;
                    plainButtonGUIStyle.active.background = null;
                    plainButtonGUIStyle.hover.background = null;
                    plainButtonGUIStyle.focused.background = null;
                    plainButtonGUIStyle.normal.textColor = Color.white;
                    plainButtonGUIStyle.active.textColor = Color.white;
                    plainButtonGUIStyle.hover.textColor = Color.white;
                    plainButtonGUIStyle.focused.textColor = Color.white;
                }

                return plainButtonGUIStyle;
            }
        }

        private static GUIStyle propertyBoxGUIStyle = null;

        public static GUIStyle PropertyBoxGUIStyle
        {
            get
            {
                if (propertyBoxGUIStyle == null)
                {
                    propertyBoxGUIStyle = new GUIStyle();
                    propertyBoxGUIStyle.padding = new RectOffset(2, 2, 0, 0);
                }

                return propertyBoxGUIStyle;
            }
        }

        private static GUIStyle graphBackgroundGUIStyle = null;

        public static GUIStyle GraphBackgroundGUIStyle
        {
            get
            {
                if (graphBackgroundGUIStyle == null)
                {
                    Texture2D val = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    if (EditorGUIUtility.isProSkin)
                    {
                        val.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
                    }
                    else
                    {
                        val.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
                    }

                    ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
                    val.Apply();
                    graphBackgroundGUIStyle = new GUIStyle(GUI.skin.box);
                    graphBackgroundGUIStyle.normal.background = val;
                    graphBackgroundGUIStyle.active.background = val;
                    graphBackgroundGUIStyle.hover.background = val;
                    graphBackgroundGUIStyle.focused.background = val;
                }

                return graphBackgroundGUIStyle;
            }
        }

        private static Texture2D variableWatchButtonTexture = null;

        public static Texture2D VariableWatchButtonTexture
        {
            get
            {
                if (variableWatchButtonTexture == null)
                {
                    variableWatchButtonTexture = LoadTexture("VariableWatchButton.png");
                }

                return variableWatchButtonTexture;
            }
        }

        private static Texture2D gearTexture = null;

        public static Texture2D GearTexture
        {
            get
            {
                if (gearTexture == null)
                {
                    gearTexture = LoadTexture("GearIcon.png");
                }

                return gearTexture;
            }
        }

        private static Texture2D breakpointTexture = null;

        public static Texture2D BreakpointTexture
        {
            get
            {
                if (breakpointTexture == null)
                {
                    breakpointTexture = LoadTexture("BreakpointIcon.png", false);
                }

                return breakpointTexture;
            }
        }

        private static Texture2D errorIconTexture = null;

        public static Texture2D ErrorIconTexture
        {
            get
            {
                if (errorIconTexture == null)
                {
                    errorIconTexture = LoadTexture("ErrorIcon.png");
                }

                return errorIconTexture;
            }
        }

        private static Texture2D conditionalAbortBothTexture = null;

        public static Texture2D ConditionalAbortBothTexture
        {
            get
            {
                if (conditionalAbortBothTexture == null)
                {
                    conditionalAbortBothTexture = LoadTexture("ConditionalAbortBothIcon.png");
                }

                return conditionalAbortBothTexture;
            }
        }

        private static Texture2D conditionalAbortLowerPriorityTexture = null;

        public static Texture2D ConditionalAbortLowerPriorityTexture
        {
            get
            {
                if (conditionalAbortLowerPriorityTexture == null)
                {
                    conditionalAbortLowerPriorityTexture = LoadTexture("ConditionalAbortLowerPriorityIcon.png");
                }

                return conditionalAbortLowerPriorityTexture;
            }
        }

        private static Texture2D conditionalAbortSelfTexture = null;

        public static Texture2D ConditionalAbortSelfTexture
        {
            get
            {
                if (conditionalAbortSelfTexture == null)
                {
                    conditionalAbortSelfTexture = LoadTexture("ConditionalAbortSelfIcon.png");
                }

                return conditionalAbortSelfTexture;
            }
        }

        private static Texture2D contentSeparatorTexture = null;

        public static Texture2D ContentSeparatorTexture
        {
            get
            {
                if (contentSeparatorTexture == null)
                {
                    contentSeparatorTexture = LoadTexture("ContentSeparator.png");
                }

                return contentSeparatorTexture;
            }
        }

        private static Texture2D variableDeleteButtonTexture = null;

        public static Texture2D VariableDeleteButtonTexture
        {
            get
            {
                if (variableDeleteButtonTexture == null)
                {
                    variableDeleteButtonTexture = LoadTexture("VariableDeleteButton.png");
                }

                return variableDeleteButtonTexture;
            }
        }

        private static Texture2D upArrowButtonTexture = null;

        public static Texture2D UpArrowButtonTexture
        {
            get
            {
                if (upArrowButtonTexture == null)
                {
                    upArrowButtonTexture = LoadTexture("UpArrowButton.png");
                }

                return upArrowButtonTexture;
            }
        }

        private static Texture2D downArrowButtonTexture = null;

        public static Texture2D DownArrowButtonTexture
        {
            get
            {
                if (downArrowButtonTexture == null)
                {
                    downArrowButtonTexture = LoadTexture("DownArrowButton.png");
                }

                return downArrowButtonTexture;
            }
        }

        private static Texture2D taskConnectionCollapsedTexture = null;

        public static Texture2D TaskConnectionCollapsedTexture
        {
            get
            {
                if (taskConnectionCollapsedTexture == null)
                {
                    taskConnectionCollapsedTexture = LoadTexture("TaskConnectionCollapsed.png");
                }

                return taskConnectionCollapsedTexture;
            }
        }

        private static Texture2D expandTaskTexture = null;

        public static Texture2D ExpandTaskTexture
        {
            get
            {
                if (expandTaskTexture == null)
                {
                    expandTaskTexture = LoadTexture("TaskExpandIcon.png", false);
                }

                return expandTaskTexture;
            }
        }

        private static Texture2D collapseTaskTexture = null;

        public static Texture2D CollapseTaskTexture
        {
            get
            {
                if (collapseTaskTexture == null)
                {
                    collapseTaskTexture = LoadTexture("TaskCollapseIcon.png", false);
                }

                return collapseTaskTexture;
            }
        }

        private static Texture2D disableTaskTexture = null;

        public static Texture2D DisableTaskTexture
        {
            get
            {
                if (disableTaskTexture == null)
                {
                    disableTaskTexture = LoadTexture("TaskDisableIcon.png", false);
                }

                return disableTaskTexture;
            }
        }

        private static Texture2D enableTaskTexture = null;

        public static Texture2D EnableTaskTexture
        {
            get
            {
                if (enableTaskTexture == null)
                {
                    enableTaskTexture = LoadTexture("TaskEnableIcon.png", false);
                }

                return enableTaskTexture;
            }
        }

        private static Texture2D taskConnectionRunningBottomTexture = null;

        public static Texture2D TaskConnectionRunningBottomTexture
        {
            get
            {
                if (taskConnectionRunningBottomTexture == null)
                {
                    taskConnectionRunningBottomTexture = LoadTaskTexture("TaskConnectionRunningBottom.png");
                }

                return taskConnectionRunningBottomTexture;
            }
        }

        private static Texture2D taskConnectionRunningTopTexture = null;

        public static Texture2D TaskConnectionRunningTopTexture
        {
            get
            {
                if (taskConnectionRunningTopTexture == null)
                {
                    taskConnectionRunningTopTexture = LoadTaskTexture("TaskConnectionRunningTop.png");
                }

                return taskConnectionRunningTopTexture;
            }
        }

        private static Texture2D[] taskConnectionTopTexture = new Texture2D[9];

        public static Texture2D GetTaskConnectionTopTexture(int colorIndex)
        {
            if (taskConnectionTopTexture[colorIndex] == null)
            {
                taskConnectionTopTexture[colorIndex] = LoadTaskTexture("TaskConnectionTop" + ColorIndexToColorString(colorIndex) + ".png");
            }

            return taskConnectionTopTexture[colorIndex];
        }

        private static Texture2D[] taskConnectionBottomTexture = new Texture2D[9];

        public static Texture2D GetTaskConnectionBottomTexture(int colorIndex)
        {
            if (taskConnectionBottomTexture[colorIndex] == null)
            {
                taskConnectionBottomTexture[colorIndex] = LoadTaskTexture("TaskConnectionBottom" + ColorIndexToColorString(colorIndex) + ".png");
            }

            return taskConnectionBottomTexture[colorIndex];
        }

        private static Texture2D executionSuccessRepeatTexture = null;

        public static Texture2D ExecutionSuccessRepeatTexture
        {
            get
            {
                if (executionSuccessRepeatTexture == null)
                {
                    executionSuccessRepeatTexture = LoadTexture("ExecutionSuccessRepeat.png", false);
                }

                return executionSuccessRepeatTexture;
            }
        }

        private static Texture2D executionSuccessTexture = null;

        public static Texture2D ExecutionSuccessTexture
        {
            get
            {
                if (executionSuccessTexture == null)
                {
                    executionSuccessTexture = LoadTexture("ExecutionSuccess.png", false);
                }

                return executionSuccessTexture;
            }
        }

        private static Texture2D executionFailureTexture = null;

        public static Texture2D ExecutionFailureTexture
        {
            get
            {
                if (executionFailureTexture == null)
                {
                    executionFailureTexture = LoadTexture("ExecutionFailure.png", false);
                }

                return executionFailureTexture;
            }
        }

        private static Texture2D deleteButtonTexture = null;

        public static Texture2D DeleteButtonTexture
        {
            get
            {
                if (deleteButtonTexture == null)
                {
                    deleteButtonTexture = LoadTexture("DeleteButton.png");
                }

                return deleteButtonTexture;
            }
        }

        private static Texture2D playTexture = null;

        public static Texture2D PlayTexture
        {
            get
            {
                if (playTexture == null)
                {
                    playTexture = LoadTexture("Play.png");
                }

                return playTexture;
            }
        }

        private static Texture2D pauseTexture = null;

        public static Texture2D PauseTexture
        {
            get
            {
                if (pauseTexture == null)
                {
                    pauseTexture = LoadTexture("Pause.png");
                }

                return pauseTexture;
            }
        }

        private static Texture2D stepTexture = null;

        public static Texture2D StepTexture
        {
            get
            {
                if (stepTexture == null)
                {
                    stepTexture = LoadTexture("Step.png");
                }

                return stepTexture;
            }
        }

        public static Texture2D historyBackwardTexture = null;

        public static Texture2D HistoryBackwardTexture
        {
            get
            {
                if (historyBackwardTexture == null)
                {
                    historyBackwardTexture = LoadTexture("HistoryBackward.png");
                }

                return historyBackwardTexture;
            }
        }

        public static Texture2D historyForwardTexture = null;

        public static Texture2D HistoryForwardTexture
        {
            get
            {
                if (historyForwardTexture == null)
                {
                    historyForwardTexture = LoadTexture("HistoryForward.png");
                }

                return historyForwardTexture;
            }
        }

        private static Texture2D variableButtonTexture = null;

        public static Texture2D VariableButtonTexture
        {
            get
            {
                if (variableButtonTexture == null)
                {
                    variableButtonTexture = LoadTexture("VariableButton.png");
                }

                return variableButtonTexture;
            }
        }

        private static Texture2D locationTexture = null;

        public static Texture2D LocationTexture
        {
            get
            {
                if (locationTexture == null)
                {
                    locationTexture = LoadTexture("LocationIcon.png", false);
                }

                return locationTexture;
            }
        }

        private static Texture2D plusSignTexture = null;

        public static Texture2D PlusSignTexture
        {
            get
            {
                if (plusSignTexture == null)
                {
                    plusSignTexture = LoadTexture("PlusSingIcon.png", false);
                }

                return plusSignTexture;
            }
        }


        private static Texture2D plusArrowSignTexture = null;

        public static Texture2D PlusArrowSignTexture
        {
            get
            {
                if (plusArrowSignTexture == null)
                {
                    plusArrowSignTexture = LoadTexture("PlusArrowSingIcon.png", false);
                }

                return plusArrowSignTexture;
            }
        }

        private static Rect drawBoxRect = new Rect();

        public static void DrawBox(float x, float y, float width, float height, GUIStyle style = null)
        {
            drawBoxRect.x = x;
            drawBoxRect.y = y;
            drawBoxRect.width = width;
            drawBoxRect.height = height;
            GUI.Box(drawBoxRect, string.Empty, style);
        }

        public static void DrawContentSeperator(int yOffset = 2)
        {
            DrawContentSeperator(yOffset, 0);
        }

        public static void DrawContentSeperator(int yOffset, int widthExtension)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x = -5f;
            lastRect.y = lastRect.y + (lastRect.height + (float) yOffset);
            lastRect.height = 2f;
            lastRect.width = lastRect.width + (float) (10 + widthExtension);
            GUI.DrawTexture(lastRect, (Texture) (object) ContentSeparatorTexture);
        }

        public static string GetEditorBaseDirectory(UnityEngine.Object obj = null)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            string text = Uri.UnescapeDataString(new UriBuilder(codeBase).Path);
            return Path.GetDirectoryName(text.Substring(Application.dataPath.Length - 6));
        }

        private static Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();

        public static void OpenAsset(UnityEngine.Object asset)
        {
            if (null == asset)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            AssetDatabase.OpenAsset(asset);
        }

        public static Texture2D LoadIcon(string iconName, ScriptableObject obj = null)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return null;
            }

            if (iconCache.ContainsKey(iconName))
            {
                return iconCache[iconName];
            }

            //Texture2D val = null;
            //string name = iconName.Replace("{SkinColor}", (!EditorGUIUtility.isProSkin) ? "Light" : "Dark");
            //Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            //if (manifestResourceStream == null)
            //{
            //    name = string.Format("BehaviorDesignerEditor.Resources.{0}", iconName.Replace("{SkinColor}", (!EditorGUIUtility.isProSkin) ? "Light" : "Dark"));
            //    manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            //}
            //if (null == manifestResourceStream)
            //{
            //    name = string.Format("Assets/Editor/ACDesigner/Resources/{0}", iconName.Replace("{SkinColor}", (!EditorGUIUtility.isProSkin) ? "Light" : "Dark"));
            //    val = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            //}
            //if (manifestResourceStream != null)
            //{
            //    val = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            //    ImageConversion.LoadImage(val, ReadToEnd(manifestResourceStream));
            //    manifestResourceStream.Close();
            //}
            string name = string.Format("Assets/Editor/ACDesigner/Resources/{0}", iconName.Replace("{SkinColor}", (!EditorGUIUtility.isProSkin) ? "Light" : "Dark"));
            Texture2D val = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            if (val == null)
            {
                name = string.Format("Assets/Editor/ACDesigner/Resources/{0}", iconName.Replace("{SkinColor}", ""));
                val = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            }

            if (val != null)
            {
                ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
            }

            iconCache.Add(iconName, val);
            return val;
        }

        private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

        public static Texture2D LoadTexture(string imageName, bool useSkinColor = true, UnityEngine.Object obj = null)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return null;
            }

            if (textureCache.ContainsKey(imageName))
            {
                return textureCache[imageName];
            }

            //Texture2D val = null;
            //string name = string.Format("{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
            //Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            //if (manifestResourceStream == null)
            //{
            //    name = string.Format("BehaviorDesignerEditor.Resources.{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
            //    manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            //}
            //if (null == manifestResourceStream)
            //{
            //    name = string.Format("Assets/Editor/ACDesigner/Resources/{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
            //    val = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            //}
            //if (manifestResourceStream != null)
            //{
            //    val = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            //    ImageConversion.LoadImage(val, ReadToEnd(manifestResourceStream));
            //    manifestResourceStream.Close();
            //}
            string name = string.Format("Assets/Editor/ACDesigner/Resources/{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
            Texture2D val = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
            textureCache.Add(imageName, val);
            return val;
        }

        private static Texture2D LoadTaskTexture(string imageName, bool useSkinColor = true, ScriptableObject obj = null)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return null;
            }

            if (textureCache.ContainsKey(imageName))
            {
                return textureCache[imageName];
            }

            //Texture2D val = null;
            //string name = string.Format("{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
            //Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            //if (manifestResourceStream == null)
            //{
            //    name = string.Format("BehaviorDesignerEditor.Resources.{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
            //    manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            //}
            //if (null == manifestResourceStream)
            //{
            //    name = string.Format("Assets/Editor/AIDesigner/Resources/{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
            //    val = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            //}
            //if (manifestResourceStream != null)
            //{
            //    val = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            //    ImageConversion.LoadImage(val, ReadToEnd(manifestResourceStream));
            //    manifestResourceStream.Close();
            //}
            string name = string.Format("Assets/Editor/AIDesigner/Resources/{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
            Texture2D val = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            if (val == null)
            {
                Debug.Log(string.Format("{0}/Images/Task Backgrounds/{1}{2}", GetEditorBaseDirectory(obj), (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName));
            }

            ((UnityEngine.Object) val).hideFlags = HideFlags.HideAndDontSave;
            textureCache.Add(imageName, val);
            return val;
        }

        private static byte[] ReadToEnd(Stream stream)
        {
            byte[] array = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = stream.Read(array, 0, array.Length)) > 0)
                {
                    memoryStream.Write(array, 0, count);
                }

                return memoryStream.ToArray();
            }
        }

        private static GUIStyle InitTaskGUIStyle(Texture2D texture, RectOffset overflow)
        {
            GUIStyle val = new GUIStyle(GUI.skin.box);
            val.border = new RectOffset(10, 10, 10, 10);
            val.overflow = overflow;
            val.normal.background = texture;
            val.active.background = texture;
            val.hover.background = texture;
            val.focused.background = texture;
            val.normal.textColor = Color.white;
            val.active.textColor = Color.white;
            val.hover.textColor = Color.white;
            val.focused.textColor = Color.white;
            val.stretchHeight = true;
            val.stretchWidth = true;
            val.alignment = TextAnchor.LowerCenter;
            val.wordWrap = false;
            return val;
        }

        private static string ColorIndexToColorString(int index)
        {
            switch (index)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return "Red";
                case 2:
                    return "Pink";
                case 3:
                    return "Brown";
                case 4:
                    return "RedOrange";
                case 5:
                    return "Turquoise";
                case 6:
                    return "Cyan";
                case 7:
                    return "Blue";
                case 8:
                    return "Purple";
                default:
                    return string.Empty;
            }
        }
    }
}