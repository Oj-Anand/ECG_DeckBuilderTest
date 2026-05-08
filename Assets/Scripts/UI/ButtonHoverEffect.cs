using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace UnityTest.UI
{
    /// <summary>
    /// Adds a subtle scale up + tween effect on pointer hover
    /// Attach to any UI element to element 
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float duration = 0.15f;

        private Vector3 _orignalScale;
        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>(); 
            _orignalScale = _rect.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData) 
        {
            _rect.DOScale(_orignalScale * hoverScale, duration).SetEase(Ease.OutQuad); 
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _rect.DOScale(_orignalScale, duration).SetEase(Ease.OutQuad);
        }
    }  
}


