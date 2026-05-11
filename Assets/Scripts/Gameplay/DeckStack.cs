using UnityEngine;
using System.Collections.Generic;
using UnityTest.Data;
using UnityTest.View;

namespace UnityTest.Gameplay
{
    /// <summary>
    /// Manages visual deck of face down cards
    /// Spawns initial pool, shuffles and stacks them, and exposes pop/peek operations
    /// </summary>
    public class DeckStack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardRegistry registry;
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform deckAnchor;
        
        [Header("Stack visuals")]
        [SerializeField] private float stackOffset = 0.005f; //y offset per card
        [SerializeField] private Vector3 faceDownEuler = new Vector3(-90f, 0f, 0f);

        private readonly List<CardView> _cards = new List<CardView>();

        public int Count => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        public void BuildDeck()
        {
            ClearDeck();

            var pool = new List<CardData>(registry.AllCards);
            Shuffle(pool);

            for (int i = 0; i < pool.Count; i++)
            {
                CardView view = Instantiate(cardPrefab, transform);
                view.Bind(pool[i]); 

                Vector3 pos = deckAnchor.position + Vector3.up * (stackOffset * i);
                view.transform.position = pos;
                view.transform.rotation = Quaternion.Euler(faceDownEuler); 

                _cards.Add(view);
            }

        }

        public CardView PopTopCard()
        {
            if (_cards.Count == 0) return null; 
            int topIndex = _cards.Count - 1;
            CardView top = _cards[topIndex];
            _cards.RemoveAt(topIndex);
            return top;
        }

        private void ClearDeck()
        {
            foreach (CardView card in _cards)
            {
                if (card != null) Destroy(card.gameObject); 
            }

            _cards.Clear(); 
        }

        // Fisher Yates Shuffle 
        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]); 
            }
        }
    }  
}

