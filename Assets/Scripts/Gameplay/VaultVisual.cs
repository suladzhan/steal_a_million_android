using UnityEngine;

namespace StealAMillion
{
    public sealed class VaultVisual : MonoBehaviour
    {
        public Transform hinge;
        private float progress;
        private bool opening;
        private Quaternion closedRotation;
        public void Open(){if(opening||hinge==null)return;closedRotation=hinge.localRotation;opening=true;}
        private void Update()
        {
            if(!opening||hinge==null)return;
            progress=Mathf.MoveTowards(progress,1,Time.deltaTime*.7f);
            hinge.localRotation=Quaternion.AngleAxis(Mathf.SmoothStep(0,105,progress),hinge.parent.InverseTransformDirection(transform.up))*closedRotation;
        }
    }
}
