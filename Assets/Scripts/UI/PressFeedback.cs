using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StealAMillion
{
    public sealed class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private float target=1;
        public void OnPointerDown(PointerEventData data)
        {
            var button = GetComponent<Selectable>();
            if (button != null && button.IsInteractable()) target=.965f;
        }
        public void OnPointerUp(PointerEventData data) { target=1; }
        public void OnPointerExit(PointerEventData data) { target=1; }
        private void Update(){float scale=Mathf.Lerp(transform.localScale.x,target,1-Mathf.Exp(-28*Time.unscaledDeltaTime));transform.localScale=Vector3.one*scale;}
        private void OnDisable() { target=1;transform.localScale = Vector3.one; }
    }
}
