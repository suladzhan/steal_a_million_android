using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace StealAMillion.Editor
{
    public static class AndroidBuild
    {
        [MenuItem("Steal A Million/Build/Android APK (Development)")]
        public static void Apk() { Build(false); }

        [MenuItem("Steal A Million/Build/Android AAB (Release)")]
        public static void Aab() { Build(true); }
        public static void ReleaseApk(){Build(false,true);}

        private static void Build(bool bundle,bool release=false)
        {
            ProjectSetup.EnsureReady();
            ReleaseReadiness.Configure();if(bundle)ReleaseReadiness.Validate();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Install Android Build Support, SDK & NDK Tools and OpenJDK in Unity Hub.");
            if (bundle && !PlayerSettings.Android.useCustomKeystore)
                throw new InvalidOperationException("Configure your upload keystore in Player Settings > Publishing Settings before creating a release AAB.");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Unable to switch to Android. In batch mode pass -buildTarget Android.");
            Directory.CreateDirectory("Builds/Android");
            EditorUserBuildSettings.buildAppBundle = bundle;
            string destination = "Builds/Android/StealAMillion-V2." + (bundle ? "aab" : "apk");
            var balance=Resources.Load<RunnerConfig>("Runner/Balance");bool originalMock=balance.mockAds;
            if(bundle||release){balance.mockAds=false;EditorUtility.SetDirty(balance);AssetDatabase.SaveAssets();}
            try{
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = ProjectSetup.Scenes,
                target = BuildTarget.Android,
                locationPathName = destination,
                options = bundle||release ? BuildOptions.None : BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android build failed: " + report.summary.result + ". Check the Unity log.");
            Debug.Log("Build complete: " + Path.GetFullPath(destination));
            }finally{
                // Building the player can reload assets and invalidate the original object.
                balance=Resources.Load<RunnerConfig>("Runner/Balance");
                if(balance==null)throw new InvalidOperationException("Unable to restore Runner/Balance after building.");
                balance.mockAds=originalMock;EditorUtility.SetDirty(balance);AssetDatabase.SaveAssets();
            }
        }
    }
}
