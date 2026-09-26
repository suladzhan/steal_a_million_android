using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace StealAMillion.Editor
{
    public static class RunnerFonts
    {
        [MenuItem("Steal A Million/Build Localization Fonts")]
        public static void Build()
        {
            string[] sources={"NotoSans-Bold.ttf","NotoSansArabic-Regular.ttf","NotoSansCJKsc-Regular.otf","NotoSansDevanagari-Regular.ttf","NotoSansThai-Regular.ttf"};
            string[] names={"Primary","Arabic","CJK","Devanagari","Thai"};
            var fonts=new List<TMP_FontAsset>();Directory.CreateDirectory("Assets/Resources/Runner/Fonts");AssetDatabase.Refresh();
            for(int i=0;i<sources.Length;i++)
            {
                string path="Assets/Resources/Runner/Fonts/"+names[i]+".asset";
                var asset=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if(asset==null)
                {
                    var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/"+sources[i]);
                    if(font==null)throw new FileNotFoundException("Run Tools/Fetch-RunnerDependencies.ps1: "+sources[i]);
                    asset=TMP_FontAsset.CreateFontAsset(font,72,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
                    asset.name=names[i];asset.atlasTextures[0].name=names[i]+" Atlas";asset.material.name=names[i]+" Material";
                    AssetDatabase.CreateAsset(asset,path);AssetDatabase.AddObjectToAsset(asset.material,asset);AssetDatabase.AddObjectToAsset(asset.atlasTextures[0],asset);
                }
                asset.isMultiAtlasTexturesEnabled=true;fonts.Add(asset);
            }
            fonts[0].fallbackFontAssetTable=fonts.Skip(1).ToList();
            var settings=new SerializedObject(Resources.Load<TMP_Settings>("TMP Settings"));settings.FindProperty("m_defaultFontAsset").objectReferenceValue=fonts[0];settings.ApplyModifiedPropertiesWithoutUndo();
            foreach(var font in fonts)EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();Debug.Log("Dynamic TMP fonts configured: Latin/Cyrillic/Turkish, Arabic, CJK, Devanagari, Thai.");
        }
    }
}
