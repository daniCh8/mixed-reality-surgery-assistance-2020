# Changing Patient through Unity

- [Changing Patient through Unity](#changing-patient-through-unity)
    - [Requirements](#requirements)
        - [Unity Hub](#unity-hub)
        - [Unity v2019.4.26f1](#unity-v2019426f1)
        - [Visual Studio](#visual-studio)
        - [Project Download](#project-download)
    - [Getting Started](#getting-started)
        - [Unity Account](#unity-account)
        - [Unity Hub License](#unity-hub-license)
        - [Unity Location](#unity-location)
        - [Project Location](#project-location)
    - [Change Patient](#change-patient)
        - [Open the Project](#open-the-project)
        - [Project Setup](#project-setup)
        - [New Patient Import](#new-patient-import)

## Requirements

### Unity Hub
[Download here](https://unity3d.com/get-unity/download). The Unity Hub is a management tool that you can use to manage all of your Unity Projects and installations.

### Unity v2019.4.26f1
[Download here](https://unity3d.com/unity/qa/lts-releases?version=2019.4). On the linked website, expand LTS Release 2019.4.26f1, and click on Download (Win) or Download (Mac) based on the OS you use. Use the downloaded file to install Unity, and remember the installation location because we will need it later.

### Visual Studio
[Download the Community version here](https://visualstudio.microsoft.com/). Visual Studio is a full-featured IDE to code, debug, test, and deploy to any platform. We will need it to build the application with the new patients loaded.

### Project Download
[Download the project folder here](https://github.com/daniCh8/mixed-reality-surgery-assistance-2020). Press on *Code*, select *Download ZIP*, and unzip the downloaded archive in your desired location. Keep in mind that the project's size is roughly 800 MB.

![](./pictures/change-patient-guide/000.png)

## Getting Started

### Unity Account

First, we will need to set up a Unity Account to use Unity and Unity Hub. Open Unity Hub and press the account button on the right. 

![](./pictures/change-patient-guide/001.png)

Press on Sign in in the menu. 

![](./pictures/change-patient-guide/002.png)

From here, create a new account if you don't have one, or sign in to your account.

### Unity Hub License

Now that we are logged in Unity Hub, we need to create a license to use Unity. Open Unity Hub and press the account button on the right. Click on Manage license from the menu.

![](./pictures/change-patient-guide/003.png)

You can create a new license by pressing on *Activate new license* and select *Unity Personal* as a License Agreement.

### Unity Location

We need to wire the Unity Version we installed with Unity Hub. To do so, open Unity Hub and go to Installs through the menu on the left.

![](./pictures/change-patient-guide/004.png)

Press on the *Locate* button and select the Unity installation folder you used in the [Unity Installation](#unity-v2019426f1).

### Project Location

We need to locate the project folder in Unity Hub. To do so, open Unity Hub and go to Projects through the menu on the left.

![](./pictures/change-patient-guide/005.png)

From here, press the *Add* button and select the project folder you used in the [Project Download](#project-download).

## Change Patient

### Open the Project

In Unity Hub, open the project by pressing it in the project list. Before opening it, remember to select 2019.4.26f1 as Unity Version and Universal Windows Platform as Target Platform. Keep in mind that opening the project might take some time, especially the first time.

![](./pictures/change-patient-guide/006.png)

### Project Setup

If it is the first time you open the project, you will need to select the right scene to work on. To do so, you have to:

1. Navigate to Assets -> MyDemo in the project folders on the bottom of the screen.
2. Drag and drop SampleScene.unity in the hierarchy.
3. Remove the default scene.

You can see this process in the GIF below.

![](./pictures/change-patient-guide/007.gif)

### New Patient Import

First, you need to import the patient data into the Unity project. The best way to do this is the following:

1. Create a new patient folder with all the necessary data on your file explorer. Naming and hierarchy of files in the new patient are not relevant.
2. Drag and drop the new patient folder into the patients' folder of the project.

This way, we will have the patient data correctly imported into the Unity project.

This process is shown in the GIF below.

![](./pictures/change-patient-guide/008.gif)

### New Patient Setup

#### `PatientsController` Attributes

To set up the new patient, we will first need to change the attributes of the PatientsController. The app supports up to two patients right now. For this guide, let's use the PatientOne features to store the new patient. We have to perform the following steps: 

1. Select the `PatientsController` in the hierarchy.
2. Put the new variables in the appropriate fields of the inspector. We will need to update the following attributes:
    - `Scans One`, where we will put the new patient's scan.
    - `Lat Screw One`, where we will put the new patient's lat screw positions (or None, if there are no lat screws for this patient).
    - `Med Screw One`, where we will put the new patient's med screw positions (or None, if there are no med screws for this patient).
    - `Dist Screw One`, where we will put the new patient's dist screw positions (or None, if there are no dist screws for this patient).

Below is a GIF showing this process.

![](./pictures/change-patient-guide/009.gif)

#### `BoneManipulation` Attributes

Now, we will need to update the patient bones.