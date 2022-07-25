# Post Processing Blocker

This blocks Post Processing by using a secondary camera. This works by having a secondary main camera with a higher depth. The shader takes a grabpass at queue 4001 and renders it on a camera that is 10000 meters away.

### Known issue

* At certain angles the mirror will display a copy of your screen.
* Not compatible with clone/doppelganger systems in worlds.