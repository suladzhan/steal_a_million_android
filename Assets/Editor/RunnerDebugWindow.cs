using StealAMillion.Core;
using UnityEditor;
using UnityEngine;

namespace StealAMillion.Editor
{
    public sealed class RunnerDebugWindow : EditorWindow
    {
        private int level=1,world;
        [MenuItem("Steal A Million/Runner Debug")]
        public static void Open(){GetWindow<RunnerDebugWindow>("Runner Debug");}
        private void OnGUI()
        {
            var game=GameManager.Instance;
            if(!Application.isPlaying||game==null){EditorGUILayout.HelpBox("Enter Play Mode to use runner QA controls. This window is excluded from player builds.",MessageType.Info);return;}
            EditorGUILayout.LabelField("Bankroll",game.Session.Money.Format());
            EditorGUILayout.BeginHorizontal();
            foreach(int exponent in new[]{2,3,5,6,9,12})if(GUILayout.Button(BigMoney.Power10(exponent).Format()))
            {
                if(!game.Session.Run.active)game.Play();
                game.Session.SetMoney(BigMoney.Power10(exponent));game.Save();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Add 10,000 cosmetic coins + 10,000 XP")){RunnerProgress.AddCoins(game.Progress,10000);RunnerProgress.AddXp(game.Progress,10000);game.Save();}
            if(GUILayout.Button("Unlock cosmetics")){foreach(var item in game.Cosmetics)if(!game.Progress.owned.Contains(item.id))game.Progress.owned.Add(item.id);game.Save();}
            level=EditorGUILayout.IntSlider("Level",level,1,500);
            if(GUILayout.Button("Start selected level")){game.Progress.highestLevel=Mathf.Max(level,game.Progress.highestLevel);game.Play(level);}
            world=EditorGUILayout.IntSlider("World",world,0,game.Worlds.Count-1);
            if(GUILayout.Button("Preview selected world")){if(!game.Progress.worlds.Contains(world))game.Progress.worlds.Add(world);game.SelectWorld(world);}
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Next risk: win"))game.ForcedRisk=1;
            if(GUILayout.Button("Next risk: loss"))game.ForcedRisk=-1;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Bonus")){game.Progress.keys=3;game.Play(0,false,true);}
            if(GUILayout.Button("Endless")){game.Progress.highestLevel=Mathf.Max(game.Config.endlessUnlockLevel,game.Progress.highestLevel);game.Play(0,true);}
            if(GUILayout.Button("Finish"))game.Finish();
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Complete mission counters"))
            {
                var s=game.Progress.stats;s.completed=s.cash=s.safe=s.risks=s.wins=s.jackpots=s.avoided=s.perfect=s.powers=s.investments=s.keys=s.bonus=s.rareWins=10000;
                game.Save();game.UI.MissionScreen();
            }
            if(GUILayout.Button("Select track colliders")){Selection.activeGameObject=game.World.trackRoot.gameObject;if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.drawGizmos=true;}
            if(GUILayout.Button("Reset save")&&EditorUtility.DisplayDialog("Reset QA progress?","This removes the current local save.","Reset","Cancel"))game.ResetProgress();
        }
    }
}
