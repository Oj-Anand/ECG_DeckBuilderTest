using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityTest.Data;
using UnityTest.Views;

namespace UnityTest.UI
{
    /// <summary>
    /// One row in the deck viewer's scroll list
    /// Displays a deck number and 8 card thumbnails
    /// Forwards thumbnail clicks upward via event
    /// </summary>
    public class DeckEntryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI deckLabel;
        [SerializeField] private Transform thumbnailContainer;
        [SerializeField] private CardThumbnail thumbnailPrefab;

        public event Action<CardData> OnCardClicked;

        public void Bind(int deckNumber, List<CardData> cards)
        {
            deckLabel.text = $"Deck {deckNumber}";

            // Clear any existing thumbnails
            foreach (Transform child in thumbnailContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var card in cards)
            {
                CardThumbnail thumb = Instantiate(thumbnailPrefab, thumbnailContainer);
                thumb.Bind(card);
                thumb.OnClicked += HandleThumbnailClicked;
            }
        }

        private void HandleThumbnailClicked(CardData card)
        {
            OnCardClicked?.Invoke(card);
        }
    }
}