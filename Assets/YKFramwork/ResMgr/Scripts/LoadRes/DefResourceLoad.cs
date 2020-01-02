using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YKFramework.ResMgr
{
    public class DefResourceLoad:IResourceLoad
    {
        public T LoadAsset<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public Object LoadAsset(string path)
        {
            return LoadAsset<UnityEngine.Object>(path);
        }

        public void LoadAssetAsync<T>(string path, Action<T> callback) where T : Object
        {
            
            Resources.LoadAsync<T>(path).completed += operation =>
            {
                ResourceRequest request = operation as ResourceRequest;    
                if (callback != null)
                {
                    callback(request != null && request.asset == null ? null : request.asset as T);
                }
            };
        }

        public void LoadAssetAsync(string path, Action<Object> callback)
        {
            LoadAssetAsync<UnityEngine.Object>(path,callback);
        }
    }
}