using UnityEngine;
using System;

namespace UnityTest.Services
{
    /// <summary>
    /// Centralizes UUID storage and retrieval
    /// The UUID is the only piece of state stored locally (PlayerPrefs)
    /// all other persistence goes through IDeckRepository
    /// </summary>
    public static class UserSession
    {
        private const string UserIdKey = "user_id";

        public static bool HasUser() => PlayerPrefs.HasKey(UserIdKey);

        public static string GetUserId()
        {
            return PlayerPrefs.GetString(UserIdKey, null);
        }

        public static string CreateUser()
        {
            string id = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(UserIdKey, id);
            PlayerPrefs.Save();
            return id;
        }
    }
}