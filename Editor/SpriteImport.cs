using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;

public class SpriteImport  {

    public static string CreateFolder(string folderName)
    {
        //folder to store sprites
        if (!AssetDatabase.IsValidFolder("Assets/Imported"))
        {
            AssetDatabase.CreateFolder("Assets", "Imported");
        }
        string guid = AssetDatabase.CreateFolder("Assets/Imported", folderName);

        return AssetDatabase.GUIDToAssetPath(guid);
    }

    public static void CreateSprite(string localImagePath, string importFolder, string spriteFolder)
    {
        if (localImagePath != null)
            {
                string imagePath = Path.Combine(importFolder, localImagePath); //TODO combine uses an \ under windows
                if (File.Exists(imagePath))
                {

                    string spriteDest = GetSpriteLocation(localImagePath, spriteFolder);
                    File.Copy(imagePath, spriteDest);
                }
            }
    }

    public static Sprite GetSprite(string localImagePath, string spriteFolder)
    {
        string spriteLoc = GetSpriteLocation(localImagePath, spriteFolder);
        return (Sprite)AssetDatabase.LoadAssetAtPath(spriteLoc, typeof(Sprite));
    }

    private static string GetSpriteLocation(string localImagePath, string spriteFolder)
    {
        string imageFileName = Path.GetFileName(localImagePath);

        return Path.Combine(spriteFolder, imageFileName);
    }

    public static void SetTexturesToSprites(string folderPath)
    {
        bool dirty = false;
        string[] assetGUIDs = AssetDatabase.FindAssets("", new string[] { folderPath });
        foreach (string guid in assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

            if (ti.textureType != TextureImporterType.Sprite)
            {
                ti.textureType = TextureImporterType.Sprite;
                dirty = true;
            }
            //ti.mipmapEnabled = false;
            //EditorUtility.SetDirty(ti);
            //ti.SaveAndReimport(); //takes to long if we do it one by one
        }

        //force reimport for sprites in 3D
        if (dirty)
        {
            AssetDatabase.ImportAsset(folderPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            Debug.Log("Reimported all textures as sprites");
        }
    }

    public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

        return NewSprite;
    }

    public static Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
}
