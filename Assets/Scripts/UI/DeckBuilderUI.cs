using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace UnityTest.UI
{
    /// <summary>
    /// Screen space UI for the deck builder: hand counter, save/discard buttons and loading overlay
    /// </summary>
    public class DeckBuilderUI : MonoBehaviour
    {
        [Header("Counter")]
        [SerializeField] private TextMeshProUGUI cardCountText;

        [Header("Completion")]
        [SerializeField] private CanvasGroup completionGroup;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button discardButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private RectTransform loadingSpinner; 

        public Button SaveButton => saveButton;
        public Button DiscardButton => discardButton;

        private void Awake()
        {
            completionGroup.alpha = 0f;
            completionGroup.interactable = false; 
            completionGroup.blocksRaycasts = false;
            loadingOverlay.SetActive(false);
        }

        private void Update()
        {
            if (loadingOverlay.activeSelf)
            {
                loadingSpinner.Rotate(0, 0, -180f * Time.deltaTime); 
            }
        }

        public void SetCardCount(int current, int max)
        {
            cardCountText.text = $"{current}/ {max}"; 
        }

        public void ShowCompletionUI()
        {
            completionGroup.interactable = true;
            completionGroup.blocksRaycasts = true;
            completionGroup.DOFade(1f, 0.4f); 
        }

        public void ShowLoading(bool show)
        {
            loadingOverlay.SetActive(show);
        }

        public void ShowError(string errorMsg)
        {
            Debug.Log(errorMsg); 
        }
       
    }   
}

