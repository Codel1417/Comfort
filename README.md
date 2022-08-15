# VRC Comfort   WIP

VRChat lacks accessibility and comfort options. I aim to fix this without resorting to Modifying the client. Why? Because I suffer from vision sensitivity and VRChat currently does not offer any accessibility options that I can benefit from.

### Why is this needed?

There are too many VRChat worlds that do not offer options to disable/control Post Processing or AudioLink, Or the world added controls but they are insufficient. To some people these are minor things or can affect the "experience" of the world. Attempts to convince the developers of VRChat to add a post processing toggle have been met with pushback. The devs believe an additive solution would be better instead of overriding the world settings but I have found this does not help me.

The issue this solves is something I suffer with every day. I suffer from vision issues but not color blindness. Visual overestimation reactions similar to seizures occur but instead it triggers a fight or flight response. Reducing the difference between bright and dark & making the darkest parts light level closer to the brightest parts is the best solution I have found. Worlds that try to have realistic lighting increase the chance of a reaction. **This solution isn't perfect but sadly Contrast, Saturation, and Gamma sliders do not help.** Worlds will not look 'correct' in regards to what the author intended but accessibility should **ALWAYS** take priority over the original experience. It is difficult to understand that people view the world differently and a comfortable color scheme for one person is painful for another. I have accepted that worlds may have washed out colors but in a way that is the goal of this.

#### Relevant Canny Pages

* https://feedback.vrchat.com/feature-requests/p/toggle-world-post-processing-locally
* https://feedback.vrchat.com/feature-requests/p/toggle-to-disable-bloom-and-other-post-process-effects
* https://feedback.vrchat.com/feature-requests/p/option-to-disable-postprocessing-clientside
* https://feedback.vrchat.com/feature-requests/p/disable-post-processing-stack
* Might Benefit? https://feedback.vrchat.com/feature-requests/p/anti-epilepsy-option

## Included Tools

These tools are all local to you and do not affect other users.

### Post Processing Blocker

This disables post processing completely. The downside is Nameplates are also hidden.

### AudioLink Blocker

This disables most AudioLink features to reduce motion in the world. This only works for the GrabPass version of AudioLink.

### Sun flare blocker

This is simply a collider to block unity flare effects.

## How to use

* Import the Unity package
* Go to `Tools/Setup Comfort System`

![Comfort UI Listing the available options](Editor/_Media/ComfortMenuUI.png)

* Set your avatar
* Check which features you want to enable
* Click 'Setup'

### Also See

* [Flux Bloom Removal Shader by Rollthered](https://rollthered.booth.pm/items/3092302) | This clamps bloom wihout removing other post processing effects
* [AntiMigraine Light Limiter by Eremite](https://github.com/Erumite/Eremite_VRC/tree/master/Assets/Eremite/Shaders/AntiMigraine_Light_Limiter) | Provides multiple options to control how visuals look