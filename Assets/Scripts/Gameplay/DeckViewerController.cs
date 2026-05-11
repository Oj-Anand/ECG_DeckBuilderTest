using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityTest.Data;
using UnityTest.Services;
using UnityTest.View;

namespace UnityTest.UI
{
    public class DeckViewerController : MonoBehaviour
    {
        [SerializeField] private JsonBinDeckRepository deckRepository;
        [SerializeField] private CardRegistry registry;
        [SerializeField] private DeckEntryView deckEntryPrefab;
        [SerializeField] private Transform deckListContent;
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private GameObject emptyStateLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private Button buildNewDeckButton;
        [SerializeField] private CardFocusOverlay focusOverlay;

        private void Start()
        {
            backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
            buildNewDeckButton.onClick.AddListener(() => SceneManager.LoadScene("DeckBuilder"));
            LoadDecks();
        }

        private void LoadDecks()
        {
            loadingOverlay.SetActive(true);
            emptyStateLabel.SetActive(false);

            string userId = UserSession.GetUserId();
            deckRepository.LoadDecks(
                userId,
                decks =>
                {
                    loadingOverlay.SetActive(false);
                    if (decks.Count == 0)
                    {
                        emptyStateLabel.SetActive(true);
                    }
                    else
                    {
                        DisplayDecks(decks);
                    }
                },
                error =>
                {
                    loadingOverlay.SetActive(false);
                    Debug.LogError(error);
                    // Show error UI
                });
        }

        private void DisplayDecks(List<List<string>> decks)
        {
            for (int i = 0; i < decks.Count; i++)
            {
                DeckEntryView entry = Instantiate(deckEntryPrefab, deckListContent);
                var cardData = ResolveCards(decks[i]);
                entry.Bind(i + 1, cardData);
                entry.OnCardClicked += HandleCardClicked;
            }
        }
        private void HandleCardClicked(CardData card)
        {
            //Debug.Log($"[Controller] Routing to focus overlay: {card?.CardName}");
            focusOverlay.Show(card);
        }

        private List<CardData> ResolveCards(List<string> cardIds)
        {
            var result = new List<CardData>();
            foreach (var id in cardIds)
            {
                var card = registry.FindById(id);
                if (card != null) result.Add(card);
            }
            return result;
        }

    }
}