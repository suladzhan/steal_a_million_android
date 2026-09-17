#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using StealAMillion.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StealAMillion.Tests
{
    public sealed class VisualFlowTests
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        [UnityTest, Timeout(180000)]
        public IEnumerator PortraitScreensAndCompleteGameFlow()
        {
            Assert.That(Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY"), Is.Not.Null.And.Not.Empty,
                "Run with Tools/Test-Unity.ps1 -Visual to isolate test saves.");
            Assert.That(Resources.Load<TMP_Settings>("TMP Settings"), Is.Not.Null);
            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find("Code-based tests runner"));
            new GameObject("Test Bootstrap").AddComponent<GameManager>();
            var game = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            Assert.That(game, Is.Not.Null);
            game.SendMessage("OnApplicationFocus", true);
            game.ResetProgress();
            yield return null;

            foreach (int height in new[] { 960, 1170, 1200 })
            {
                Resize(540, height);
                yield return null;
                yield return null;
                yield return new WaitForSeconds(.15f);
                game.MainMenu();
                yield return null;
                Capture("menu-" + height, 540, height);
                game.NewRun();
                yield return null;
                Capture("game-" + height, 540, height);
                game.OpenSettings();
                yield return null;
                Capture("settings-" + height, 540, height);
                game.OpenShop();
                yield return null;
                Capture("shop-" + height, 540, height);
            }

            game.MainMenu();
            Press("NEW GAME");
            Assert.That(game.UI.HasModal, Is.True);
            Press("CANCEL");
            Assert.That(game.UI.HasModal, Is.False);
            game.NewRun();
            var safe = FindButton("SAFE");
            var risk = FindButton("RISK");
            Press(safe);
            Press(risk);
            Assert.That(game.Session.Money.Current, Is.EqualTo(200), "Two simultaneous choices resolve once.");
            game.Pause();
            Assert.That(Time.timeScale, Is.Zero);
            game.MainMenu();
            Press("CONTINUE");
            game.SkipResult();
            yield return Until(() => game.Session.Save.state == RunState.Playing);
            Assert.That(game.Session.Save.decisionIndex, Is.EqualTo(1));

            game.SendMessage("OnApplicationPause", true);
            long before = game.Session.Money.Current;
            Press("SAFE");
            Assert.That(game.Session.Money.Current, Is.EqualTo(before));
            game.SendMessage("OnApplicationPause", false);

            game.NewRun();
            game.Session.Decision.riskProbability = 0;
            Press("RISK");
            Assert.That(game.Session.Save.pendingResult.after, Is.Zero);
            yield return new WaitForSeconds(.2f);
            Capture("suspense", 540, 1200);
            yield return Until(() => game.Session.Save.state == RunState.GameOver, 6);
            yield return null;
            Capture("game-over", 540, 1200);
            Press("SECOND CHANCE  /  TEST AD");
            game.SecondChance();
            yield return Until(() => game.Session.Save.state == RunState.Playing, 4);
            Assert.That(game.Session.Money.Current, Is.EqualTo(500));
            Assert.That(game.Session.Save.secondChanceUsed, Is.True);
            game.Session.Decision.riskProbability = .8f;

            game.NewRun();
            for (int i = 0; i < 30; i++)
            {
                Press(game.Session.Decision.safeReward.mode == RewardMode.Multiply ? "KEEP" : "SAFE");
                game.SkipResult();
                yield return Until(() => game.Session.Save.state != RunState.ShowingResult);
            }
            Assert.That(game.Session.Save.state, Is.EqualTo(RunState.Victory));
            Assert.That(game.Session.Money.Current, Is.EqualTo(1000000));
            yield return new WaitForSeconds(.3f);
            Capture("victory", 540, 1200);
            game.OpenShop();
            game.Purchase("gold");
            Assert.That(game.Session.Save.equippedCosmetic, Is.EqualTo("gold"));
            game.OpenSettings();
            var toggle = game.UI.GetComponentsInChildren<Toggle>().First();
            toggle.isOn = false;
            Assert.That(game.Session.Save.soundEnabled, Is.False);
            var persisted = new SaveManager().Load();
            Assert.That(persisted.equippedCosmetic, Is.EqualTo("gold"));
            Assert.That(persisted.soundEnabled, Is.False);
            game.MainMenu();
        }

        private static IEnumerator Until(Func<bool> condition, float timeout = 3)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < end) yield return null;
            Assert.That(condition(), Is.True, "Timed out waiting for game state.");
        }

        private static Button FindButton(string label)
        {
            var game = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            return game.UI.GetComponentsInChildren<Button>().First(button =>
                button.GetComponentsInChildren<TMP_Text>().Any(text => text.text == label));
        }

        private static void Press(string label) { Press(FindButton(label)); }
        private static void Press(Button button)
        {
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }

        private static void Resize(int width, int height)
        {
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance", StaticFlags).GetValue(null);
            var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType");
            string targetGroup = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "Android" : "Standalone";
            var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(groupType, targetGroup) });
            var sizeType = assembly.GetType("UnityEditor.GameViewSize");
            var modeType = assembly.GetType("UnityEditor.GameViewSizeType");
            object size = Activator.CreateInstance(sizeType, InstanceFlags, null,
                new[] { Enum.Parse(modeType, "FixedResolution"), (object)width, height, "SAM QA " + height }, null);
            int index = (int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group, null)
                + (int)group.GetType().GetMethod("GetCustomCount").Invoke(group, null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
            var gameViewType = assembly.GetType("UnityEditor.GameView");
            var view = EditorWindow.GetWindow(gameViewType);
            gameViewType.GetProperty("selectedSizeIndex", InstanceFlags).SetValue(view, index);
            view.Repaint();
            var playModeType = assembly.GetType("UnityEditor.PlayModeView");
            playModeType.GetProperty("targetSize", InstanceFlags).SetValue(view, new Vector2(width, height));
        }

        private static void Capture(string name, int width, int height)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var art in UnityEngine.Object.FindObjectsByType<GameArt>(FindObjectsSortMode.None))
            {
                Assert.That(art.GetComponent<CanvasRenderer>(), Is.Not.Null, "Missing renderer: " + art.kind);
                Assert.That(art.canvasRenderer.materialCount, Is.GreaterThan(0), "Missing material: " + art.kind);
            }
            var truncations = new System.Collections.Generic.List<string>();
            foreach (var text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                text.ForceMeshUpdate();
                if (text.isTextTruncated) truncations.Add(text.text + " rect=" + text.rectTransform.rect + " font=" + text.fontSize);
                Assert.That(text.rectTransform.rect.width, Is.GreaterThan(0));
            }
            var render = typeof(EditorGUIUtility).GetMethod("RenderPlayModeViewCamerasInternal", StaticFlags);
            Assert.That(render, Is.Not.Null);
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            render.Invoke(null, new object[] { target, 0, Vector2.zero, false, false });
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var capture = new Texture2D(width, height, TextureFormat.RGB24, false);
            capture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            capture.Apply();
            if (SystemInfo.graphicsUVStartsAtTop)
            {
                var pixels = capture.GetPixels32();
                var row = new Color32[width];
                for (int y = 0; y < height / 2; y++)
                {
                    Array.Copy(pixels, y * width, row, 0, width);
                    Array.Copy(pixels, (height - 1 - y) * width, pixels, y * width, width);
                    Array.Copy(row, 0, pixels, (height - 1 - y) * width, width);
                }
                capture.SetPixels32(pixels);
                capture.Apply();
            }
            RenderTexture.active = previous;
            string directory = Path.GetFullPath("TestResults/Screenshots");
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, name + ".png"), capture.EncodeToPNG());
            int bright = capture.GetPixels32().Count(pixel => pixel.r > 100 || pixel.g > 100 || pixel.b > 100);
            UnityEngine.Object.DestroyImmediate(capture);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            Assert.That(bright, Is.GreaterThan(width * height / 100), "Rendered screen is blank: " + name);
            Assert.That(truncations, Is.Empty, "Truncated text in " + name + " screen=" + Screen.width + "x" + Screen.height + ": " + string.Join("; ", truncations));
        }
    }
}
#endif
