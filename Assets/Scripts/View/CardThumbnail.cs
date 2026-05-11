using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityTest.Data;
using System;

namespace UnityTest.Views
{
    /// <summary>
    /// Small UI representation of a card used in the deck viewer's list
    /// Clicking it raises an event so the parent can spawn a full-size focus view
    /// </summary>
    public class CardThumbnail : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image illustrationImage;
        [SerializeField] private TextMeshProUGUI nameText;

        private CardData _data;
        public CardData Data => _data;

        public event Action<CardData> OnClicked;

        public void Bind(CardData data)
        {
            _data = data;
            illustrationImage.sprite = data.Illustration;
            nameText.text = data.CardName;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log($"[Thumbnail] Clicked: {_data?.CardName}");
            OnClicked?.Invoke(_data);
        }
    }
}