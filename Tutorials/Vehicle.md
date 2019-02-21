# Vehicle Mod

## What do I need to prepare

Assets:
-Vehicle Model with textures and Vehicle HitBox
-Vehicle Engine Sounds(Start,Idle,Running)
-Fire Sound(Near and Far)

And,finish the Step 1-4 in [GetStarted](GetStarted.md)

## Step.1 Process the Vehicle Model

![Vehicle-Model](VehicleModel.jpg)

The wheels,turret,gun and dym(barrel) of the vehicle model should have a correct child-parent relationship and naming.

The child-parent relationship should like this. [Click to view the example vehicle model](https://github.com/Doreamonsky/Panzer-War-Lit-Mod/blob/master/UnityProject/ArtSources/Template-Vehicle.fbx?raw=true)

|                 |           |     |
| --------------- | --------- | --- |
| MainBody        |           |     |
| LeftWheel       | l_1-l_n   |     |
| LeftUpperWheel  | l_1-l_n   |     |
| LefTrack        |           |     |
| RightWheel      | r_1-r_n   |     |
| RightUpperWheel | ru_1-ru_n |     |
| RightTrack      |           |     |
| Turret          | Gun       | Dym |

## Step.2 Make the Vehicle HitBox Model

![VehicleHitBox](VehicleHitBox.jpg)

The HitBox Model is used for penetration system. You should separate HitBox to pieces. Every piece of HitBox will have their own armor thickness after being imported to the game engine.

[Click to view the example vehicle HitBox](https://github.com/Doreamonsky/Panzer-War-Lit-Mod/blob/master/UnityProject/ArtSources/Template-Vehicle_HitBox.fbx?raw=true)

## Step.3 Import Vehicle Model to the Unity Engine

![ImportToEngine](ImportToEngine.jpg)

Create a folder in the project window in the Unity Engine,and drag your models into that folder.

Set up the materials.

And then drag the vehicle model from the project window to the Hierachy window to set up the collision.

![Collision](Collision.jpg)

Add box colldiers to the MainBody and Turret. Modify the size of box collider to suit the the MainBody and Turret.

Then,create a subfolder in your folder and name it Collision.Drag your model from the Hierachy Windows into that folder.

You can refer the Template-Vehicle folder.
