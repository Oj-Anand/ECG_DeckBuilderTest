using System;
using System.Collections.Generic;

namespace UnityTest.Services
{
    /// <summary>
    /// Persistence contract for saving and loading decks
    /// Implementations may target any backend (REST, local, mock)
    /// All methods are asynchronous and report results via callbacks
    /// </summary>
    public interface IDeckRepository
    {
        /// <summary>
        /// Appends a new deck to the user's saved collection
        /// </summary>
        /// <param name="userId">The user's UUID.</param>
        /// <param name="cardIds">The 8 card IDs forming the deck.</param>
        /// <param name="onComplete">Invoked with success=true on save, false on any error.</param>
        /// <param name="onError">Optional. Invoked with a human-readable error message on failure.</param>
        void SaveDeck(string userId, List<string> cardIds, Action<bool> onComplete, Action<string> onError = null);

        /// <summary>
        /// Loads all decks previously saved by the user.
        /// </summary>
        /// <param name="userId">The user's UUID.</param>
        /// <param name="onLoaded">Invoked with the user's decks (empty list if none).</param>
        /// <param name="onError">Optional. Invoked with a human-readable error message on failure.</param>
        void LoadDecks(string userId, Action<List<List<string>>> onLoaded, Action<string> onError = null);
    }
}