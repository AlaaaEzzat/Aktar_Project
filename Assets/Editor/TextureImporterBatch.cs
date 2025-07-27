using UnityEngine;
using UnityEditor;

public class TextureImporterBatch : EditorWindow
{
	[MenuItem("Tools/Apply Texture Settings To All Sprites")]
	public static void ApplySettingsToAllTextures()
	{
		string[] textureGuids = AssetDatabase.FindAssets("t:Texture");
		int count = 0;

		foreach (string guid in textureGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

			if (importer == null)
				continue;

			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Single;
			importer.sRGBTexture = true;
			importer.alphaSource = TextureImporterAlphaSource.FromInput;
			importer.alphaIsTransparency = true;
			importer.isReadable = false;
			importer.mipmapEnabled = false;
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.filterMode = FilterMode.Bilinear;

			// Use SerializedObject for advanced fields
			SerializedObject so = new SerializedObject(importer);
			so.FindProperty("m_SpritePixelsToUnits").floatValue = 100f;

			var pivot = so.FindProperty("m_SpritePivot");
			if (pivot != null)
				pivot.vector2Value = new Vector2(0.5f, 0.5f); // Center

			var meshType = so.FindProperty("m_SpriteMeshType");
			if (meshType != null)
				meshType.intValue = 0; // FullRect = 0, Tight = 1

			so.ApplyModifiedProperties();

			// Compression settings
			TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
			settings.maxTextureSize = 2048;
			settings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
			settings.textureCompression = TextureImporterCompression.Compressed;
			importer.SetPlatformTextureSettings(settings);

			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			count++;
		}

		Debug.Log($"✅ Updated {count} textures to FullRect sprite mesh type.");
	}
}
