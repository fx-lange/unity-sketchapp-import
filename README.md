# Unity-SketchApp-Import

**work in progress**

The SketchApp Import is a minimalistic plugin for Unity, bringing UI components and UI prototypes designed in Sketch into your Unity projects. 

The plugin is currently based and dependent on the export files provided by Framer Generator, loading PNG assets as sprite images and position them correctly based to your design.

As the Framer Generator also supports exporting from Photshop and Flinto the plugin can also be used to import Photoshop and Flinto designs into Unity

## Usage

### Export your design via Framer Generator

If you never used the Sketch Framer workflow checkout some of guides on Medium like [Sketch Framer Bridge](https://blog.prototypr.io/build-the-bridge-between-sketch-and-framer-a3babf2cfa0f) and [Framer Sketch Workflow](https://medium.com/facebook-design/framer-sketch-an-intentional-workflow-f91ee2ee1cc1), especially on how to best prepare your design in Sketch for the export workflow. Most of it will also be true for later importing to Unity. 

From the export folder we are only interested in the `imported` subfolder, where you can find the `layers.json` and the `images` folder with all the PNG assets.

### Import to Unity

* Inside Unity make sure that `Editor Settings/Default Behavior Mode` is set to **2D**, so the assets will be imported as sprites.
* Open the plugin via `Window/Sketch Import
* Select the `layers.json` from your *Sketch Export* (inside the `imported` subfolder as mentioned above).
* Select an UI component (like Canvas,Panel,...) inside your scene as the *Root* for your imported UI layers.
* Select the Image Prefab from the Import Package Prefabs folder.


## Dependencies

* [Framer Generator](https://github.com/koenbok/Framer#set-up-framer-library) - for exporting from Sketch (osx only)
* [Json.Net.Unity3D](https://github.com/SaladLab/Json.Net.Unity3D/releases) 9.0.1 - for parsing the json file provided by Framer Generator
