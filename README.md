# Unity Spring Bone
Makes bones springy and adds drag, stretch and gravity! Apply jiggle physics to anything in Unity. Inspired by Unity3d's spring bone script which can be found at:
https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project/blob/release/legacy/2.0/Assets/UnityChan/Scripts/SpringBone.cs

# Getting Started
Clone both the SpringBone and PhysicsEnvironment scripts, and apply the SpringBone script to any object. By default, the target axis of the spring is set to the forward direction, but this can be changed by removing the \[HideInInspector\] flag over Vector3 target.

If you want to add global wind to the springs, give an object the PhysicsEnvironment script and set the global wind direction. Note that if you only clone the SpringBone script, you need to change the wind calculation in SpringBone to not depend on PhysicsEnvironment.
