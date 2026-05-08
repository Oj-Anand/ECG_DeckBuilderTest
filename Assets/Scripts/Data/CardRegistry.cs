using UnityEngine;
using System.Collections.Generic;

namespace UnityTest.Data
{
    /// <summary>
    /// Central registry of all CardData SOs
    /// Provides lookup by id for the presistence layer
    /// Provides bulk access for the deck builder
    /// </summary>
    [CreateAssetMenu(fileName = "CardRegistry", menuName = "Cards/CardRegistry")]
    public class CardRegistry : ScriptableObject
    {
        [SerializeField] private List<CardData> allCards = new List<CardData>(); 

        public IReadOnlyList<CardData> AllCards => allCards;

        public CardData FindById(string cardId)
        {
            foreach (CardData card in allCards)
            {
                if(card.CardId == cardId) return card;
            }

            return null; 
        }

    }
}

