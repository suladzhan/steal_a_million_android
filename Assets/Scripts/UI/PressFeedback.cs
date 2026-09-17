using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StealAMillion
{
    public sealed class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public void OnPointerDown(PointerEventData data)
        {
            var button = GetComponent<Selectable>();
            if (button != null && button.IsInteractable()) transform.localScale = Vector3.one * .975f;
        }
        public void OnPointerUp(PointerEventData data) { transform.localScale = Vector3.one; }
        public void OnPointerExit(PointerEventData data) { transform.localScale = Vector3.one; }
        private void OnDisable() { transform.localScale = Vector3.one; }
    }
}
