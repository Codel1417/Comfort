using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Comfort.Editor
{
    public static class AnimationHelpers
    {
        public static void AddAnimatorStates(VRCAvatarDescriptor avatar, string parameterName, AnimationClip emable,
            AnimationClip disable, bool writeDefaults, AnimMode animMode)
        {
            Debug.Log("Adding animator states for " + parameterName);
            VRCAvatarDescriptor.CustomAnimLayer
                avatarDescriptorBaseAnimationLayer = avatar.baseAnimationLayers[4];
            AnimatorController animatorController =
                (AnimatorController)avatarDescriptorBaseAnimationLayer.animatorController;
            //Add checks for existing states
            if (animatorController.parameters.All(x => x.name != parameterName))
            {
                animatorController.AddParameter(parameterName, AnimMode.Toggle == animMode ? AnimatorControllerParameterType.Bool : AnimatorControllerParameterType.Float);
            }
            AnimatorControllerLayer layer = animatorController.layers.FirstOrDefault(lay => lay.name == parameterName);
            if (layer == null)
            {
                layer = new AnimatorControllerLayer
                {
                    defaultWeight = 1,
                    blendingMode = AnimatorLayerBlendingMode.Override,
                    avatarMask = null,
                    name = parameterName,
                    stateMachine = new AnimatorStateMachine
                    {
                        name = parameterName
                    }
                };
                animatorController.AddLayer(layer);
            }

            // add the animator state
            AnimatorStateMachine stateMachine = layer.stateMachine;
            if (stateMachine == null)
            {
                stateMachine = new AnimatorStateMachine
                {
                    name = parameterName
                };
                layer.stateMachine = stateMachine;
            }
            
            if (animMode == AnimMode.Toggle)
            {
                AnimatorState notBlocked = stateMachine.AddState(parameterName + " Not Blocked");
                AnimatorState blocked = stateMachine.AddState(parameterName + " Blocked");
                blocked.writeDefaultValues = writeDefaults;
                notBlocked.writeDefaultValues = writeDefaults;
                notBlocked.motion = disable;
                blocked.motion = emable;
                // add the transition

                AnimatorStateTransition animatorStateTransition = blocked.AddTransition(notBlocked);
                animatorStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0f, parameterName);
                animatorStateTransition.hasExitTime = false;
                animatorStateTransition.exitTime = 0f;
                animatorStateTransition.duration = 0f;
                animatorStateTransition.offset = 0f;

                AnimatorStateTransition animatorStateTransition2 = notBlocked.AddTransition(blocked);
                animatorStateTransition2.AddCondition(AnimatorConditionMode.If, 1f, parameterName);
                animatorStateTransition2.AddCondition(AnimatorConditionMode.If, 1f, "IsLocal");
                animatorStateTransition2.hasExitTime = false;
                animatorStateTransition2.exitTime = 0f;
                animatorStateTransition2.duration = 0f;
                animatorStateTransition2.offset = 0f;
            }

            if (animMode == AnimMode.Blend)
            {
                BlendTree blendTree = new BlendTree
                {
                    name = parameterName,
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = parameterName
                };
                blendTree.AddChild(disable, 0f);
                blendTree.AddChild(emable, 1f);
                AnimatorState animatorState = stateMachine.AddState(parameterName);
                animatorState.motion = blendTree;
            }

            EditorUtility.SetDirty(animatorController);
            Debug.Log("Added animator states for " + parameterName);
        }
        public static bool IsWriteDefaults(VRCAvatarDescriptor avatar)
        {
            // check animator to get if write defaults is enabled
            foreach (VRCAvatarDescriptor.CustomAnimLayer layer in avatar.baseAnimationLayers)
            {
                if (layer.animatorController != null)
                {
                    AnimatorController animator = (AnimatorController)layer.animatorController;

                    foreach (AnimatorControllerLayer layer2 in animator.layers)
                    {
                        if (layer2.stateMachine != null && layer2.stateMachine.defaultState != null && layer2.stateMachine.defaultState.writeDefaultValues)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public enum AnimMode
        {
            Toggle,
            Blend
        }
    }
}