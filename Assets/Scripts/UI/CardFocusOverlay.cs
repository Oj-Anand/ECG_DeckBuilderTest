using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTest.Data;
using UnityTest.View;
using UnityTest.Views;

namespace UnityTest.UI
{
    /// <summary>
    /// Full-screen overlay that displays a single card at large size when
    /// the user clicks a thumbnail. Click anywhere to dismiss.
    /// </summary>
    public class CardFocusOverlay : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image focusIllustration;
        [SerializeField] private TMPro.TextMeshProUGUI focusName;
        [SerializeField] private TMPro.TextMeshProUGUI focusDescription;
        [SerializeField] private TMPro.TextMeshProUGUI focusCost;
        [SerializeField] private TMPro.TextMeshProUGUI focusAttack;
        [SerializeField] private TMPro.TextMeshProUGUI focusHealth;
        [SerializeField] private GameObject statsGroup; // toggle off for spells
        [SerializeField] private float fadeDuration = 0.25f;

        private CardView _spawnedCard;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Show(CardData data)
        {
            // Clean up any previously focused card.
            focusIllustration.sprite = data.Illustration;
            focusName.text = data.CardName;
            focusDescription.text = data.Description;
            focusCost.text = data.Cost.ToString();

            bool isCharacter = data.Type == CardType.Character;
            statsGroup.SetActive(isCharacter);
            if (isCharacter)
            {
                focusAttack.text = data.Attack.ToString();
                focusHealth.text = data.Health.ToString();
            }

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1f, fadeDuration);
        }

        public void Hide()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, fadeDuration);

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Hide();
        }
    }
}