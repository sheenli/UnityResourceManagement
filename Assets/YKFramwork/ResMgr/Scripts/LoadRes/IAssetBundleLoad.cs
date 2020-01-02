using System;
using UnityEngine;

namespace YKFramework.ResMgr
{
    public interface IAssetBundleLoad
    {
        T LoadAsset<T>(string abName,string asset) where T : UnityEngine.Object;
        void LoadAssetAsync<T>(string abName,string asset,Action<T> callback) where T : UnityEngine.Object;

        UnityEngine.Object LoadAsset(string abName, string asset);
        void LoadAssetAsync(string abName, string asset, Action<UnityEngine.Object> callback);
        
        void GetAB(string abName, Action<AssetBundle> callback);
        void Release(string abName,bool force = false);
        void ReleaseUnLoad();
    }
}