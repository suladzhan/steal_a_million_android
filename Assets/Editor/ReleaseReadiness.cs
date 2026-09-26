using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StealAMillion.Editor
{
    public static class ReleaseReadiness
    {
        public const string Version="0.3.0";public const int Code=12;
        public static void Configure(){PlayerSettings.bundleVersion=Version;PlayerSettings.Android.bundleVersionCode=Code;
            PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;PlayerSettings.Android.forceInternetPermission=false;
            if(ReleaseSettings.Current==null){var config=ScriptableObject.CreateInstance<ReleaseSettings>();AssetDatabase.CreateAsset(config,"Assets/Resources/Runner/ReleaseSettings.asset");}
            var importer=AssetImporter.GetAtPath("Assets/Resources/Runner/Audio/music-v002.wav") as AudioImporter;
            if(importer!=null){var s=importer.defaultSampleSettings;s.loadType=AudioClipLoadType.DecompressOnLoad;s.compressionFormat=AudioCompressionFormat.Vorbis;s.quality=.65f;importer.defaultSampleSettings=s;importer.SaveAndReimport();}
            AssetDatabase.SaveAssets();Debug.Log("Major release configuration prepared");}
        public static void Validate(){var config=ReleaseSettings.Current;
            if(config==null||string.IsNullOrWhiteSpace(config.publisherName)||string.IsNullOrWhiteSpace(config.supportEmail)||!config.supportEmail.Contains("@")||!Uri.TryCreate(config.privacyUrl,UriKind.Absolute,out var uri)||uri.Scheme!="https")
                throw new InvalidOperationException("Publish configuration needs the developer name, support email and public HTTPS privacy URL in Runner/ReleaseSettings.");
            if(!PlayerSettings.Android.useCustomKeystore)throw new InvalidOperationException("Configure an upload keystore outside the repository before building the upload AAB.");
            Debug.Log("RELEASE_METADATA_PASS");}
    }
}
