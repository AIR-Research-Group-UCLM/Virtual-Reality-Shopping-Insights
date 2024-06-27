Within src/, there are C# classes used in the VR Shopping application shown in our paper. Even though in the folder **HTML-Documentation** you can find a formal description of important methods and classes from the C# scripts documentation with XML comments, in this file we will provide a less formal description of the scripts from VRSI while others are more generic and can be adapted to different virtual reality rehabilitation applications. 

## Retail environment setup

### BackgroundMusic.cs
Plays background music clip in a loop to create a higher degree of shopping immersion.

### ObjectFollowingPlayer.cs
Class that can be attached to objects to follow the player. It can be used for the shopping cart and any other object that you need to follow the player (for example, UI menus  to have them always in user's reach).

### StoreInCart.cs
This class comprises the interaction with the shopping cart between the user and the product. It allows the configuration of the objects that can be added. Relying on the Unity's physics engine, the addition and removal of products are based on collisions and states of the Interactor-Interactable lifecycle.

### SwipeItem.cs
This class enables to swap between 3D models of a list of models in order to enable customization of the product through a swipe gesture with a hand. It serves as a way to display different configuration for a given product such as colours, sizes, layouts and so on. You need to attach this script to the original displayed object, while setting in the Unity scene all the others as not active in the desired spot.

## Data collection component

### AOILevels.cs & AOILevelsEditor.
The first class is a placeholder to be added to a shelf in the virtual environment that you want to be tracked by Areas of Interest. On the other hand, the second one provides the functinality adding, by default, the three levels as GameObjects with BoxColliders components.

### HeatmapScreenShooter.cs
This class allows to hold a list of Cameras within the Unity scene that which point of view can be configured, allowing users to take screenshots of the whole environment. However, its original design was meant to record the Heatmap in a set of PNG files from different points of view, in order to persist it outisde the Unity editor. Hence, it is recommended to set to Active the GameObject holding the generated Heatmap of your desired data.

### Manager classes.
Thse classes encapsulates the creation and writing of data into CSV files according its domain, such as teleportation, head tracking, hand tracking, product interactions or eye tracking. These serve as a junction of all data received from tracker class instances, which are attached to individual objects. For example, ProductInteractionTracker class scripts are attached to each individual object that you want to be tracked regarding the interactions performed over it by the user. Its configuration is straightforward by looking at the name of the public fields in the Unity editor, and using the drag-and-drop model to attach the GameObjects of the scene with the script designated. More information regarding their configuration can be found in the README.md of the project's root folder.

### Tracker classes
Tracker classes must be attached to individual GameObjects that you want to track through an execution, allowing the collection of data for such objects. These are compatible with each other: you can have a product tracked for product interactions as well as for eye tracking. For AOI interaction, you have the class AOILevelTracker, which sends the data to ShelvesDataManager. As well as the manager classes, the configuration of these is straightforward, mostly of them only requiring the reference to the GameObject with the corresponding manager script attached. 
 
## Data files generated during execution 

The files generated during execution are stored in the working directory. If the application is run directly on the headset, we can find it within the shared internal storage of the Meta Quest headset, in the subdirectory: "/sdcard/Android/data/com.<companyName>.XXX/". If you did not change the original values from the APK in this repository it will be: "/Android/data/com.AIR.VRSI/VRSI/"
Starting from this directory, the following file structure is generated (assuming that UserName here is "TestUser"). Please note that the generic date formatting YYYY-MM-DD-hh-mm-ss does not mean that there are the same for every file in the example structure below:
		
```text
PLAYERS_JSONs/
├── ETPlayerDataYYYY-MM-DD-hh-mm-ss
├── PositionPlayerDataYYYY-MM-DD-hh-mm-ss
├── ETPlayerDataYYYY-MM-DD-hh-mm-ss
├── PositionPlayerDataYYYY-MM-DD-hh-mm-ss
...
TestUser/
├── SESSION_YYYY-MM-DD_hh_mm_ss/
    ├── EyeTrackerData-AOIBigEnvironment.csv
    ├── EyeTrackerData-ProductsBigEnvironment.csv
    ├── HeadHandsDataBigEnvironment.csv
    ├── ProductInteractionDataBigEnvironment.csv
    ├── ProductReleasesBigEnvironment.csv
    ├── ShelvesDataBigEnvironment.csv
    ├── ShoppingCartDataBigEnvironment.csv
    ├── TeleportDataBigEnvironment.csv
    ├── TurningsBigEnvironment.csv
├── SESSION_YYYY-MM-DD_hh_mm_ss/
    ├── EyeTrackerData-AOIBigEnvironment.csv
    ├── EyeTrackerData-ProductsBigEnvironment.csv
    ├── HeadHandsDataBigEnvironment.csv
    ...
```
Along with the Table 1 from our paper, we provide a brief description of each of the CSV files:

### EyeTrackerData-AOIBigEnvironment.csv

This file contains data captured from eye-tracking within a specified area of interest (AOI). 

#### Fields:

-   **Frame**: The frame number of the recorded data since the start of the execution.
-   **Timestamp**: The exact time when the data was recorded in seconds sicne the start of the execution.
-   **Product/AOI**: The product or area of interest being observed.
-   **Section/Shelf**: Specific section or shelf where the product/AOI is located.
-   **EyeLeftPosition_x/y/z**: 3D coordinates of the left eye's position.
-   **EyeLeftRotation_x/y/z**: Rotation data of the left eye in 3D space.
-   **EyeRightPosition_x/y/z**: 3D coordinates of the right eye's position.
-   **EyeRightRotation_x/y/z**: Rotation data of the right eye in 3D space.
-   **HMD_x/y/z**: 3D coordinates of the head-mounted display's (HMD) position.
-   **RCHit_x/y/z**: 3D coordinates of the raycast hit position.

### EyeTrackerData-ProductsBigEnvironment.csv

This file contains eye-tracking data related to products being watched (detected by the raycast hits geenrated from the GameObjects representing the eyes). If enabled, eye tracking supported by Meta Quest Pro allows a precise tracking of the position and rotation of the eyes.

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **Product/AOI**: The product or area of interest being observed.
-   **Section/Shelf**: Specific section or shelf where the product/AOI is located.
-   **EyeLeftPosition_x/y/z**: 3D coordinates of the left eye's position.
-   **EyeLeftRotation_x/y/z**: Rotation data of the left eye in 3D space.
-   **EyeRightPosition_x/y/z**: 3D coordinates of the right eye's position.
-   **EyeRightRotation_x/y/z**: Rotation data of the right eye in 3D space.
-   **HMD_x/y/z**: 3D coordinates of the head-mounted display's (HMD) position.
-   **RCHit_x/y/z**: 3D coordinates of the raycast hit position.

### HeadHandsDataBigEnvironment.csv

This file records the position and rotation data of the head and hands within the virtual environment.

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **HMD_x/y/z**: 3D coordinates of the head-mounted display's (HMD) position.
-   **HMD_rot_x/y/z**: Rotation data of the HMD in 3D space.
-   **HandR_x/y/z**: 3D coordinates of the right hand's position.
-   **HandR_rot_x/y/z**: Rotation data of the right hand in 3D space.
-   **HandL_x/y/z**: 3D coordinates of the left hand's position.
-   **HandL_rot_x/y/z**: Rotation data of the left hand in 3D space.
-   **Velocity_HandR_x/y/z**: Velocity data of the right hand in 3D space.
-   **Velocity_HandL_x/y/z**: Velocity data of the left hand in 3D space.
-   **Distance**: Distance traveled.

### ProductInteractionDataBigEnvironment.csv

This file documents interactions with objects/products.

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **Object**: The object being interacted with.
-   **Section**: Specific section where the interaction took place.
-   **State**: The state of the object (e.g., grabbed, released).
-   **Position_x/y/z**: 3D coordinates of the object's position.
-   **Rotation_x/y/z**: Rotation data of the object in 3D space.
-   **Scale_x/y/z**: Scale data of the object.
-   **RightHand_State**: The state of the right hand during interaction according the Interactor-Interactable lifecycle from the Meta Interection SDK. It can be Select (the Interactor is interacting with an Interactable), Hover (the Interactor can interact with the best Interactable candidate), Normal or Disabled.
-   **LeftHand_State**: The state of the left hand during interaction. It can be Select (the Interactor is interacting with an Interactable), Hover (the Interactor can interact with the best Interactable candidate), Normal or Disabled.

### ProductReleasesBigEnvironment.csv

This file records data about product releases for those that are grabbed in first place.

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **Object**: The object being released.
-   **DurationUntilRelease**: Duration until the object was released.
-   **RightHand_State**: The state of the right hand at the time of release.
-   **LeftHand_State**: The state of the left hand at the time of release.

### ShelvesDataBigEnvironment.csv

This file contains data related to shelves withing the virtual environment's layout and areas of interest of these.

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **Shelf**: The shelf being observed or interacted with.
-   **AOI**: Area of interest on the shelf.
-   **HandR_x/y/z**: 3D coordinates of the right hand's position.
-   **HandR_rot_x/y/z**: Rotation data of the right hand in 3D space.
-   **HandL_x/y/z**: 3D coordinates of the left hand's position.
-   **HandL_rot_x/y/z**: Rotation data of the left hand in 3D space.
-   **Velocity_HandR_x/y/z**: Velocity data of the right hand in 3D space.
-   **Velocity_HandL_x/y/z**: Velocity data of the left hand in 3D space.

### ShoppingCartDataBigEnvironment.csv

This file documents actions related to a shopping cart.

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **Item**: The item being added or removed from the cart.
-   **Action**: The action taken (e.g., add, remove).

### TeleportDataBigEnvironment.csv

This file records teleportation data.

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **TPHotspot**: The teleportation hotspot used.
-   **HMD_x/z**: 3D coordinates of the head-mounted display's (HMD) position.
-   **Duration**: Duration of the teleportation.
-   **WasTP**: Indicates if a teleportation event occurred.
-   **ZOI**: Zone of interest related to the teleportation.
-   **Distance**: Distance traveled during teleportation.
-   **RightHand_State**: The state of the right hand during teleportation.
-   **LeftHand_State**: The state of the left hand during teleportation.

### TurningsBigEnvironment.csv

This file documents turnings (rotate the point of view of the user by a certain amount of degrees) within the virtual environment.

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **RightHand_State**: The state of the right hand during turning.
-   **LeftHand_State**: The state of the left hand during turning.

### HeadHandsDataBigEnvironment_segmented.csv

It includes new fields regarding the data being segmented in Zones of Interests (ZOIs).

#### Fields:

-   **Frame**: The frame number of the recorded data.
-   **Timestamp**: The exact time when the data was recorded.
-   **HMD_x/y/z**: 3D coordinates of the head-mounted display's (HMD) position.
-   **HMD_rot_x/y/z**: Rotation data of the HMD in 3D space.
-   **HandR_x/y/z**: 3D coordinates of the right hand's position.
-   **HandR_rot_x/y/z**: Rotation data of the right hand in 3D space.
-   **HandL_x/y/z**: 3D coordinates of the left hand's position.
-   **HandL_rot_x/y/z**: Rotation data of the left hand in 3D space.
-   **Velocity_HandR_x/y/z**: Velocity data of the right hand in 3D space.
-   **Velocity_HandL_x/y/z**: Velocity data of the left hand in 3D space.
-   **Distance**: Distance traveled or distance between points.
-   **Zone**: The zone within the environment where the data was recorded (Start for the start of the applciation, Shelf, Adjacent, Near or Far).
-   **Status**: The status of the user regarding its movement in such frame (either Stop or Moving).

Aditionally, two CSV with data derived are obtained from the analysis tool:

### EyeTrackerData-AOIBigEnvironment_withFixations.csv

It contains additonal fields regarding fixation and saccades of eyes:

#### Fields:

-   **fixation**: Boolean or categorical data indicating whether a fixation occurred.
-   **fixation_start**: Timestamp indicating the start of a fixation.
-   **fixation_end**: Timestamp indicating the end of a fixation.
-   **fixation_duration**: Duration of the fixation in milliseconds or seconds.
-   **Zone**: The zone within the environment where the data was recorded.
