using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityTest.View;

namespace UnityTest.Gameplay
{
    /// <summary>
    /// Arranges cards in a fan around a central pivot. 
    /// Recomputes layout whenever a card is added.
    /// </summary>
    public class HandLayout : MonoBehaviour
    {
        [SerializeField] private Transform handAnchor;
        [SerializeField] private float fanRadius = 4f;
        [SerializeField] private float maxSpreadDegrees = 40f;
        [SerializeField] private float anglePerCard = 6f;
        [SerializeField] private float layoutDuration = 0.3f;

        [Tooltip("Multiplier applied to the card's deck scale when it lands in hand.")]
        [SerializeField] private float handCardScaleMultiplier = 0.8f;

        [Tooltip("Half the card prefab's height in local units. Used to compensate for the card's center pivot.")]
        [SerializeField] private float cardHalfHeight = 5f;

        private readonly List<CardView> _hand = new List<CardView>();
        private Vector3 _cachedBaseScale = Vector3.one;

        public int Count => _hand.Count;
        public IReadOnlyList<CardView> Cards => _hand;

        public (Vector3 pos, Quaternion rot, Vector3 scale) GetSlotForNextCard(Vector3 baseScale)
        {
            int newCount = _hand.Count + 1;
            return ComputeSlot(_hand.Count, newCount, baseScale);
        }

        public void AddCard(CardView card, Vector3 baseScale)
        {
            _cachedBaseScale = baseScale;
            _hand.Add(card);
            RelayoutAllCards();
        }

        private void RelayoutAllCards()
        {
            for (int i = 0; i < _hand.Count; i++)
            {
                var (pos, rot, scale) = ComputeSlot(i, _hand.Count, _cachedBaseScale);
                _hand[i].transform.DOMove(pos, layoutDuration).SetEase(Ease.OutCubic);
                _hand[i].transform.DORotateQuaternion(rot, layoutDuration).SetEase(Ease.OutCubic);
                _hand[i].transform.DOScale(scale, layoutDuration).SetEase(Ease.OutCubic);
            }
        }

        private (Vector3 pos, Quaternion rot, Vector3 scale) ComputeSlot(int index, int total, Vector3 baseScale)
        {
            float spread = Mathf.Min(maxSpreadDegrees, anglePerCard * (total - 1));
            float startAngle = -spread / 2f;
            float angle = total <= 1 ? 0f : startAngle + (spread / (total - 1)) * index;

            Vector3 pivot = handAnchor.position + Vector3.down * fanRadius;
            Vector3 offset = Quaternion.Euler(0, 0, -angle) * Vector3.up * fanRadius;
            Vector3 pos = pivot + offset;

            Quaternion rot = handAnchor.rotation * Quaternion.Euler(0, 0, -angle);

            // Pivot compensation
            Vector3 finalScale = baseScale * handCardScaleMultiplier;
            pos += rot * Vector3.up * (cardHalfHeight * finalScale.y);

            return (pos, rot, finalScale);
        }
    }
}