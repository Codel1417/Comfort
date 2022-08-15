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
        private string _parameterName = "";
        [SerializeField] private VRCAvatarDescriptor _avatar;
        private AnimationClip _enabledAnimation;
        private AnimationClip _disabledAnimation;
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
                    _parameterName = "Post Processing Blocker";
                    _enabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                        "Assets/Comfort/Post Processing Blocker/Animations/Enable Post Processing Blocker.anim");
                    _disabledAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                        "Assets/Comfort/Post Processing Blocker/Animations/Enable Post Processing Blocker.anim");
                    SetupBase(_avatar, _parameterName, AnimationHelpers.AnimMode.Toggle, writeDefaults, _enabledAnimation, _disabledAnimation);
                    Helpers.CreateGrabPassSphere(_avatar, _parameterName, "Codel1417/PrePostProcess Capture");
                    Helpers.SetUpCamera(_avatar, _parameterName);
                    Helpers.SetUpCameraOverlay(_avatar, _parameterName);
                    Helpers.SetScale(_avatar, _parameterName);
                }

                if (audiolink)
                {
                    _parameterName = "Audio Link Blocker";
                    _enabledAnimation =
                        AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Audiolink Blocker/Animations/Disable AudioLinkBlocker.anim");
                    _disabledAnimation =
                        AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Audiolink Blocker/Animations/Enable AudioLinkBlocker.anim");
                    SetupBase(_avatar, _parameterName, AnimationHelpers.AnimMode.Toggle, writeDefaults, _enabledAnimation, _disabledAnimation);
                    Helpers.CreateGrabPassSphere(_avatar, _parameterName, "Codel1417/AudioLink Blocker");
                }

                if (flareBlocker)
                {
                    _enabledAnimation =
                        AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Flare/Animations/Enable Flare Collider.anim");
                    _disabledAnimation =
                        AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/Comfort/Flare/Animations/Disable Flare Collider.anim");
                    _parameterName = "Flare Blocker";
                    SetupBase(_avatar, _parameterName, AnimationHelpers.AnimMode.Toggle, writeDefaults, _enabledAnimation, _disabledAnimation);
                    Helpers.AddCollider(_avatar, _parameterName);
                }
            }
        }

        public static void SetupBase(VRCAvatarDescriptor avatarDescriptor, String parameterName,
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