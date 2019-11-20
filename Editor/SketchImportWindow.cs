using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

using System.IO;

using Newtonsoft.Json.Linq;


public class SketchImportWindow : EditorWindow
{
    
    //private members for parsing the json
    string jsonFileName = "layers.json";
    string importFolder = "";
    float importScale = 1f;

    //private members for sprites and images
    string spriteAssetSubFolder = "sprites";
    string spriteFolderPath = null;

    //parent transform 
    RectTransform rootTransform = null;
    
    //image prefab
    private Image imagePrefab = null;

    //Create a Menu Item so we can open this window
    [MenuItem("Window/Sketch Import")]
    static void OpenImportWindow()
    {
        SketchImportWindow window = EditorWindow.GetWindow<SketchImportWindow>(false, "Sketch Import");
        window.minSize = new Vector2(140, 140);
        
        window.LoadImagePrefab();
    }  
    
    void LoadImagePrefab(){
        var script = MonoScript.FromScriptableObject( this );
        
        var scriptPath = AssetDatabase.GetAssetPath( script );
        string directoryPath = Path.GetDirectoryName(scriptPath);

        string pluginDir = Path.GetDirectoryName(directoryPath);
        string prefabsDir = pluginDir + "/Prefabs";
        string relativePath = prefabsDir + "/SketchImportImage.prefab";
        imagePrefab = (Image)AssetDatabase.LoadAssetAtPath<Image>(relativePath);
    }

    void OnGUI()
    {
        //TEST BUTTON
        //OnTestButton();

        //SOURCE
        GUILayout.Label("Sketch Export (layers.json):",EditorStyles.boldLabel);
        if (GUILayout.Button(importFolder, EditorStyles.objectField))
        {
            OnSelectButton();
        }

        //SCALE
        GUILayout.Label("Scale:", EditorStyles.boldLabel);
        importScale = EditorGUILayout.FloatField(importScale, EditorStyles.objectField);
        
        //ROOT TRANSFORM
        GUILayout.Label("Root:", EditorStyles.boldLabel);
        rootTransform = EditorGUILayout.ObjectField(rootTransform, typeof(RectTransform),true) as RectTransform;
        
        //IMAGE PREFAB
        // GUILayout.Label("Image Prefab:", EditorStyles.boldLabel);
        // imagePrefab = EditorGUILayout.ObjectField(imagePrefab, typeof(Image),true) as Image;

        GUILayout.Space(10);
        //IMPORT
        GUI.enabled = IsFormValid();
        if (GUILayout.Button("Import", GUILayout.Height(25)))
        {
            OnImportButton();
            Debug.Log("Set import scale to " + importScale);
        }
        GUI.enabled = true;
    }

    private void OnSelectButton()
    {
        string jsonFile = EditorUtility.OpenFilePanel("Select Json", importFolder, "json");
        if (!jsonFile.Equals("") && jsonFile.EndsWith(".json"))
        {
            importFolder = Path.GetDirectoryName(jsonFile);
            jsonFileName = Path.GetFileName(jsonFile);
            JArray json = PreParseJson();
            
            foreach(JToken token in json){
                JObject artboard = (JObject)token;
                Debug.Log("Artboard: " + artboard["name"]);
            }
        }
        
        if(!imagePrefab){
            LoadImagePrefab();
        }
    }

    private void OnImportButton()
    {
        if (IsFormValid())
        {

            JArray json = PreParseJson();
            if(json!=null){
                
                //folder to store sprites
                spriteFolderPath = SpriteImport.CreateFolder(spriteAssetSubFolder);
                
                //create sprites
                foreach(JToken artboard in json){
                    PreCreateSprites((JObject)artboard);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //make sure that all textures are imported as sprites (even in 3d)
                SpriteImport.SetTexturesToSprites(spriteFolderPath);

                //parse json artboards
                Undo.RegisterFullObjectHierarchyUndo(rootTransform, "Import Layers");
                foreach(JToken artboard in json){
                    Parse((JObject)artboard,rootTransform,Vector2.zero);
                }

                //finish by refreshing all the assets
                AssetDatabase.Refresh();
            }

        }
    }

    private bool IsFormValid()
    {
        return rootTransform != null && !importFolder.Equals("") && imagePrefab != null;
    }

    private void PreCreateSprites(JObject json)
    {
        //SPRITE
        JToken jsonImage = json["image"];
        if (jsonImage != null && jsonImage.Type != JTokenType.Null)
        {
            string localImagePath = (string)jsonImage["path"];
            SpriteImport.CreateSprite(localImagePath, importFolder, spriteFolderPath);
        }

        //CHILDREN
        JArray children = (JArray)json["children"];
        for (int i = children.Count - 1; i >= 0; --i)
        {
            PreCreateSprites((JObject)children[i]);
        }
    }
      
    private JArray PreParseJson(){
        string jsonPath = Path.Combine(importFolder, jsonFileName);
         
        if (File.Exists(jsonPath))
        {
            //read & parse text to json
            string fullJsonAsText = File.ReadAllText(jsonPath);
            JArray json = JArray.Parse(fullJsonAsText);
            return json;
        }else{
            return null;
        }
    }
   
    private void Parse(JObject json, RectTransform parent, Vector2 parentPos)
    {
        string name = (string)json["name"];

        //instantiate
        //Image image = PrefabUtility.InstantiatePrefab(imagePrefab) as Image; //still linked to the prefab
        Image image = Instantiate<Image>(imagePrefab);

        image.name = name;
        image.rectTransform.SetParent(parent, false);

        JToken jsonFrame = null;

        //SPRITE AND SIZE
        JToken jsonImage = json["image"];
        if (jsonImage != null && jsonImage.Type != JTokenType.Null)
        {

            //layers.Add(name, image);

            jsonFrame = jsonImage["frame"];

            string localImagePath = (string)jsonImage["path"];
            Sprite spr = SpriteImport.GetSprite(localImagePath, spriteFolderPath);
            if (spr != null)
            {
                
                image.sprite = spr;
                image.SetNativeSize();

                //visibility (invisible -> disabled image component)
                JToken jsonVisible = json["visible"];
                if (jsonVisible != null)
                {
                    //float alpha = (bool)jsonVisible ? 1f : 0f;
                    //image.color = new Color(255f, 255f, 255f, alpha);
                    if(!(bool)jsonVisible)
                    {
                        image.enabled = false;
                    }
                }
            }
        }else
        {
            if("artboard" == (string)json["kind"])
            {
                image.color = ParseColor((string)json["backgroundColor"]);
            }else
            {
                image.enabled = false;
            }

            jsonFrame = json["layerFrame"];
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)jsonFrame["width"]);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)jsonFrame["height"]);
        }

        //POSITION
        if (jsonFrame == null)
        {
            Debug.LogError("json entry without frame");
            return;
        }

        float x = (float)jsonFrame["x"] * importScale;
        float y = (float)jsonFrame["y"] * importScale;

        Vector2 globalPos = new Vector2(x, -y);
        Vector2 localPos = globalPos - parentPos; //TODO really needed? buggy for multiple artboards

        image.rectTransform.anchoredPosition = localPos;

        //CHILDREN
        JArray children = (JArray)json["children"];
        for (int i = children.Count - 1; i >= 0; --i)
        {
            Parse((JObject)children[i],image.rectTransform, globalPos);
        }
    }

    private Color ParseColor(string colorString){
        // "rgba(255, 255, 255, 1)
        string reduced = colorString.Replace("rgba(","").Replace(")","");
        string[] values = reduced.Split(',');
        float r = 0;
        float.TryParse(values[0], out r);
        float g = 0;
        float.TryParse(values[1], out g);
        float b = 0;
        float.TryParse(values[2], out b);
        float a = 0;
        float.TryParse(values[3], out a);
        
        return new Color(r,g,b,a);   
    }

    private void OnTestButton()
    {
        if (GUILayout.Button("Test Button", GUILayout.Height(30)))
        {
            //string importPath = "C:/Users/lange/Documents/Unity/_Assets/FramerPrototypes/Roulette/MixedFidelity3.framer/imported/test@1x/images/Layer-selected-mtg3oeiy.png";

            //string path = "Assets/ExampleSprite3D.png";

            //File.Copy(importPath, path);
            //AssetDatabase.Refresh();
            ////AssetDatabase.AddObjectToAsset(sprite, path); //needed?
            //AssetDatabase.SaveAssets();

            //TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

            ////ti.spritePixelsPerUnit = sprite.pixelsPerUnit;
            //ti.textureType = TextureImporterType.Sprite;
            ////ti.mipmapEnabled = false;
            //EditorUtility.SetDirty(ti);
            //ti.SaveAndReimport();

            //AssetImporter.


            //TODO reimport folder or everyone on GetSprite?

            SpriteImport.SetTexturesToSprites("Assets/Imported/sprites 11");
        }
    }
}

