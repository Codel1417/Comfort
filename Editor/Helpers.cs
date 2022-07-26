using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Comfort.Editor
{
    public static class Helpers
    {
        public static VRCAvatarDescriptor GetSelectedAvatar()
        {
            GameObject selected = Selection.activeGameObject;
            if ( selected != null)
            {
               return selected.GetComponent<VRCAvatarDescriptor>();
            }
            // else check hierarchy for a valid avatar
            VRCAvatarDescriptor avatar = Object.FindObjectOfType<VRCAvatarDescriptor>();
            if (avatar != null)
            {
                return avatar;
            }
            return null;
        }

        public static void AddParameter(GameObject avatar, string parameterName)
        {
            Debug.Log("Adding parameter: " + parameterName);
            // get the avatar
            VRCAvatarDescriptor avatarDescriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            VRCExpressionParameters expressionParameters = avatarDescriptor.expressionParameters;
            if (expressionParameters == null)
            {
                expressionParameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                avatarDescriptor.expressionParameters = expressionParameters;
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
                valueType = VRCExpressionParameters.ValueType.Bool,
                defaultValue = 0f,
                saved = true
            };
            newParameters[newParameters.Length - 1] = newParameter1;
            expressionParameters.parameters = newParameters;
            
            Debug.Log("Added parameter: " + parameterName);
        }

        public static void CreateGrabPassSphere(VRCAvatarDescriptor avatar, string parameterName, string shader)
        {
            GameObject root = avatar.transform.Find(parameterName).gameObject;
            Transform grabPassSphere = root.transform.Find("GrabPassSphere");
            if (grabPassSphere == null)
            {
                grabPassSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
                grabPassSphere.parent = root.transform;
                grabPassSphere.localPosition = Vector3.zero;
                grabPassSphere.localRotation = Quaternion.identity;
                grabPassSphere.localScale = Vector3.one;
            }
            Collider collider = grabPassSphere.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
            MeshRenderer renderer = grabPassSphere.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find(shader));
        }
        public static void AddAnimatorStates(GameObject avatar, string parameterName, AnimationClip emable, AnimationClip disable, bool writeDefaults = true)
        {
            Debug.Log("Adding animator states for " + parameterName);
            VRCAvatarDescriptor avatarDescriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            VRCAvatarDescriptor.CustomAnimLayer
                avatarDescriptorBaseAnimationLayer = avatarDescriptor.baseAnimationLayers[4];
            AnimatorController animator = (AnimatorController)avatarDescriptorBaseAnimationLayer.animatorController;
            //Add checks for existing states
            if (animator.parameters.All(x => x.name != parameterName))
            {
                animator.AddParameter(parameterName, AnimatorControllerParameterType.Bool);
            }
            if (animator.layers.All(x => x.name != parameterName))
            {
                animator.AddLayer(parameterName);
            }
            
            AnimatorControllerLayer layer = animator.layers.FirstOrDefault(lay => lay.name == parameterName);
            if (layer == null)
            {
                Debug.LogError("Could not find layer " + parameterName);
                return;
            }
            layer.defaultWeight = 1;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            layer.avatarMask = null;
            // add the animator state
            AnimatorStateMachine stateMachine =  new AnimatorStateMachine(); // reset the state machine
            AnimatorState notBlocked = stateMachine.AddState("Post Processing Not Blocked");
            AnimatorState blocked = stateMachine.AddState("Post Processing Blocked");
            blocked.writeDefaultValues = writeDefaults;
            notBlocked.writeDefaultValues = writeDefaults;
            notBlocked.motion = disable;
            blocked.motion = emable;
            // add the transition

            //Default
            AnimatorStateTransition defaultTransition = stateMachine.defaultState.AddTransition(notBlocked);
            defaultTransition.hasExitTime = false;
            defaultTransition.exitTime = 0f;
            defaultTransition.duration = 0f;
            defaultTransition.offset = 0f;

            AnimatorStateTransition animatorStateTransition = blocked.AddTransition(notBlocked);
            animatorStateTransition.AddCondition(AnimatorConditionMode.If, 0f, parameterName);
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
            
            
            // add the state machine to the layer
            layer.stateMachine = stateMachine;
            Debug.Log("Added animator states for " + parameterName);
        }

        public static void SetUpConstraint(GameObject avatar, string parameterName)
        {
            Transform target = avatar.transform.Find(parameterName);
            if (target == null)
            {
                target = new GameObject(parameterName).transform;
                target.parent = avatar.transform;
            }

            GameObject head = avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).gameObject;
            target.transform.position = head.transform.position;
            target.gameObject.SetActive(false);
            ParentConstraint parentConstraint = target.gameObject.AddComponent<ParentConstraint>();
            ConstraintSource source = new ConstraintSource
            {
                sourceTransform = head.transform,
                weight = 1
            };
            parentConstraint.AddSource(source);
            parentConstraint.weight = 1;
            parentConstraint.constraintActive = true;
        }
        
        public static bool isWriteDefaults(VRCAvatarDescriptor avatar)
        {
            // check animator to get if write defaults is enabled
            foreach (VRCAvatarDescriptor.CustomAnimLayer layer in avatar.baseAnimationLayers)
            {
                AnimatorController animator = (AnimatorController) layer.animatorController;
                if (animator != null)
                {
                    foreach (AnimatorControllerLayer layer2 in animator.layers)
                    {
                        if (layer2.stateMachine.defaultState.writeDefaultValues)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}