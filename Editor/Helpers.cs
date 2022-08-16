using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace Comfort.Editor
{
    public static class Helpers
    {
        public static VRCAvatarDescriptor GetSelectedAvatar()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
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

        public static void CreateGrabPassSphere(VRCAvatarDescriptor avatar, string parameterName, string materialPath)
        {
            GameObject root = avatar.transform.Find(parameterName).gameObject;
            Transform grabPassSphere = root.transform.Find("GrabPassSphere");
            if (grabPassSphere == null)
            {
                grabPassSphere = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                grabPassSphere.parent = root.transform;
                grabPassSphere.localPosition = Vector3.zero;
                grabPassSphere.localRotation = Quaternion.identity;
                grabPassSphere.localScale = Vector3.one;
                grabPassSphere.name = "GrabPassSphere";
            }
            
            Collider collider = grabPassSphere.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            MeshRenderer renderer = grabPassSphere.GetComponent<MeshRenderer>();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            material.name = parameterName + "Material";
            renderer.sharedMaterial = material;
            
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.allowOcclusionWhenDynamic = false;
            renderer.receiveShadows = false;

        }
        
        public static void SetUpConstraint(VRCAvatarDescriptor avatar, string parameterName)
        {
            Transform target = avatar.transform.Find(parameterName);
            if (target == null)
            {
                target = new GameObject(parameterName).transform;
                target.parent = avatar.transform;
            }

            Transform headTransform = avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);

            target.transform.position = headTransform.position;
            target.gameObject.SetActive(false);

            ParentConstraint parentConstraint = target.gameObject.GetComponent<ParentConstraint>();
            if (parentConstraint == null)
            {
                parentConstraint = target.gameObject.AddComponent<ParentConstraint>();
            }
            ConstraintSource source = new ConstraintSource
            {
                sourceTransform = headTransform,
                weight = 1
            };
            List<ConstraintSource> sources = new List<ConstraintSource> {};
            parentConstraint.GetSources(sources);
            if (!sources.Contains(source))
            {
                parentConstraint.AddSource(source);
            }
            parentConstraint.weight = 1;
            parentConstraint.constraintActive = true;
        }
        public static void SetUpCameraOverlay(VRCAvatarDescriptor avatar, string parameterName)
        {
            if (avatar == null)
            {
                return;
            }

            Transform root = avatar.transform.Find(parameterName);
            Transform camera = root.Find("Camera");
            Transform overlayTransform = camera.Find("Overlay");
            GameObject overlay;
            if (overlayTransform == null)
            {
                overlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
                overlay.transform.parent = camera;
                overlay.name = "Overlay";
                overlay.transform.localPosition = new Vector3(0, 0, 10000);
                overlay.transform.localScale = new Vector3(100, 100, 100);
                Collider collider = overlay.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }
            else
            {
                overlay = overlayTransform.gameObject;
            }

            overlay.layer = LayerMask.NameToLayer("PlayerLocal");
            MeshRenderer renderer = overlay.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                renderer.allowOcclusionWhenDynamic = false;
                renderer.material =
                    AssetDatabase.LoadAssetAtPath<Material>(
                        "Assets/Comfort/Post Processing Blocker/Materials/RT Overlay.mat");
            }
        }
        public static void SetUpCamera(VRCAvatarDescriptor avatar, string parameterName)
        {
            if (avatar == null)
            {
                return;
            }

            GameObject root = avatar.transform.Find(parameterName).gameObject;
            Camera camera = root.GetComponentInChildren<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Camera")
                {
                    transform =
                    {
                        parent = root.transform,
                        localPosition = Vector3.zero,
                        localRotation = Quaternion.identity
                    },
                };
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = false;
            camera.nearClipPlane = 9000f;
            camera.farClipPlane = 11000f;
            camera.depth = 100f;
            camera.cullingMask = 1 << LayerMask.NameToLayer("PlayerLocal");
        }
        public static void AddCollider(VRCAvatarDescriptor avatar, string parameterName)
        {
            Debug.Log("Adding Collider");
            GameObject root = avatar.transform.Find(parameterName).gameObject;
            Transform collider = root.transform.Find("collider");
            if (collider == null)
            {
                collider = new GameObject("collider").transform;
                collider.parent = root.transform;
                collider.localPosition = Vector3.zero;
                collider.localRotation = Quaternion.identity;
                collider.localScale = Vector3.one;
            }

            Collider _collider = collider.GetComponent<Collider>();
            if (_collider == null)
            {
                _collider = collider.gameObject.AddComponent<BoxCollider>();
            }

            Bounds colliderBounds = _collider.bounds;
            colliderBounds.size = new Vector3(1f, 0.8f, 0f);

            collider.localScale = new Vector3(5f, 5f, 5f);
            collider.localPosition = new Vector3(0, 5, 0);
        }
        public static void SetScale(VRCAvatarDescriptor avatar, string parameterName)
        {
            GameObject root = avatar.transform.Find(parameterName).gameObject;
            root.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
        }
    }
}