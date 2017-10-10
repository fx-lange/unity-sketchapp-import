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
    Transform rootTransform = null;

    //Create a Menu Item so we can open this window
    [MenuItem("Window/Sketch Import")]
    static void OpenImportWindow()
    {
        SketchImportWindow window = EditorWindow.GetWindow<SketchImportWindow>(false, "Sketch Import");
        window.minSize = new Vector2(140, 130);
    }  

    void OnGUI()
    {
        ////TEST BUTTONS
        //if (GUILayout.Button("Test Button", GUILayout.Height(30)))
        //{
        //    string guid = AssetDatabase.CreateFolder("Assets", importFolder);
        //    string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
        //    Debug.Log(newFolderPath);
        //}


        //SOURCE
        GUILayout.Label("Sketch Export:",EditorStyles.boldLabel);
        if (GUILayout.Button(importFolder, EditorStyles.objectField))
        {
            OnSelectButton();
        }
        
        //ROOT TRANSFORM
        GUILayout.Label("Root:", EditorStyles.boldLabel);
        rootTransform = EditorGUILayout.ObjectField(rootTransform, typeof(Transform),true) as Transform;

        GUILayout.Space(10);
        //IMPORT
        GUI.enabled = IsFormValid();
        if (GUILayout.Button("Import", GUILayout.Height(25)))
        {
            OnImportButton();
        }
        GUI.enabled = true;
    }

    void onTestButton()
    {
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath("Assets/Import/Prefabs/Image.prefab");
        Debug.Log(objs.Length);

        Debug.Log(AssetDatabase.IsValidFolder("Assets/Import/Prefabs"));

        Image image = (Image)AssetDatabase.LoadAssetAtPath<Image>( "Assets/Import/Prefabs/Image.prefab");
        image.name = name;
        Debug.Log(image.name);
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
        }
    }

    void OnImportButton()
    {
        if (!importFolder.Equals("") && rootTransform != null )
        {

            string jsonPath = Path.Combine(importFolder, jsonFileName);
         
            if (File.Exists(jsonPath))
            {
                //folder to store sprites
                if (!AssetDatabase.IsValidFolder("Assets/Imported"))
                {
                    AssetDatabase.CreateFolder("Assets","Imported");
                }
                spriteAssetGUID = AssetDatabase.CreateFolder("Assets/Imported", spriteAssetSubFolder);
                
                //read & parse text to json
                string fullJsonAsText = File.ReadAllText(jsonPath);
                JArray json = JArray.Parse(fullJsonAsText);

                //parse json
                Undo.RegisterFullObjectHierarchyUndo(rootTransform, "Import Layers");
                Parse((JObject)json[0],rootTransform);

                //finish by refreshing all the assets
                AssetDatabase.Refresh();
            }

        }
    }

    void Parse(JObject json,Transform parent)
    {
        string imagePrefabPath = "Assets/Import/Prefabs/Image.prefab";

        string name = (string)json["name"];

        //load prefab from assets
        Image imagePrefab = (Image)AssetDatabase.LoadAssetAtPath<Image>(imagePrefabPath);
        
        //instantiate
        //Image image = PrefabUtility.InstantiatePrefab(imagePrefab) as Image; //still linked to the prefab
        Image image = Instantiate<Image>(imagePrefab);

        image.name = name;
        image.rectTransform.SetParent(parent, false);

        JToken jsonImage = json["image"];
        if (jsonImage != null && jsonImage.Type != JTokenType.Null)
        {

            //layers.Add(name, image);

            JToken jsonFrame = jsonImage["frame"];
            if (jsonFrame != null)
            {
                float x = (float)jsonFrame["x"];
                float y = (float)jsonFrame["y"];

                image.rectTransform.anchoredPosition = new Vector2(x, -y);
            }
            else
            {
                image.name += " no frame";
            }

            Sprite spr = CreateSprite(json["image"]);
            if (spr != null)
            {
                image.sprite = spr;
                image.SetNativeSize();
                image.color = new Color(255f, 255f, 255f, 50f); //?
            }else
            {
                image.enabled = false;
            }
        }else
        {
            image.enabled = false;
        }

        JArray children = (JArray)json["children"];
        for (int i = children.Count - 1; i >= 0; --i)
        {
            Parse((JObject)children[i],image.transform);
        }
    }

    bool IsFormValid()
    {
        return rootTransform != null && !importFolder.Equals("");
    }

    Sprite CreateSprite(JToken jsonImage)
    {

        // http://stackoverflow.com/questions/24066400/checking-for-empty-null-jtoken-in-a-jobject
        if (jsonImage != null && jsonImage.Type != JTokenType.Null)
        {
            JToken localImagePath = jsonImage["path"];

            if (localImagePath != null)
            {
                string imagePath = Path.Combine(importFolder, (string)localImagePath);
                
                if (File.Exists(imagePath))
                {
                    string imageFileName = (string)localImagePath;
                    imageFileName = imageFileName.Replace("images/", ""); //TODO regex

                    string destFolder = AssetDatabase.GUIDToAssetPath(spriteAssetGUID);
                    string spriteDest = Path.Combine(destFolder, imageFileName);

                    File.Copy(imagePath, spriteDest);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Sprite sprite = (Sprite)AssetDatabase.LoadAssetAtPath(spriteDest, typeof(Sprite));
                    return sprite;
                }
                else
                {
                    Debug.LogWarning("Not a png or doesn't exist:" + imagePath);
                }
            }

        }
        return null;
    }

}
