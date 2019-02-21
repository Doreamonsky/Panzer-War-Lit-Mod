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

**Before getting started,you should have a quick view of the basic control of the Unity Engine.**

Open the project which is mentioned in the [GetStarted](GetStarted.md).

Create a folder in the project window in the Unity Engine,and drag your models into that folder.

Set up the materials.

And then drag the vehicle model from the project window to the Hierachy window to set up the collision.

![Collision](Collision.jpg)

Add box colldiers to the MainBody and Turret. Modify the size of box collider to suit the the MainBody and Turret.

Then,create a subfolder in your folder and name it Collision.Drag your model from the Hierachy Windows into that folder.

You can refer the Template-Vehicle folder.

## Step.4 Create Vehicle Data

![OpenTool](OpenTool.jpg)

If there are no errors in the console,you should find Mod/Mod Manager in the top windows bar.(If there are errors,you should install Unity 2018.3.0 f2)

![CreateVehicleData](CreateVehicleData.jpg)

Click Open Create Vehicle.

![InputVehicleName](InputVehicleName.jpg)

And input the name of your vehicle. Then,click the button.

![VehicleData](VehicleData.jpg)

These files will be created.

In the following,I will guide you how to set them one by one.

### 1. VehicleEngineSoundData

Drag your sound files to the project window. And assign the variables one by one.

![EngineSound](EngineSound.jpg)

### 2. VehicleHitBox

![HitBoxGenerate](HitBoxGenerate.jpg)

Assign the variable External Armor Model with your HitBox model.

Click Generate Prefab Button. Then,Click Open Edit Mode.

![HitBoxEdit](HitBoxEdit.jpg)

Click Generate HitBox Model. You will notice that HitBox Model are generated in the scene. And you can set the armor thickness by clicking it.

![HitBoxChild](HitBoxChild.jpg)

![HitBoxChildResult](HitBoxChildResult.jpg)

After you set all the armor thickness. Drag them into the blue prefab like the picture.

**Finally,Click Save Button!!!!!!!**

### 3. VehicleTextData

Before that,we need to create fire sound assets and bullet data assets.

![CreateBulletData](CreateBulletData.jpg)
By right clicking the project window,you can create fire sound asset and bullet data asset.

### FireSound Asset

![FireSound](FireSound.jpg)

Set the variable of Fire Near and Fire Far with fire sound.

You can use in-built sounds if you don't prepare it.

### Bullet Data Asset

![BulletData](BulletData.jpg)

Be sure to use in-built Bullet VFX!
And for ATGM,be sure to toggle Is Missle on.

===

![VehicleTextData01](VehicleTextData01.jpg)

![VehicleTextData02](VehicleTextData02.jpg)

Set the vehicle data as you wish or according to the histroy.

### 4. WheelCollider

Drag the WheelCollider and the main model into the scene so that you can edit the wheel collider data much more easier.After you set them,right click the variable with blue rect and click apply to prefab XXXX.

![WheelCollider](WheelCollider.jpg)

### 5. VehicleData

![VehicleDataEditor](VehicleDataEditor.jpg)

Assign the variable MainModel and click Open Edit Mode.

![VehicleDataEditMode](VehicleDataEditMode.jpg)

Move and rotate the GameObject with **orange icon** to the right position and rotation. Then,press 'K' key to save the transform data of the selected GameObject to the vehicle data.

Then,Click UnLock Inspector.

### 6. VehicleTrack

Panzer_Track_Middle
