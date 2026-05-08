using System;
using System.Collections.Generic;

namespace UnityTest.Data
{
    /// <summary>
    /// Wire format for the entire JSONBin contents which is a list of all users
    /// This is what gets serialized to/from the remote bin
    /// </summary>
    [Serializable]
    public class UserCollection
    {
        public List<UserRecord> users = new List<UserRecord>();
    }

    /// <summary>
    /// A single user's saved data: their UUID and their list of saved decks
    /// Each deck is itself an array of 8 card IDs
    /// </summary>
    [Serializable]
    public class UserRecord
    {
        public string user_id;
        public List<DeckRecord> decks = new List<DeckRecord>();
    }

    /// <summary>
    /// A single saved deck which is a list of card ID strings
    /// Wrapped in a class because Unity's JsonUtility cannot serialize
    /// nested arrays/lists directly 
    /// </summary>
    [Serializable]
    public class DeckRecord
    {
        public List<string> card_ids = new List<string>();
    }
}