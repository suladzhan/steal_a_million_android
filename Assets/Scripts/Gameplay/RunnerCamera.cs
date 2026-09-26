using UnityEngine;

namespace StealAMillion
{
    public sealed class RunnerCamera : MonoBehaviour
    {
        public Transform target;
        public bool preview,finish,shop;
        private Camera lens;
        private float shake,pulse;
        public void Impulse(bool positive){if(positive)pulse=1;else shake=.35f;}
        public void Snap(){Follow(true);}
        private void LateUpdate(){Follow(false);}
        private void Follow(bool snap)
        {
            if(target==null)return;if(lens==null)lens=GetComponent<Camera>();
            float dt=Time.unscaledDeltaTime;
            shake=Mathf.Max(0,shake-dt);pulse=Mathf.Max(0,pulse-dt);
            Vector3 offset=preview?new Vector3(0,5.8f,-9.5f):finish?new Vector3(0,6,-14):new Vector3(0,8.8f,-12);
            var position=new Vector3(finish?0:target.position.x*.15f,0,target.position.z)+offset;
            if(shake>0)position.x+=Mathf.Sin(Time.unscaledTime*80)*shake*.35f;
            transform.position=snap?position:Vector3.Lerp(transform.position,position,1-Mathf.Exp(-12*dt));
            var look=new Vector3(0,shop?-1.5f:preview?.8f:1.1f,target.position.z+(preview?0:finish?2:7));
            transform.rotation=Quaternion.LookRotation(look-transform.position);
            lens.fieldOfView=52+pulse*3;
        }
    }
}
