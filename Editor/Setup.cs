using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Comfort.Editor
{
    [Serializable]
    public class Setup : EditorWindow
    {
        // Start is called before the first frame update
        [SerializeField] VisualTreeAsset visualTree;

        //Consts
        [SerializeField] private VRCAvatarDescriptor _avatar;
        [SerializeField] private bool postProcessingBlocker = true;
        [SerializeField] private bool audiolink = true;
        [SerializeField] private bool flareBlocker = true;


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
            visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Comfort/Editor/Layout.uxml");
            VisualElement root = visualTree.CloneTree();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Comfort/Editor/style.uss"));
            rootVisualElement.Add(root);
            root.Q<Toggle>("postToggle").value = postProcessingBlocker;
            root.Q<Toggle>("audioLinkToggle").value = audiolink;
            root.Q<Toggle>("flareToggle").value = flareBlocker;
            _avatar = Helpers.GetSelectedAvatar(); // get selected avatar
            root.Q<ObjectField>("avatarField").value = _avatar;
            root.Q<ObjectField>("avatarField").objectType = typeof(VRCAvatarDescriptor);
            root.Q<Button>("setupButton").clickable.clicked += SetupButtonClickEvent;
            root.Bind(new SerializedObject(this));

        }

        private void SetupButtonClickEvent()
        {
            int parameterCount = 0;
            if (audiolink)
            {
                parameterCount += 1;
            }

            if (postProcessingBlocker)
            {
                parameterCount += 1;
            }

            if (flareBlocker)
            {
                parameterCount += 1;
            }

            if (_avatar != null && parameterCount > 0 && EditorUtility.DisplayDialog("Setup Comfort System",
                    "Setup Comfort System? this will cost " + parameterCount + " avatar paramaters", "OK", "Cancel"))
            {
                bool writeDefaults = AnimationHelpers.IsWriteDefaults(_avatar);
                if (postProcessingBlocker)
                {
                    string parameterName = "Post Processing Blocker";
                    AnimationClip enabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                        "Assets/Comfort/Post Processing Blocker/Animations/Enable Post Processing Blocker.anim");
                    AnimationClip disabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                        "Assets/Comfort/Post Processing Blocker/Animations/Disable Post Processing Blocker.anim");
                    SetupBase(_avatar, parameterName, AnimationHelpers.AnimMode.Toggle, writeDefaults, enabledAnimation, disabledAnimation);
                    Helpers.CreateGrabPassSphere(_avatar, parameterName, "Assets/Comfort/Post Processing Blocker/Materials/ScreenCapture.mat");
                    Helpers.SetUpCamera(_avatar, parameterName);
                    Helpers.SetUpCameraOverlay(_avatar, parameterName);
                    Helpers.SetScale(_avatar, parameterName);
                }

                if (audiolink)
                {
                    string parameterName = "AudioLink Blocker";
                    AnimationClip enabledAnimation =
                        AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Audiolink Blocker/Animations/Enable AudioLinkBlocker.anim");
                    AnimationClip disabledAnimation =
                        AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Audiolink Blocker/Animations/Disable AudioLinkBlocker.anim");
                    SetupBase(_avatar, parameterName, AnimationHelpers.AnimMode.Toggle, writeDefaults, enabledAnimation, disabledAnimation);
                    Helpers.CreateGrabPassSphere(_avatar, parameterName, "Assets/Comfort/Audiolink Blocker/Material/AudioLinkiJammer.mat");
                }

                if (flareBlocker)
                {
                    AnimationClip enabledAnimation =
                        AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Flare/Animations/Enable Flare Collider.anim");
                    AnimationClip disabledAnimation =
                        AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Flare/Animations/Disable Flare Collider.anim");
                    string parameterName = "Flare Blocker";
                    SetupBase(_avatar, parameterName, AnimationHelpers.AnimMode.Toggle, writeDefaults, enabledAnimation, disabledAnimation);
                    Helpers.AddCollider(_avatar, parameterName);
                }
            }
        }

        public static void SetupBase(VRCAvatarDescriptor avatarDescriptor, string parameterName,
            AnimationHelpers.AnimMode animMode, bool writeDefaults, AnimationClip enabledAnimation, AnimationClip disabledAnimation)
        {
            Undo.RecordObject(avatarDescriptor, "Setup " + parameterName);
            Helpers.SetUpConstraint(avatarDescriptor, parameterName);
            VRCHelpers.AddParameter(avatarDescriptor, parameterName, animMode);
            AnimationHelpers.AddAnimatorStates(avatarDescriptor, parameterName, enabledAnimation, disabledAnimation,
                writeDefaults, animMode);
            VRCHelpers.AddMenuOption(avatarDescriptor,parameterName,animMode);
        }
        
        
        private void OnAvatarChanged(ChangeEvent<Object> evt)
        {
            VisualElement help = rootVisualElement.Q<VisualElement>("help");
            if (evt.newValue as VRCAvatarDescriptor != null)
            {
                _avatar = (evt.newValue as VRCAvatarDescriptor);
                help.style.display = DisplayStyle.None;
            }
            else
            {
                _avatar = evt.previousValue as VRCAvatarDescriptor;
                help.style.display = DisplayStyle.Flex;
            }

            Repaint();
        }
    }
}