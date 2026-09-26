using UnityEngine;
using UnityEngine.EventSystems;

namespace StealAMillion
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RunnerController : MonoBehaviour
    {
        public float TargetX { get; private set; }
        public float ForwardSpeed { get; set; }
        public float HorizontalVelocity { get; private set; }
        public bool Active { get; set; }
        private CharacterController controller;
        private Vector2 previous;
        private bool dragging;
        private float minX=-3.6f,maxX=3.6f;
        public void Place(float x,float z)
        {
            if(controller==null)controller=GetComponent<CharacterController>();
            controller.enabled=false;transform.position=new Vector3(x,0,z);controller.enabled=true;TargetX=x;
        }
        public void Bounds(float min,float max){minX=min;maxX=max;TargetX=Mathf.Clamp(TargetX,min,max);}
        public void Drag(float normalizedDelta)
        {
            if(!Active)return;
            TargetX=Mathf.Clamp(TargetX+normalizedDelta*GameManager.Instance.Config.dragSensitivity,minX,maxX);
            if(Mathf.Abs(normalizedDelta)>.003f)GameManager.Instance.Progress.tutorialMove=true;
        }
        private void Update()
        {
            if(!Active||Time.timeScale==0){dragging=false;return;}
            if(Input.touchCount>0)
            {
                var touch=Input.GetTouch(0);
                if(touch.phase==TouchPhase.Began)dragging=EventSystem.current==null||!EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                if(dragging&&touch.phase==TouchPhase.Moved)Drag(touch.deltaPosition.x/Screen.width);
                if(touch.phase==TouchPhase.Ended||touch.phase==TouchPhase.Canceled)dragging=false;
            }
            else
            {
                if(Input.GetMouseButtonDown(0)){previous=Input.mousePosition;dragging=EventSystem.current==null||!EventSystem.current.IsPointerOverGameObject();}
                if(dragging&&Input.GetMouseButton(0)){Vector2 point=Input.mousePosition;Drag((point.x-previous.x)/Screen.width);previous=point;}
                if(Input.GetMouseButtonUp(0))dragging=false;
            }
            if(controller==null)controller=GetComponent<CharacterController>();
            float x=Mathf.MoveTowards(transform.position.x,TargetX,GameManager.Instance.Config.horizontalSpeed*Time.deltaTime);
            HorizontalVelocity=(x-transform.position.x)/Mathf.Max(.001f,Time.deltaTime);
            controller.Move(new Vector3(x-transform.position.x,-.6f*Time.deltaTime,ForwardSpeed*Time.deltaTime));
        }
    }
}
