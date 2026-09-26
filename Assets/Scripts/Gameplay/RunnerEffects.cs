using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace StealAMillion
{
    public sealed class RunnerEffects : MonoBehaviour
    {
        private readonly List<ParticleSystem> pool=new List<ParticleSystem>();
        private readonly List<TextMeshPro> textPool=new List<TextMeshPro>();
        private readonly List<float> timers=new List<float>();
        private int next,nextText;
        private readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        private void Awake()
        {
            var particleMaterial=Resources.Load<Material>("Runner/Materials/Particles");
            foreach(var name in new[]{"spark","note","confetti","streak","dust"})materials[name]=Resources.Load<Material>("Runner/Particles/"+name)??particleMaterial;
            for(int i=0;i<8;i++)
            {
                var go=new GameObject("Cash Burst "+i);go.transform.SetParent(transform,false);
                var p=go.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=p.main;main.playOnAwake=false;main.loop=false;main.duration=.8f;main.startLifetime=.8f;main.startSpeed=3;main.startSize=.13f;main.gravityModifier=.5f;main.maxParticles=64;main.simulationSpace=ParticleSystemSimulationSpace.World;
                var emission=p.emission;emission.enabled=false;var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=40;shape.radius=.2f;
                p.GetComponent<ParticleSystemRenderer>().sharedMaterial=particleMaterial;pool.Add(p);
            }
            for(int i=0;i<10;i++){var text=RunnerArt.Text(transform,"Floating Change","",Vector3.zero,4,"#35D06F",new Vector2(4,.7f));text.gameObject.SetActive(false);textPool.Add(text);timers.Add(0);}
        }
        public void Clear()
        {
            foreach(var system in pool)system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            for(int i=0;i<textPool.Count;i++){timers[i]=0;textPool[i].gameObject.SetActive(false);}
        }
        public void Burst(Vector3 position,Color color,bool big=false)
        { Cue(big?SoundCue.Win:SoundCue.Safe,position,color,big); }
        public void Cue(SoundCue cue,Vector3 position,Color color,bool big=false)
        {
            bool celebration=cue==SoundCue.Jackpot||cue==SoundCue.Victory;
            bool loss=cue==SoundCue.Loss||cue==SoundCue.Police||cue==SoundCue.GameOver;
            string texture=celebration?"confetti":cue==SoundCue.Cash?"note":loss?"streak":"spark";
            var p=pool[next++%pool.Count];p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            p.transform.position=position+Vector3.up;p.transform.rotation=Quaternion.Euler(-90,0,0);
            p.GetComponent<ParticleSystemRenderer>().sharedMaterial=materials[texture];
            var main=p.main;main.startColor=celebration?new ParticleSystem.MinMaxGradient(color,Color.white):new ParticleSystem.MinMaxGradient(color);
            main.startSpeed=celebration?4:loss?2.8f:big?3.5f:2;
            main.startLifetime=celebration?1.1f:loss?.4f:.6f;main.startSize=cue==SoundCue.Cash?.16f:.12f;
            main.gravityModifier=celebration?.8f:.35f;main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
            p.Emit(celebration?32:big?20:10);
        }
        public void Floating(string value,Color color,Vector3 position)
        {
            int index=nextText++%textPool.Count;var text=textPool[index];text.text=value;text.color=color;text.transform.position=position+new Vector3(0,2.5f,0);text.gameObject.SetActive(true);timers[index]=1;
        }
        private void Update()
        {
            var camera=Camera.main;
            for(int i=0;i<timers.Count;i++)if(timers[i]>0)
            {
                timers[i]-=Time.deltaTime;var text=textPool[i];text.transform.position+=Vector3.up*Time.deltaTime*1.3f;
                if(camera!=null)text.transform.rotation=camera.transform.rotation;
                text.transform.localScale=Vector3.one*(1+.15f*Mathf.Sin((1-timers[i])*Mathf.PI));
                var color=text.color;color.a=Mathf.Clamp01(timers[i]*2);text.color=color;if(timers[i]<=0)text.gameObject.SetActive(false);
            }
        }
    }
}
