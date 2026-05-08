using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityTest.Data;

namespace UnityTest.Services
{
    /// <summary>
    /// IDeckRepository implementation that uses JSONBin.io as the backend
    /// Stores all users in a single shared bin, keyed by user_id
    /// </summary>
    public class JsonBinDeckRepository : MonoBehaviour, IDeckRepository
    {
        [Header("JSONBin Configuration")]
        [SerializeField] private string binId;
        [SerializeField] private string masterKey;

        private const string ApiBaseUrl = "https://api.jsonbin.io/v3/b";
        private string BinUrl => $"{ApiBaseUrl}/{binId}";
        private string LatestUrl => $"{BinUrl}/latest";

        public void SaveDeck(string userId, List<string> cardIds, Action<bool> onComplete, Action<string> onError = null)
        {
            StartCoroutine(SaveDeckRoutine(userId, cardIds, onComplete, onError));
        }

        public void LoadDecks(string userId, Action<List<List<string>>> onLoaded, Action<string> onError = null)
        {
            StartCoroutine(LoadDecksRoutine(userId, onLoaded, onError));
        }

        private IEnumerator SaveDeckRoutine(string userId, List<string> cardIds, Action<bool> onComplete, Action<string> onError)
        {
            //fetch current bin contents
            UserCollection collection = null;
            string fetchError = null;
            yield return FetchCollection(c => collection = c, e => fetchError = e);

            if (fetchError != null)
            {
                onError?.Invoke(fetchError);
                onComplete?.Invoke(false);
                yield break;
            }

            //locate or create the user's record, append the new deck
            UserRecord record = collection.users.Find(u => u.user_id == userId);
            if (record == null)
            {
                record = new UserRecord { user_id = userId };
                collection.users.Add(record);
            }
            record.decks.Add(new DeckRecord { card_ids = new List<string>(cardIds) });

            //write the updated collection back
            string putError = null;
            yield return PutCollection(collection, e => putError = e);

            if (putError != null)
            {
                onError?.Invoke(putError);
                onComplete?.Invoke(false);
                yield break;
            }

            onComplete?.Invoke(true);
        }

        private IEnumerator LoadDecksRoutine(string userId, Action<List<List<string>>> onLoaded, Action<string> onError)
        {
            UserCollection collection = null;
            string fetchError = null;
            yield return FetchCollection(c => collection = c, e => fetchError = e);

            if (fetchError != null)
            {
                onError?.Invoke(fetchError);
                yield break;
            }

            UserRecord record = collection.users.Find(u => u.user_id == userId);
            var decks = new List<List<string>>();
            if (record != null)
            {
                foreach (var deck in record.decks)
                {
                    decks.Add(new List<string>(deck.card_ids));
                }
            }
            onLoaded?.Invoke(decks);
        }

        private IEnumerator FetchCollection(Action<UserCollection> onSuccess, Action<string> onError)
        {
            using UnityWebRequest req = UnityWebRequest.Get(LatestUrl);
            req.SetRequestHeader("X-Master-Key", masterKey);
            yield return req.SendWebRequest();

            Debug.Log($"URL hit: {req.url}");
            Debug.Log($"Response code: {req.responseCode}");
            Debug.Log($"Response body: {req.downloadHandler.text}");

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Fetch failed: {req.error}");
                yield break;
            }

            // JSONBin wraps the bin contents inside a "record" field
            JsonBinResponse wrapper = JsonUtility.FromJson<JsonBinResponse>(req.downloadHandler.text);
            UserCollection collection = wrapper.record;


            if (collection == null) collection = new UserCollection();
            onSuccess?.Invoke(collection);
        }

        private IEnumerator PutCollection(UserCollection collection, Action<string> onError)
        {
            string json = JsonUtility.ToJson(collection);
            using UnityWebRequest req = UnityWebRequest.Put(BinUrl, json);
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Master-Key", masterKey);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Save failed: {req.error}");
            }
        }

        // JSONBin's GET response wraps the actual data inside a "record" field
        // and adds a "metadata" field. I only care about "record"
        [Serializable]
        private class JsonBinResponse
        {
            public UserCollection record;
        }
    }
}