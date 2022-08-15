using System;
using System.Collections.Generic;
using System.Linq;
using Cam;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Comfort.Editor
{
    public static class VRCHelpers
    {
        public static void AddParameter(VRCAvatarDescriptor avatar, string parameterName, AnimationHelpers.AnimMode animMode)
        {
            Debug.Log("Adding parameter: " + parameterName);
            // get the avatar
            VRCExpressionParameters expressionParameters = avatar.expressionParameters;
            if (expressionParameters == null)
            {
                expressionParameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                avatar.expressionParameters = expressionParameters;
            }

            VRCExpressionParameters.Parameter[] parameters = expressionParameters.parameters;

            if (parameters.Any(p => p.name == parameterName))
            {
                return;
            }

            VRCExpressionParameters.Parameter[]
                newParameters = new VRCExpressionParameters.Parameter[parameters.Length + 1];
            for (int i = 0; i < parameters.Length; i++)
            {
                newParameters[i] = parameters[i];
            }

            VRCExpressionParameters.Parameter newParameter1 = new VRCExpressionParameters.Parameter
            {
                name = parameterName,
                valueType = animMode == AnimationHelpers.AnimMode.Toggle
                    ? VRCExpressionParameters.ValueType.Bool
                    : VRCExpressionParameters.ValueType.Float,
                defaultValue = 0f,
                saved = true
            };
            newParameters[newParameters.Length - 1] = newParameter1;
            expressionParameters.parameters = newParameters;
            EditorUtility.SetDirty(expressionParameters);
            
            Debug.Log("Added parameter: " + parameterName);
        }

        public static void AddMenuOption(VRCAvatarDescriptor avatar, string parameterName, AnimationHelpers.AnimMode animMode)
        {
            VRCExpressionsMenu expressionsMenu = avatar.expressionsMenu;
            if (expressionsMenu == null)
            {
                expressionsMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                avatar.expressionsMenu = expressionsMenu;
            }

            //Find comfort submenu
            VRCExpressionsMenu.Control submenu = expressionsMenu.controls.FirstOrDefault(s => s.name == "Comfort" && s.subMenu != null);
            if (submenu == null && expressionsMenu.controls.Count < VRCExpressionsMenu.MAX_CONTROLS)
            {
                submenu = new VRCExpressionsMenu.Control
                {
                    name = "Comfort",
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    subMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>()
                };
                VRCExpressionsMenu.Control[] newControls =
                    new VRCExpressionsMenu.Control[expressionsMenu.controls.Count + 1];
                for (int i = 0; i < expressionsMenu.controls.Count; i++)
                {
                    newControls[i] = expressionsMenu.controls[i];
                }

                newControls[newControls.Length - 1] = submenu;
                expressionsMenu.controls = new List<VRCExpressionsMenu.Control>(newControls);
            }
            else if (submenu == null)
            {
                Debug.LogError("Too many controls in expressions menu");
                return;
            }

            if (submenu.subMenu.controls.Count < VRCExpressionsMenu.MAX_CONTROLS && submenu.subMenu.controls.All(c => c.name != parameterName))
            {
                submenu.subMenu.controls.Add(new VRCExpressionsMenu.Control
                {
                    name = parameterName,
                    type = animMode == AnimationHelpers.AnimMode.Toggle ? VRCExpressionsMenu.Control.ControlType.Toggle : VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                    parameter = new VRCExpressionsMenu.Control.Parameter
                    {
                        name = parameterName,
                    }
                });
            }
            string avatarPath = AssetDatabase.GetAssetPath(expressionsMenu);
            avatarPath = avatarPath.Substring(0, avatarPath.LastIndexOf("/", StringComparison.Ordinal));
            if (AssetDatabase.LoadAssetAtPath(avatarPath + "/" + submenu.name + ".asset", typeof(VRCExpressionsMenu)))
            {
                AssetDatabase.SaveAssets();
            }
            else
            {
                AssetDatabase.CreateAsset(submenu.subMenu, $"{avatarPath}/Comfort.asset");
            }
            AssetDatabase.SaveAssets();
        }
    }
}