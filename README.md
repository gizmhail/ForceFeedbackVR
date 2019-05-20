# ForceFeedbackVR
Tests to use Unity physics to interact in VR
Based on Oculus SDK (but adaptable to any one)

## Principle for a physics/VR object
1. create a copy of the grabbable object, that is NOT kinematic, and hidden by default
2. have those 2 versions in separate collision mask layers
3. without collision, have the  the copy follow the original, grabbed, object
3. on a collision
- make the grabbed object translucent (ghosty aspect)
- display the copy
- add forces to it so that the original grabbed point attracts the same point on the copy
- add vibrations based on collision hit point distance between the original and the copy

## Install
- clone
- open with Unity 2018.3.x
- import Oculus integration asset: https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022

## Demonstration video
- first try: https://www.youtube.com/watch?v=PfATr7v0IbE