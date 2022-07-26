using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;

namespace Comfort.Editor
{
    public class Setup : EditorWindow
    {
        // Start is called before the first frame update

        //Consts
        private string ParameterName = "";
        private static VRCAvatarDescriptor _avatar;
        private static AnimationClip _enabledAnimation;
        private static AnimationClip _disabledAnimation;
        private static bool postProcessingBlocker = false;
        private static bool audiolink = false;
        private static bool flareBlocker = false;

        // make window
        [MenuItem("Tools/Setup Comfort System")]
        private static void Init()
        {
            Setup window =
                (Setup)EditorWindow.GetWindow(typeof(Setup));
            window.titleContent = new GUIContent("Setup Comfort System");
            window.autoRepaintOnSceneChange = true;
            window.minSize = new Vector2(350, 300);
            window.position = new Rect(Screen.width / 2f - window.minSize.x / 2f,
                Screen.height / 2f - window.minSize.y / 2, window.minSize.x, window.minSize.y);
            window.Show();
        }

        public void CreateGUI()
        {
            
            const float margin = 2;
            const float padding = 1;
            var avatarHelpBox = new VisualElement()
            { 
                style =
                {
                    flexDirection = FlexDirection.Row, 
                    alignItems = Align.Center, 
                    marginBottom = margin * 2,
                    marginRight = margin,
                    marginLeft = margin,
                    marginTop = margin * 2,
                    paddingTop = padding * 2,
                    paddingBottom = padding * 2,
                    paddingRight = padding,
                    paddingLeft = padding,
                    minHeight = 30,
                } 
            };
            avatarHelpBox.AddToClassList("unity-box");
            if (_avatar == null)
            {
                avatarHelpBox.Add(new Image() { image = EditorGUIUtility.FindTexture("d_console.warnicon"), scaleMode = ScaleMode.ScaleToFit });
            }
            avatarHelpBox.Add(new Label("Please select the avatar"));
            rootVisualElement.Add(avatarHelpBox);


            ObjectField objectField = new ObjectField
            {
                objectType = typeof(VRCAvatarDescriptor),
                allowSceneObjects = true,
                value = _avatar,
                label = "Avatar",
                style = { paddingBottom = 10}
            };
            objectField.RegisterValueChangedCallback(OnAvatarChanged);
            rootVisualElement.Add(objectField);
            // add write defaults checkbox

            Toggle postProcessingBlockerToggle = new Toggle
            {
                value = postProcessingBlocker,
                label = "Post Processing Blocker",
                tooltip = "Enable/Disable the Post Processing Blocker",
            };
            rootVisualElement.Add(postProcessingBlockerToggle);
            
            Toggle audiolinkToggle = new Toggle
            {
                value = audiolink,
                label = "Audio Link Blocker",
                tooltip = "Enable/Disable the Audio Link",
            };
            rootVisualElement.Add(audiolinkToggle);

            Toggle flareBlockerToggle = new Toggle
            {
                value = flareBlocker,
                label = "Flare Blocker",
                tooltip = "Enable/Disable the Flare Blocker",
                style = { paddingBottom = 10}
            };
            rootVisualElement.Add(flareBlockerToggle);

            var setupHelp = new VisualElement()
            { 
                style =
                {
                    flexDirection = FlexDirection.Row, 
                    alignItems = Align.Center, 
                    marginBottom = margin,
                    marginRight = margin,
                    marginLeft = margin,
                    marginTop = margin,
                    paddingTop = padding,
                    paddingBottom = padding,
                    paddingRight = padding,
                    paddingLeft = padding,
                    minHeight = 50
                } 
            };
            setupHelp.AddToClassList("unity-box");
            setupHelp.Add(new Label("Pressing setup will" +
                                    "\n* Create the Selected Items on the Avatar" +
                                    "\n* Set Up Parameters" +
                                    "\n* Set Up Animations"));
            rootVisualElement.Add(setupHelp);
            Button button = new Button(() =>
            {
                if (_avatar != null)
                {
                    bool writeDefaults = Helpers.isWriteDefaults(_avatar);
                    if (postProcessingBlockerToggle.value)
                    {
                        ParameterName = "Post Processing Blocker";
                        _enabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Post Processing Blocker/Animations/Enable Post Processing Blocker.anim");
                        _disabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Post Processing Blocker/Animations/Enable Post Processing Blocker.anim");
                        Undo.RecordObject(_avatar, "Setup " + ParameterName);
                        Helpers.SetUpConstraint(_avatar.gameObject, ParameterName);
                        Helpers.AddParameter(_avatar.gameObject, ParameterName);
                        Helpers.AddAnimatorStates(_avatar.gameObject, ParameterName, _enabledAnimation,
                            _disabledAnimation,
                            writeDefaults);
                        Helpers.CreateGrabPassSphere(_avatar, ParameterName, "Codel1417/PrePostProcess Capture");
                        SetUpCamera(_avatar, ParameterName);
                        SetUpCameraOverlay(_avatar, ParameterName);
                        SetScale(_avatar, ParameterName);
                    }

                    if (audiolinkToggle.value)
                    {
                        ParameterName = "Audio Link Blocker";
                        _enabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Audiolink Blocker/Animations/Disable AudioLinkBlocker.anim");
                        _disabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Audiolink Blocker/Animations/Enable AudioLinkBlocker.anim");
                        Undo.RecordObject(_avatar, "Setup " + ParameterName);
                        Helpers.SetUpConstraint(_avatar.gameObject, ParameterName);
                        Helpers.AddParameter(_avatar.gameObject, ParameterName);
                        Helpers.AddAnimatorStates(_avatar.gameObject, ParameterName, _enabledAnimation,
                            _disabledAnimation,
                            writeDefaults);
                        Helpers.CreateGrabPassSphere(_avatar, ParameterName, "Codel1417/AudioLink Blocker");
                    }

                    if (flareBlockerToggle.value)
                    {
                        _enabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Flare/Animations/Enable Flare Collider.anim");
                        _disabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Flare/Animations/Disable Flare Collider.anim");
                        ParameterName = "Flare Blocker";
                        Undo.RecordObject(_avatar, "Setup " + ParameterName);
                        Helpers.SetUpConstraint(_avatar.gameObject, ParameterName);
                        Helpers.AddParameter(_avatar.gameObject, ParameterName);
                        Helpers.AddAnimatorStates(_avatar.gameObject, ParameterName, _enabledAnimation,
                            _disabledAnimation,
                            writeDefaults);
                        AddCollider(_avatar, ParameterName);
                    }
                }
            })
            {
                text = "Setup Comfort System",
                style = { height = 30 }
            };
            rootVisualElement.Add(button);


            //add link to github pinned to bottom of window
            Button githubLink = new Button(() => { Application.OpenURL("https://github.com/Codel1417/Comfort"); })
            {
                text = "Github",
                tooltip = "Opens the github page for this project",
                style =
                {
                    alignSelf = Align.Center,
                    bottom = 0,
                    
                    
                }
            };
            rootVisualElement.Add(githubLink);


            _avatar = Helpers.GetSelectedAvatar(); // get selected avatar
            objectField.value = _avatar; // set object field value to selected avatar
        }

        private void AddCollider(VRCAvatarDescriptor avatar, string ParameterName)
        {
            Debug.Log("Adding Collider");
            GameObject root = avatar.transform.Find(ParameterName).gameObject;
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
        private void OnAvatarChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue as VRCAvatarDescriptor != null)
            {
                _avatar = (evt.newValue as VRCAvatarDescriptor);
            }
            else
            {
                _avatar = evt.previousValue as VRCAvatarDescriptor;
            }

            Repaint();
        }

        public void SetUpCamera(VRCAvatarDescriptor avatar, string ParameterName)
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

        public void SetUpCameraOverlay(VRCAvatarDescriptor avatar, string ParameterName)
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

        public static void SetScale(VRCAvatarDescriptor avatar, string ParameterName)
        {
            GameObject root = avatar.transform.Find(ParameterName).gameObject;
            root.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
        }
    }
}