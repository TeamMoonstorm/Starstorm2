using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2
{
    [Serializable]
    public class KV
    {
        public string key;
        public int value;

        public KV(string key, int value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public class EventPayload
    {
        public string eventName;
        public KV data;

        public EventPayload(string eventName, KV data)
        {
            this.eventName = eventName;
            this.data = data;
        }
    }

    public static class SS2Telemetry
    {
        public static string Endpoint = "";

        public static void TrackEvent(string eventName, string key, int value)
        {
            if (string.IsNullOrEmpty(Endpoint) || key == null) return;

            var payload = new EventPayload(eventName, new KV(key, value));
            var json = JsonUtility.ToJson(payload);
            Send(json);
        }

        public static void TrackError(string errorMessage)
        {
            if (string.IsNullOrEmpty(Endpoint) || errorMessage == null) return;

            // Simple error payload
            var errorPayload = new ErrorPayload(errorMessage);
            var json = JsonUtility.ToJson(errorPayload);
            Send(json);
        }

        static void Send(string json)
        {
            if (string.IsNullOrEmpty(json)) return;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var req = new UnityWebRequest(Endpoint, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            _ = req.SendWebRequest(); // fire-and-forget; no coroutines here
        }

        [Serializable]
        private class ErrorPayload
        {
            public string errorMessage;
            public ErrorPayload(string msg) { errorMessage = msg; }
        }
    }
}
