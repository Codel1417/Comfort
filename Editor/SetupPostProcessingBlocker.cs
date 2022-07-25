using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;

namespace Comfort.Editor
{
    public class SetupPostProcessingBlocker : EditorWindow
    {
        // Start is called before the first frame update

        //Consts
        private const string ParameterName = "Post Processing Blocker";
        private static GameObject _avatar;
        private static GameObject _avatarPrefab;
        private static AnimationClip _enabledAnimation;
        private static AnimationClip _disabledAnimation;
        private static bool writeDefaults = true;

        // make window
        [MenuItem("Tools/Setup " + ParameterName)]
        private static void Init()
        {
            SetupPostProcessingBlocker window =
                (SetupPostProcessingBlocker)EditorWindow.GetWindow(typeof(SetupPostProcessingBlocker));
            window.titleContent = new GUIContent("Setup " + ParameterName);
            _avatarPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Comfort/Post Processing Blocker/Post Processing Blocker.prefab");
            _enabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/Comfort/Post Processing Blocker/Animations/Enable Post Processing Blocker.anim");
            _disabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/Comfort/Post Processing Blocker/Animations/Enable Post Processing Blocker.anim");
            window.autoRepaintOnSceneChange = true;
            window.minSize = new Vector2(300, 300);
            window.position = new Rect(Screen.width / 2f - window.minSize.x / 2f,
                Screen.height / 2f - window.minSize.y / 2, window.minSize.x, window.minSize.y);
            window.Show();
        }

        public void CreateGUI()
        {
            rootVisualElement.Add(new Label("Please select the avatar"));
            ObjectField objectField = new ObjectField
            {
                objectType = typeof(VRCAvatarDescriptor),
                allowSceneObjects = true,
                value = _avatar,
                label = "Avatar"
            };
            objectField.RegisterValueChangedCallback(OnAvatarChanged);
            rootVisualElement.Add(objectField);

            // add write defaults checkbox
            Toggle writeDefaultsToggle = new Toggle
            {
                value = writeDefaults,
                label = "Write defaults?",
                tooltip = "Write default values to the avatar's parameters"
            };
            rootVisualElement.Add(writeDefaultsToggle);


            Button button = new Button(() =>
            {
                if (Helpers.IsValidAvatar(_avatar))
                {
                    Undo.RecordObject(_avatar, "Setup " + ParameterName);
                    Helpers.SetUpConstraint(_avatar.gameObject, ParameterName);
                    Helpers.AddParameter(_avatar.gameObject, ParameterName);
                    Helpers.AddAnimatorStates(_avatar.gameObject, ParameterName, _enabledAnimation, _disabledAnimation,
                        writeDefaults);
                    Helpers.CreateGrabPassSphere(_avatar, ParameterName);
                    SetUpCamera(_avatar);
                    SetUpCameraOverlay(_avatar);
                    SetScale(_avatar);
                }
            })
            {
                text = "Setup " + ParameterName
            };
            rootVisualElement.Add(button);


            //add link to github pinned to bottom of window
            Button githubLink = new Button(() =>
            {
                Application.OpenURL("https://github.com/Comfort-Studios/Comfort-Post-Processing-Blocker");
            })
            {
                text = "Github",
                tooltip = "Opens the github page for this project",
                style =
                {
                    alignSelf = Align.Center,
                    bottom = 0
                }
            };
            rootVisualElement.Add(githubLink);


            _avatar = Helpers.GetSelectedAvatar(); // get selected avatar
            objectField.value = _avatar; // set object field value to selected avatar
        }

        private void OnAvatarChanged(ChangeEvent<Object> evt)
        {
            if (Helpers.IsValidAvatar(evt.newValue as VRCAvatarDescriptor))
            {
                _avatar = (evt.newValue as VRCAvatarDescriptor)?.gameObject;
            }
            else
            {
                _avatar = evt.previousValue as GameObject;
            }

            Repaint();
        }
        
        public void SetUpCamera(GameObject avatar)
        {
            if (avatar == null)
            {
                return;
            }

            GameObject root = avatar.transform.Find(ParameterName).gameObject;
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
                    }
                };
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = false;
            camera.nearClipPlane = 9000f;
            camera.farClipPlane = 11000f;
            camera.depth = 0f;
            camera.fieldOfView = 179f;
            camera.cullingMask = 1 << LayerMask.NameToLayer("PlayerLocal");
        }

        public void SetUpCameraOverlay(GameObject avatar)
        {
            if (avatar == null)
            {
                return;
            }

            Transform root = avatar.transform.Find(ParameterName);
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
                    DestroyImmediate(collider);
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

        public static void SetScale(GameObject avatar)
        {
            GameObject root = avatar.transform.Find(ParameterName).gameObject;
            root.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
        }
    }
}