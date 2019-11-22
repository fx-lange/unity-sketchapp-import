# Unity-SketchApp-Import

**work in progress**

The SketchApp Import is a minimalistic plugin for Unity, bringing UI components and UI prototypes designed in Sketch into your Unity projects. 

The plugin is currently based and dependent on the export files provided by either Framer Generator (free) or Framer Classic (trial/paid), loading PNG assets as sprite images and position them according to your design.

Framer Generator (and perhaps Framer Classic too) supports exporting from Photshop and Flinto, soo this plugin can also be used to import Photoshop and Flinto designs into Unity.

## Usage

(Import/usage instructions last updated 11/19/2019)

### Export your design via Framer Generator or via Framer Classic's save function

If you never used the Sketch Framer workflow checkout some of guides on Medium like [Sketch Framer Bridge](https://blog.prototypr.io/build-the-bridge-between-sketch-and-framer-a3babf2cfa0f) and [Framer Sketch Workflow](https://medium.com/facebook-design/framer-sketch-an-intentional-workflow-f91ee2ee1cc1), especially on how to best prepare your design in Sketch for the export workflow. Most of it will also be true for later importing to Unity. This primarily regards formatting and naming conventions for easier navigation in all platforms. 

#### Export from Framer Generator (free)
1. Open Sketch with the relevant file open
2. Open Framer Generator, and click Sketch > Import
3. Check the location of the original .sketch file to now see an identically named .framer file next to it.
4. Proceed to `Layers and Images Extraction from Framer File` section.

#### Export from Framer Classic (trial/paid)
1. Open Framer Classic, navigate to the Code tab, and click Import in the bottom left (third option up from the bottom)
2. Select the Sketch file from here that you saved in Sketch earlier - this is the most formal way to import from Sketch to Framer
3. Now choose File > Save to create the Framer file
4. Proceed to `Layers and Images Extraction from Framer File` section.

### Layers and Images Extraction from Framer File

1. View that exported framer file's contents (in Mac by right click, Show Package Contents) to show a folder structure that includes the `imported` subfolder.
2. Navigate into the `imported` subfolder, then into the `[appName@Resolution]` folder, eg. MyAppName@1x.
3. Copy the `images` folder and `layers.json` file from here into somewhere directly accessible, like the Unity Assets folder.
4. Proceed to Unity Import section.

### Import to Unity

* Inside Unity make sure that `Editor Settings/Default Behavior Mode` is set to **2D**, so the assets will be imported as sprites.
* Open the plugin via `Window/Sketch Import
* Select the `layers.json` from your *Sketch Export* (inside the `imported` subfolder as mentioned above).
* Select an UI component (like Canvas,Panel,...) inside your scene as the *Root* for your imported UI layers.
* Note: Selecting Canvas allows you to use "Scale With Screen Size" as the UI scale mode, where the reference resolution can be adjusted so the Sketch file resolution matches the Unity resolution. 


## Dependencies

* [Framer Generator](https://github.com/koenbok/Framer#set-up-framer-library) - for exporting from Sketch (osx only)
* [Framer Classic](https://classic.framer.com/) - a paid alternative to Framer Generator
* [Json.Net.Unity3D](https://github.com/SaladLab/Json.Net.Unity3D/releases) 9.0.1 - for parsing the json file provided by Framer Generator
