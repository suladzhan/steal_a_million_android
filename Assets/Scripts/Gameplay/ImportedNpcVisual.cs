using UnityEngine;
using StealAMillion.Core;

namespace StealAMillion
{
    public sealed class ImportedNpcVisual : MonoBehaviour
    {
        ImportedRunnerVisual visual;
        void Awake(){visual=gameObject.AddComponent<ImportedRunnerVisual>();visual.Apply(new RunnerSave());visual.Tint("#253A70");visual.Accessory("mask","#16202A");GetComponentInChildren<Animator>().cullingMode=AnimatorCullingMode.CullUpdateTransforms;foreach(var renderer in GetComponentsInChildren<SkinnedMeshRenderer>())renderer.updateWhenOffscreen=false;}
        void Update(){visual.Tick(true,true,0,0,0,"jump");}
    }
}
