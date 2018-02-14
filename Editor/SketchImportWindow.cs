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

    //private members for sprites and images
    string spriteAssetSubFolder = "sprites";
    string spriteAssetGUID = null;

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

    void OnGUI()
    {
        //TEST BUTTON
        // if (GUILayout.Button("Test Button", GUILayout.Height(30)))
        // {
        
        //     onTestButton();
        // }


        //SOURCE
        GUILayout.Label("Sketch Export (layers.json):",EditorStyles.boldLabel);
        if (GUILayout.Button(importFolder, EditorStyles.objectField))
        {
            OnSelectButton();
        }
        
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
        }
        GUI.enabled = true;
    }

    bool IsFormValid()
    {
        return rootTransform != null && !importFolder.Equals("") && imagePrefab != null;
    }

    void OnSelectButton()
    {
        //string path = EditorUtility.OpenFolderPanel("Select folder containing the json", importFolder, "");
        //if (!path.Equals(""))
        //{
        //    importFolder = path;
        //}

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
    }

    void OnImportButton()
    {
        if (!importFolder.Equals("") && rootTransform != null )
        {

            JArray json = PreParseJson();
            if(json!=null){
                
                //folder to store sprites
                CreateSpriteFolder();
                
                //create sprites
                foreach(JToken artboard in json){
                    PreCreateSprites((JObject)artboard);
                }
                 //this only takes the first artboard
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

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

    void PreCreateSprites(JObject json)
    {
        //SPRITE
        JToken jsonImage = json["image"];
        if (jsonImage != null && jsonImage.Type != JTokenType.Null)
        {
            CreateSprite(jsonImage);
        }

        //CHILDREN
        JArray children = (JArray)json["children"];
        for (int i = children.Count - 1; i >= 0; --i)
        {
            PreCreateSprites((JObject)children[i]);
        }
    }
    
    void CreateSpriteFolder(){
        //folder to store sprites
        if (!AssetDatabase.IsValidFolder("Assets/Imported"))
        {
            AssetDatabase.CreateFolder("Assets","Imported");
        }
        spriteAssetGUID = AssetDatabase.CreateFolder("Assets/Imported", spriteAssetSubFolder);
    }
    
    JArray PreParseJson(){
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
   

    void Parse(JObject json,RectTransform parent, Vector2 parentPos)
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

            Sprite spr = GetSprite(jsonImage);
            if (spr != null)
            {
                
                image.sprite = spr;
                image.SetNativeSize();

                JToken jsonVisible = json["visible"];
                if (jsonVisible != null)
                {
                    float alpha = (bool)jsonVisible ? 1f : 0f;
                    image.color = new Color(255f, 255f, 255f, alpha);
                }
            }else 
            {
                image.enabled = false;
            }
        }else
        {
            image.enabled = false;

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

        float x = (float)jsonFrame["x"];
        float y = (float)jsonFrame["y"];

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

    void CreateSprite(JToken jsonImage)
    {
        // http://stackoverflow.com/questions/24066400/checking-for-empty-null-jtoken-in-a-jobject
        if (jsonImage != null && jsonImage.Type != JTokenType.Null)
        {
            string localImagePath = (string)jsonImage["path"];

            if (localImagePath != null)
            {
                string imagePath = Path.Combine(importFolder, localImagePath);
                
                if (File.Exists(imagePath))
                {

                    string spriteDest = GetSpriteLocation(localImagePath);
                    File.Copy(imagePath, spriteDest);
                }
            }
        }
    }

    Sprite GetSprite(JToken jsonImage)
    {
        string localImagePath = (string)jsonImage["path"];
        string spriteLoc = GetSpriteLocation(localImagePath);
        return (Sprite)AssetDatabase.LoadAssetAtPath(spriteLoc, typeof(Sprite));
    }

    string GetSpriteLocation(string localImagePath)
    {
        string imageFileName = Path.GetFileName(localImagePath);

        string destFolder = AssetDatabase.GUIDToAssetPath(spriteAssetGUID);
        return Path.Combine(destFolder, imageFileName);
    }
    
    void LoadImagePrefab(){
        var script = MonoScript.FromScriptableObject( this );
        
        var scriptPath = AssetDatabase.GetAssetPath( script );
        string directoryPath = Path.GetDirectoryName(scriptPath);
        
        //absolute path from here
        DirectoryInfo pluginDir = Directory.GetParent(directoryPath);
        DirectoryInfo prefabsDir = pluginDir.GetDirectories("Prefabs")[0];
        string imagePrefabPath = prefabsDir.GetFiles("SketchImportImage.prefab")[0].FullName;
        
        string relativePath = imagePrefabPath.Replace(Application.dataPath,"Assets");
        imagePrefab = (Image)AssetDatabase.LoadAssetAtPath<Image>(relativePath);
    }

    void onTestButton()
    {
        //
    }
}
