# Fighting-Game-AI-Thesis-Project
Thesis project tackles the design and implementation of an AI-driven enemy in the context of 2D fighting games. 

## Prerequisites to open the source project
- Dowload Unity Hub.
- Download Unity Editor version: Unity 2022.3.55f1 (or higher).
- Download Microsoft Visual Studio or Visual Studio Code with the official C# extension from Microsoft.
- Run project on Windows desktop.

## Unity Editor system requirements
- Operating system: Windows 7, Windows 10 or Windows 11 (64 bit version only)
- CPU: X64 architecture with SSE2 instruction set support.
- Graphics APIs: DX10, DX11, and DX12-capable GPUs
- RAM: 8GB (or higher)

## Initial Setup
- Download this repository to your prefered location.
- Open the project folder with Unity Hub using the **Add project from disk option**.
- Open the Scene located **Asset/Scenes/SampleScene.unity**.
- Press the Play icon to run the project.
  
## Controls
| Key | Action |
|-----|-----|
| A | Punch |
| S | Kick |
| D | Block |
| Escape | Pause Toggle |

## Usage Notes - Unity Editor
To edit and inspect the AI parameters before runtime:

- Navigate to the Hierarchy panel **(Left side of the screen)** and select the GameObject **ScriptHolder**
- All components for adjusting the AI and sparring partner parameters are displayed in the Inspector panel **(Right side of the screen)**
- Pre-game training can be enabled by setting a value to [**Training Count**] (E.g 100) and set [**Run At Start**] to "TRUE" before running the project, this will have the AI run independent training 100 times.

After pressing play, user input cannot be made until the pre-game training is complete, identified by [**Training Complete**] being set to "TRUE" in the inspector.

Alternatively, the AI can be tested without pre-training. Set [**Run At Start**] to "FALSE" and run the project.

Closing the running session will write the session logs to the unity persistent data path, which on window is located: **C:\Users\Users\AppData\LocalLow\<company name>\AI_Thesis_Project**. 
The file locaton to assess the play session log data will automatically be opened.

## Folder Structure
- Assets/Scripts/ - Game code
- Assets/Scripts/MainLogicScripts - contains all the AI game code created
- Assets/Scenes/ - Game scene
- Assets/Sprites/ - Game png sprites

## Known issues and troubleshoot

The training and human exchanges are written onto the same CSV file.The EvaluationLogs does not differentiate exchanges between the player and Self-play sparring partner because both phases use the PlayerActionAttempt delegate. A workaround to to view player exchanges is to treat the exchanges logged after the Training Count amount as official player exchanges. Rows above are warm-up exchanges.

No End timer. Closing the application after a set number of exchanges writes the session data.

Feedback output is only visible from inside the Unity Editor. The inspector displays the states and matrix data. The unity console window shows all Debug.Log messages which are not visible on the standalone build. To see the prediction data and behaviour, open the project in the Unity Editor and press play instead of running the standalone build.

CSV file data is written in with decimal values less than or equal to 1. Highlight the total set of data and use the keyboard shortcut [**Ctrl + Shift + %**] to format data as percentage values and increase to 2 decimal points for accurate values.


## Third-Party Licenses & Credits
- **TextMesh Pro** - © Unity Technologies, used under the Unity Companion License.
- **Liberation Sans / Roboto fonts** (bundled with TextMesh Pro) -
  licensed under the SIL Open Font License v1.1. See OFL.txt.
- **[ShowOnly] inspector attribute** - by Nick Glenn,
  https://gist.github.com/NickGlenn/84b8b43004a642b96ce9b6fef0bbcc8d
  (editor-only utility; used with attribution).
