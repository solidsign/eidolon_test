using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Server
{
    public abstract class ServerBase : MonoBehaviour
    {
        public abstract IEnumerator SendPostRequest<T>(string url, T data, Action onSuccess, Action onError);
    }
    
    public class Server : ServerBase
    {
        [SerializeField] private bool _sendActualRequests = false;
        [SerializeField] private bool _returnSuccess = true;
        
        public override IEnumerator SendPostRequest<T>(string url, T data, Action onSuccess, Action onError)
        {
            if (_sendActualRequests is false)
            {
                yield return SendStubRequest(onSuccess, onError);
                yield break;
            }
            
            using var request = UnityWebRequest.Post(url, JsonConvert.SerializeObject(data));
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onError?.Invoke();
            }
        }

        private IEnumerator SendStubRequest(Action onSuccess, Action onError)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (_returnSuccess) onSuccess?.Invoke();
            else onError?.Invoke();
        }
    }
}