# Unity-SketchApp-Import

**work in progress**

The SketchApp Import is a minimalistic plugin for Unity, bringing UI components and UI prototypes designed in Sketch into your Unity projects. 

The plugin is currently based and dependent on the export files provided by Framer Classic, loading PNG assets as sprite images and position them according to your design.

(Framer Classic may also support exporting from Photshop and Flinto like Framer Generator did - that has not been looked into yet here. If so, then this plugin could also be used to import Photoshop and Flinto designs into Unity)

## Usage

(Import/usage instructions last updated 11/18/2019)

### Export your design via Framer Classic's save function

If you never used the Sketch Framer workflow checkout some of guides on Medium like [Sketch Framer Bridge](https://blog.prototypr.io/build-the-bridge-between-sketch-and-framer-a3babf2cfa0f) and [Framer Sketch Workflow](https://medium.com/facebook-design/framer-sketch-an-intentional-workflow-f91ee2ee1cc1), especially on how to best prepare your design in Sketch for the export workflow. Most of it will also be true for later importing to Unity. 

Open Framer Classic, navigate to the Code tab, and click Import in the bottom left (third option up from the bottom). Select the Sketch file from here that you saved in Sketch earlier - this is the most formal way to import from Sketch to Framer. Now choose File > Save to create the Framer file. Viewing that framer file's contents (in Mac by right click, Show Contents) will show a folder structure that includes the `imported` subfolder.

From that exported framer file's contents (or folder), we are only interested in the `imported` subfolder, where you can find the `layers.json` and the `images` folder with all the PNG assets. You can put this folder in your Unity project if you would like - we will need to access that json file in a moment.

### Import to Unity

* Inside Unity make sure that `Editor Settings/Default Behavior Mode` is set to **2D**, so the assets will be imported as sprites.
* Open the plugin via `Window/Sketch Import
* Select the `layers.json` from your *Sketch Export* (inside the `imported` subfolder as mentioned above).
* Select an UI component (like Canvas,Panel,...) inside your scene as the *Root* for your imported UI layers.
** Note: Selecting Canvas allows you to use "Scale With Screen Size" as the UI scale mode, where the reference resolution can be adjusted so the Sketch file resolution matches the Unity resolution. 


## Dependencies

* [Framer Generator](https://github.com/koenbok/Framer#set-up-framer-library) - for exporting from Sketch (osx only)
* [Json.Net.Unity3D](https://github.com/SaladLab/Json.Net.Unity3D/releases) 9.0.1 - for parsing the json file provided by Framer Generator
