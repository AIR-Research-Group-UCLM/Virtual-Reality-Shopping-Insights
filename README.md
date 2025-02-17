﻿# Virtual Reality Shopping-Insights: a data-driven framework to assist the design and development of Virtual Reality shopping environments

## Description
Virtual Reality Shopping Insights (VRSI) is a framework that aids the development of Virtual Reality (VR) shopping applications as well as collecting data and analysing it. 
VRSI aims to help software developers and researchers by abstracting the layers needed to setup VR technology on applciations developed with Unity. 
Moreover, it provides data registration tools that monitors user activity from non invasive data sources. This data can be useful for marketing analysts to understand user behavior inside V-commerce applications, helping them to improve the setup and layout of such environments with data-driven decissions. The data collected includes kinematics data, interaction data with the environment and products, navigational data and tracking data from eyes, hands and head.

## Prerequisites 📋
1. To build the Unity package containing the example scene included in the .apk file, Unity 2022.3.15f, Oculus Integration Pack v59.0 are required. Make sure you also install the Android Build support within your Unity installation to be able to compile and build further .apk files made with Unity.
2. To run the application, a Meta Quest 2/Pro/3 headset, a data cable (or air link) to execute Meta Quest Link application inside the headset operating system and Meta Quest Developer Hub (or similar) are needed.

## Dependencies 
To build and run an Oculus Quest application in Unity, it is necessary to use the Android platform.. To set up a Unity project to support Android, you need to install the following:
- Native Development Kit (NDK).
- Java Development Kit.
- Android Build Support module.
- Android Software Development Kit (SDK).

For more information, please visit the website https://docs.unity3d.com/Manual/android-sdksetup.html. 

Also, two main dependencies are needed to develop a Unity project using the VRSI: Meta XR All-in-One SDK and Movement SDK:
- The Meta XR All-in-One SDK undles several Meta SDKs together, which includes many features that offer advanced rendering, social and community building, and provides capabilities to build immersive experiences in both virtual reality and mixed reality. The package also includes Interaction SDK, which offers a collection of components for adding hand interactions and used in VRSI. Moreover, it includes Core SDK, an essential software tool that interfaces with OpenXR and allows core functionalities, such as hand tracking. It can be downloaded directly from the Unity Asset Store (https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269657).
- The Movement SDK for Unity utilizes body tracking, face tracking, and eye tracking. In the case of RehabImmersive, it is used to calculate the degree of wrist flexion/abduction based on body tracking performed through inverse kinematics. This SDK can be downloaded from the [GitHub repository] (https://github.com/oculus-samples/Unity-Movement.git).

## Build APK
In the src\UnityPackage folder, there are two files with the .rar extension that contain the Unity package with the scenes used in the application. This package can serve as a starting point for new applications, as it contains the classes that make up the framework. To use it, it is necessary to first extract it. After the decompression, the RehabImmersivePack package can be imported into a Unity project. 
Steps to follow:
1. Create a new 3D Core project with Unity v2022.3.15f1.
2. Install the Meta XR All-in-One from the Asset Store (v59.0 or greater, although v59.0 is recommended).
3. Import the package within the project. Go to Assets -> Import Package -> Custom Package.
4. Follow the configuration recommendations at https://developer.oculus.com/documentation/unity/unity-gs-overview/ and  https://developer.oculus.com/documentation/unity/unity-conf-settings/.
5. You need to address any potential warning to correctly setup OpenXR within the Unity editor. More information about the SDK and its configuration can be found at https://developer.oculus.com/documentation/unity/move-overview/ .
5. Select Android platform to Build de application.
6. Make sure to set up the hand tracking and select the high frequency. Go to OVRCameraRig -> OVRManager -> Quest Features -> set "Hand Tracking support" to "Hands Only" and select "Hand Tracking Frequency" to MAX.
7. Set into "Player Settings" the Company and Product name. 
8. Build the application.

## Installation 🔧
The installation of the application into the Meta Quest device of your conveninece can be done in two ways:

1. Building and running the Unity project with the Meta Quest 2/Pro/3 connected to your PC.
2. The .apk file which can be transferred to the Meta Quest headset. Due to the size of the application exceeding the limit allowed by GitHub, it is divided into two .rar files. Once the contents are extracted, the VR_BoxAndBlock apk will be obtained. Different programs can be used, in our case we recommend using Meta Quest Developer Hub.

## Execution ⚙️
The application can be run directly from the headset, or it can be run from Meta Quest Developer Hub (or another application that allows running adb commands). To allow data collection and 3D heatmap visualization, you need to:
1. Naviagte to VRSI -> Data Collection -> Prefabs and drag into your Unity scene the prefab "AllDataManagers" to have all data collectors into one parent object.
2. Configure each script in the Unity editor using drag-and-drop feature or combo-box selection of Unity. Each field name corresponds to the script needed in that field. For example, if the field is named "Directory Manager" jsut drag-and-drop such named GameObject from the prefab, located in the obejct's hierarchy, into that field.
    - You should also consider that, in "HeadAndHandsTrackingManager" script, the scripts needed for Left Hand and Right Hand are those of type "Teleport Interactor". These can be found under OVRCameraRig_WIthInteractions prefab -> OVRInteraction -> LeftHand (or RightHand if configuring right hand) -> HandInteractorsLeft (or HandInteractorsRight if configuring right hand) -> LocomotionHandInteractor -> TeleportHandInteractor.
    - In the case of "ProductInteractionManager", you need to follow the same path until HandInteractorsLeft/Right, then you will find HandGrabInteractorRight/Left, which you wil need to drag-and-drop into the fields Right Hand and Left Hand
3. For correctly collect data from execution, you need to set the user name desired in the field "Username" and leave Session path field blank. If the application is intended to be used by several suers, you can use a placeholder name or build several times the application with different time. Please bear in mind that depending on where you execute the application the data will be registered in different locations:
    - If you run the applciation directly from the Unity editor with the Meta link cable connected to the PC and using the Meta Quest Link application, it will be saved in `C:\Users\<username>\AppData\LocalLow\<companyName>\<product name>`. Otherwise, if the application is run as an installed application in the headset, the csv files should be saved in  `/Android/data/com.<companyName>.XXX/files` (with com.oculus.XXX is your Package Name).
4. To visualize the 3D HeatMaps, it is recommended to do it inside the Unity Editor to freely naviagte it in 3D view. Yo need to copy and paste the JSON PositionPlayerData file of the execution of your convenience into the "Directory and File Name With extension" field in the script "HeatMap Generation in Editor", which is in the Heatmap prefab under VRSI -> Behavior Statistics -> 3DHeatmap -> Prefabs -> HeatmapWithGeneration. You need to match the size of the 3D cube to your virutal environment size. Then, fill the mentioned field as : "PLAYERS_JSONs/<filename>.json". There are also prefabs that allow to configure the position of a set of cameras to capture in PNG files different views of the 3D Heatmap.

There are two author defined layers (Raycastable and AOI). Raycastable layer should be set in products and AOI should be set in AOI colliders. 

To run the application, you can do it inside the Meta Quest headset if you installed the aplication, or through the Meta Quest Link application. The second options requires the cable to be connected to the PC and the headset and then launch the application from the Unity editor.

In the folder "Console Python Tool for initial analysis" you can find another README.md file that will guide you to the use of such tool for analysing the data recorded.

## Repository
```text
root/
├── apk/
│ ├── VRSI-apk.part1.rar
│ ├── VRSI-apk.part2.rar
│ ├── VRSI-apk.part3.rar
│ ├── VRSI-apk.part4.rar
│ └── VRSI-apk.part5.rar
├── sampleData/
│ └── com.AIR.VRSI/
│ | ├── EyeTrackerData-AOIBigEnvironment.csv
│ | ├── EyeTrackerData-ProductsBigEnvironment.csv
│ | ├── HeadHandsDataBigEnvironment.csv
│ | ├── ProductInteractionDataBigEnvironment.csv
│ | ├── ProductReleasesBigEnvironment.csv
│ | ├── ShelvesDataBigEnvironment.csv
│ | ├── ShoppingCartDataBigEnvironment.csv
│ | ├── TeleportDataBigEnvironment.csv
│ | └── TurningsBigEnvironment.csv
│ └── csv_obtained_from_analysis_tool/
│ | ├── EyeTrackerData-AOIBigEnvironment_withFixations.csv
│ | ├── EyeTrackerData-ProductsBigEnvironment_withFixations.csv
│ | └── HeadHandsDataBigEnvironment_segmented.csv
│ └── reports/
│ | ├── VR_SI_Graphics.pdf
│ | ├── VR_SI_Statistics.pdf
│ | ├── VR_SI_Statistics_Module_Combined.pdf
│ | ├── VRSI_Navigation_report.pdf
│ | └── VRSI_ProductInteraction_report.pdf
| └── PLAYERS_JSONs/
│ | ├── ETPlayerData2024-06-11-12-35-58.json
│ | └── PositionPlayerData2024-06-11-12-35-58.json
├── doc/
│ ├── HTML-Documentation/
│ | ├── ...
│ | └── pages.html (entry point)
│ ├── Images/
│ | ├── ...
│ ├── Architecture.md
│ └── DeveloperDocumentation.md
├── src/
| ├── BehaviorStatistics/
| │ ├── 3DHeatmap/
| │ | ├── Colormaps/
| │ | | ├── Editor/
| │ | | | ├── Colormaps.gradients
| │ | | | └── license.md
| │ | ├── Materials/ └──
| │ | | └── HeatmapMaterial.mat 
| │ | ├── Prefabs/
| │ | | | ├── Colormaps.gradients
| │ | | | └── HeatmapWithGeneration.prefab
| │ | ├── Scripts/
| │ | | | ├── HeatmaptextureGenerator.cs
| │ | | | ├── HeatmapGenerationInEditor.cs
| │ | | | ├── Heatmap.cs
| │ | | | └── GaussWeightComputeShader.compute
| │ | └── Shader/ 
| │ | | | └── HeatmapRayMarchingShader.shader
| ├── Data Collection/
| │ ├── Prefabs/
| | | ├── AllDataManagers.prefab
| | | ├── DIRECTORYMANAGER.prefab
| | | ├── EYETRACKERMANAGER.prefab
| | | ├── EyeTracking.prefab
| | | ├── HEADHANDSMANAGER.prefab
| | | ├── OBJECTRELEASES.prefab
| | | ├── PRODUCTINTERACTIONMANAGER.prefab
| | | ├── SHELVESDATAMANAGER.prefab
| | | ├── SHOPPINGCARTMANAGER.prefab
| | | ├── TPTRACKERMANAGER.prefab
| | | └── TURNERMANAGER.prefab
| │ ├── Scripts/
| | | ├── AOILevels.cs
| | | ├── AOILevelTracker.cs
| | | ├── DirectoryManager.cs
| | | ├── EyeInteractable.cs
| | | ├── EyeTracker.cs
| | | ├── EyeTrackerDataManager.cs
| | | ├── EyeTrackerWithAOIs.cs
| | | ├── AOILevelGTracker.cs
| | | ├── HeadAndHandsTrackingManager.cs
| | | ├── HeatMapScreenShooter.cs
| | | ├── AOILevelGTracker.cs
| | | ├── ProductInteractionManager.cs
| | | ├── ProductInteractionTracker.cs
| | | ├── ShelvesTrackerDataManager.cs
| | | ├── ShoppingCartTrackerManager.cs
| | | ├── TeleportDataManager.cs
| | | ├── TeleportTracking.cs
| | | └── TurnerTracker.cs
| ├── Retail Environment Setup/
| │ ├── Prefabs/
| │ | ├── ExamplesEnvironment/
| │ | | ├── OneHandProductWithoutPoses.prefab
| │ | | ├── Signboard.prefab
| │ | | └── TwoHandedProductWithPoses.prefab
| │ | └── UI/
| │ | | ├── DebugCanvas.prefab
| │ | | ├── PokeableButtonPanel.prefab
| │ | | └── ProductInformationCard.prefab
| │ ├── Scripts/
| │ | ├── BackgroundMusic.cs
| │ | ├── ObjectFollowingPlayer.cs
| │ | ├── StoreInCart.cs
| │ | └── SwipteItem.cs
| ├── UnityPackage/
| │ ├── VRSI-Example.part01.rar
| │ ├── VRSI-Example.part02.rar
| │ ├── VRSI-Example.part03.rar
| │ ├── VRSI-Example.part04.rar
| │ ├── VRSI-Example.part05.rar
| │ ├── VRSI-Example.part06.rar
| │ ├── VRSI-Example.part07.rar
| │ ├── VRSI-Example.part08.rar
| │ └── VRSI-Example.part09.rar
├── LICENSE.md
└── README.md
```

## Contributors
Ruben.Grande@uclm.es
JavierAlonso.Albusac@uclm.es
David.Vallejo@uclm.es
Carlos.Gonzalez@uclm.es
Santiago.Sancez@uclm.es
JoseJesus.Castro@uclm.es

## Copyright and license
Code released under the MIT License.
					  
				


