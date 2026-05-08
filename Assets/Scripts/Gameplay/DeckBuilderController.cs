using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityTest.Services;
using UnityTest.UI;
using UnityTest.View;

namespace UnityTest.Gameplay
{
    /// <summary>
    /// Owns the high level flow of the deckbuilding scene (draw -> animate -> add to hand -> completion check -> save)
    /// delegates each step to subsystems
    /// </summary>
    public class DeckBuilderController : MonoBehaviour
    {
        private const int HandLimit = 8;

        [SerializeField] private DeckStack deckStack;
        [SerializeField] private HandLayout handLayout;
        [SerializeField] private CardAnimator cardAnimator;
        [SerializeField] private DeckBuilderUI ui;
        [SerializeField] private JsonBinDeckRepository deckRepository;

        private bool _isAnimating;
        private bool _isComplete;

        private void Start()
        {
            deckStack.BuildDeck();
            ui.SetCardCount(0, HandLimit);
            ui.SaveButton.onClick.AddListener(OnSaveClicked);
            ui.DiscardButton.onClick.AddListener(OnDiscardClicked); 
        }

        private void Update()
        {
            if (_isAnimating || _isComplete) return;
            if (deckStack.IsEmpty) return;
            if (handLayout.Count >= HandLimit) return;

            if (Mouse.current.leftButton.wasPressedThisFrame && IsClickingDeck())
            {
                DrawTopCard(); 
            }
        }

        private bool IsClickingDeck()
        {
            //a simple raycast against the top cards collider would be most accurate, stub for now; we accept any click while th edeck is non empty
            return true; 
        }

        private void DrawTopCard()
        {
            CardView card = deckStack.PopTopCard(); 
            if(card == null) return;

            Vector3 originalScale = card.transform.localScale;

            _isAnimating = true;
            var (pos, rot, scale) = handLayout.GetSlotForNextCard(originalScale);

            cardAnimator.PlayDrawSequence(card, pos, rot, scale, () =>
            {
                handLayout.AddCard(card, originalScale);
                ui.SetCardCount(handLayout.Count, HandLimit);
                _isAnimating = false;

                if (handLayout.Count >= HandLimit)
                {
                    _isComplete = true;
                    ui.ShowCompletionUI();
                }
            }); 
        }

        private void OnSaveClicked()
        {
            ui.SaveButton.interactable = false;
            ui.DiscardButton.interactable = false;
            ui.ShowLoading(true);

            var cardIds = new List<string>();
            foreach (var card in handLayout.Cards)
            {
                cardIds.Add(card.Data.CardId);
            }

            string userId = UserSession.GetUserId();

            deckRepository.SaveDeck(
                userId,
                cardIds,
                success =>
                {
                    ui.ShowLoading(false);
                    if (success)
                    {
                        SceneManager.LoadScene("MainMenu");
                    }
                    else
                    {
                        // Error message already shown via onError callback
                        ui.SaveButton.interactable = true;
                        ui.DiscardButton.interactable = true;
                    }
                },
                errorMessage =>
                {
                    Debug.LogError($"Save failed: {errorMessage}");
                    ui.ShowError($"Couldn't save deck: {errorMessage}");
                });
        }

        private void OnDiscardClicked() 
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}

