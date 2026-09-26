using System;
using System.IO;
using UnityEngine;

namespace StealAMillion
{
    public sealed class RunnerPerformanceProbe : MonoBehaviour
    {
        [Serializable] public class Report {public string version,device;public int frames,over33ms;public float seconds,averageFps,p95ms,maxMs;}
        readonly int[] histogram=new int[251];int count,slow;float total,max;bool active,collect;object observedRun;
        public int Frames=>count;
        void Awake(){collect=Debug.isDebugBuild||File.Exists(Path.Combine(Application.persistentDataPath,"QA","enable-profile"));}
        void Update(){if(!collect)return;var game=GameManager.Instance;bool running=game!=null&&game.IsRunning;
            if(!running){if(active&&count>0&&game!=null&&(game.Session.Run.state==Core.RunnerState.Finished||game.Session.Run.state==Core.RunnerState.Broke)){Write();active=false;}return;}
            if(!ReferenceEquals(observedRun,game.Session.Run)||!active){observedRun=game.Session.Run;Array.Clear(histogram,0,histogram.Length);count=slow=0;total=max=0;active=true;}
            float ms=Time.unscaledDeltaTime*1000;histogram[Mathf.Clamp(Mathf.CeilToInt(ms),0,250)]++;count++;total+=Time.unscaledDeltaTime;max=Mathf.Max(max,ms);if(ms>33.34f)slow++;
        }
        public Report Snapshot(){int sum=0,percentile=0;for(int i=0;i<histogram.Length;i++){sum+=histogram[i];if(sum>=count*.95f){percentile=i;break;}}
            return new Report{version=Application.version,device=SystemInfo.deviceModel,frames=count,seconds=total,averageFps=count/Mathf.Max(.001f,total),p95ms=percentile,maxMs=max,over33ms=slow};}
        void Write(){try{var folder=Path.Combine(Application.persistentDataPath,"QA");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"performance-latest.json"),JsonUtility.ToJson(Snapshot(),true));}catch(Exception ex){Debug.LogWarning("Performance report: "+ex.Message);}}
    }
}
