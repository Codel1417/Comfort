# AudioLink Blocker

This works by drawing a black quad to the screen, then taking a grabpass of `_AudioTexture`. This sets the Audiolink texture to black effectively disabling the feature. If the world uses Udon AudioLink features this will not affect them.
