using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace YKFramework.ResMgr
{
    public class DefAssetBundleLoad : IAssetBundleLoad
    {
        
        StringBuilder sb = new StringBuilder();
        private Dictionary<string,AssetBundle> mABs = new Dictionary<string, AssetBundle>();
        public DefAssetBundleLoad()
        {
           
        }

        public T LoadAsset<T>(string abName, string asset) where T : UnityEngine.Object
        {
            if (mABs.ContainsKey(abName))
            {
                return mABs[abName].LoadAsset<T>(asset);
            }
            else
            {
                Debug.LogError("ab不存在 abname："+abName);
                return null;
            }
        }

        public UnityEngine.Object LoadAsset(string abName, string asset)
        {
            return LoadAsset<UnityEngine.Object>(abName,asset);
        }
        
        public void LoadAssetAsync<T>(string abName, string asset, Action<T> callback) where T : UnityEngine.Object
        {
            
            if (mABs.ContainsKey(abName))
            {
                mABs[abName].LoadAssetAsync<T>(asset).completed += op =>
                {
                    AssetBundleRequest assetBundleRequest = op as AssetBundleRequest;
                    if (callback != null)
                    {
                        callback(assetBundleRequest.asset == null ? null : assetBundleRequest.asset as T);
                    }
                };
            }
            else
            {
                Debug.LogError("ab不存在 abname："+abName);
                
            }
        }

        public void LoadAssetAsync(string abName, string asset, Action<UnityEngine.Object> callback)
        {
            LoadAssetAsync<UnityEngine.Object>(abName,asset,callback);
        }
        

        public void GetAB(string abName, Action<AssetBundle> callback)
        {
            if (mABs.ContainsKey(abName))
            {
                if (callback != null) callback.Invoke(mABs[abName]);
                return;
            }

            sb.Clear();
            sb.Append(ResMgr.Instance.cfg.RootABPath)
                .Append("/")
                .Append(abName.ToLower())
                .Append(".")
                .Append(ResMgr.Instance.cfg.AssetBundleVariant);

            try
            {
                var ab = AssetBundle.LoadFromFile(sb.ToString());
                if (ab != null)
                {
                    mABs[abName] = ab;
                }
                if (callback != null) callback.Invoke(ab);
            }
            catch (Exception e)
            {
                Debug.LogError("加载ab失败 abName ="+ abName +" e："+e);
                //throw;
            }
           
            //throw new NotImplementedException();
        }

        public void Release(string abName, bool force = false)
        {
            if (mABs.TryGetValue(abName,out var ab))
            {
                ab.Unload(force);
                mABs.Remove(abName);
            }
        }

        public void ReleaseUnLoad()
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            AssetBundle.GetAllLoadedAssetBundles();
        }

    }
}