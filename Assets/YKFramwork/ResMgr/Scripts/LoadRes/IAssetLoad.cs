using System;
using Object = UnityEngine.Object;

namespace YKFramwork.ResMgr
{
    public interface IResourceLoad
    {
        T LoadAsset<T>(string path) where T : Object;
        UnityEngine.Object LoadAsset(string path);
        void LoadAssetAsync<T>(string path,Action<T>callback) where T : Object;
        void LoadAssetAsync(string path, Action<UnityEngine.Object> callback);
    }
    
}