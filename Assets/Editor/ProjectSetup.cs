using System;
using System.IO;
using System.Linq;
using StealAMillion.Core;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StealAMillion.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public static readonly string[] Scenes = { "Assets/Scenes/Boot.unity", "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Game.unity" };

        static ProjectSetup()
        {
            if (!Application.isBatchMode) EditorApplication.delayCall += FirstImport;
        }

        private static void FirstImport()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (IsReady()) return;
            try { Configure(); }
            catch (Exception e) { Debug.LogError("Project setup failed. Use Steal A Million > Setup Project.\n" + e); }
        }

        [MenuItem("Steal A Million/Setup Project")]
        public static void Configure()
        {
            EnsureFonts();
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Art/Icons");
            AssetDatabase.Refresh();
            bool missingConfig = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Resources/GameConfig.asset") == null;
            bool firstSetup = missingConfig || !File.Exists(Scenes[0]);
            if (missingConfig)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GameConfig>(), "Assets/Resources/GameConfig.asset");
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && string.IsNullOrEmpty(activeScene.path))
            {
                // Unity refuses additive creation with an untitled scene. Preserve any existing work.
                string path = activeScene.rootCount == 0 && !File.Exists(Scenes[0]) ? Scenes[0]
                    : AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Workspace.unity");
                if (!EditorSceneManager.SaveScene(activeScene, path)) throw new IOException("Unable to preserve the current scene.");
            }
            foreach (string path in Scenes)
            {
                if (File.Exists(path)) continue;
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, path);
                EditorSceneManager.CloseScene(scene, true);
            }
            EditorBuildSettings.scenes = Scenes.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            EditorSettings.serializationMode = SerializationMode.ForceText;
            PlayerSettings.companyName = "Sulik";
            PlayerSettings.productName = "Steal A Million";
            if (firstSetup) PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.sulik.stealamillion");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            if (firstSetup) PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets.Length > 0)
            {
                var settings = new SerializedObject(assets[0]);
                var input = settings.FindProperty("activeInputHandler");
                if (input != null) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            }
            CreateIcon();
            AssetDatabase.SaveAssets();
            ValidateData();
            Debug.Log("Steal A Million is configured. Open Assets/Scenes/Boot.unity and press Play.");
        }

        public static void EnsureReady()
        {
            if (!IsReady()) Configure();
            else ValidateData();
        }

        private static bool IsReady()
        {
            return AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Resources/GameConfig.asset") != null
                && Scenes.All(File.Exists) && Resources.Load<TMP_Settings>("TMP Settings") != null;
        }

        private static void EnsureFonts()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") != null) return;
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly);
            if (package == null) throw new InvalidOperationException("The Unity UI package has not finished importing.");
            var files = Directory.GetFiles(package.resolvedPath, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
            if (files.Length == 0) throw new FileNotFoundException("Use Window > TextMeshPro > Import TMP Essential Resources, then rerun setup.");
            AssetDatabase.ImportPackage(files[0], false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
                throw new InvalidOperationException("TMP resources are still importing. Run Setup Project once import finishes.");
        }

        [MenuItem("Steal A Million/Validate Data")]
        public static void ValidateData()
        {
            var asset = Resources.Load<TextAsset>("Data/decisions");
            if (asset == null) throw new InvalidOperationException("Missing decisions.json");
            var data = JsonUtility.FromJson<DecisionCatalog>(asset.text);
            var errors = data.Validate();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors.ToArray()));
            Debug.Log("Validated " + data.decisions.Length + " decisions across " + data.decisions.Last().level + " levels.");
        }

        private static void CreateIcon()
        {
            const string path = "Assets/Art/Icons/AppIcon.png";
            if (!File.Exists(path))
            {
                const int size = 512;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                string[] glyph = { "00100", "01111", "10100", "01110", "00101", "11110", "00100" };
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        int gx = (x - 146) / 44;
                        int gy = 6 - (y - 102) / 44;
                        bool inGlyph = x >= 146 && x < 366 && y >= 102 && y < 410 && gx >= 0 && gx < 5 && gy >= 0 && gy < 7 && glyph[gy][gx] == '1';
                        bool border = x >= 50 && x < 462 && y >= 50 && y < 462 && (x < 57 || x > 454 || y < 57 || y > 454);
                        texture.SetPixel(x, y, inGlyph ? UIFactory.Gold : border ? UIFactory.Surface : UIFactory.Background);
                    }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
        }
    }
}
