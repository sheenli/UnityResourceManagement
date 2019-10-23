using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YKFramwork.ResMgr
{
    public class ResMgr : MonoBehaviour
    {
        private Dictionary<string, UnityEngine.Object> mAllRes = new Dictionary<string, UnityEngine.Object>();

        public IAssetBundleLoad ABMgr { set; get; }

        public IResourceLoad ResourceLoad { set; get; }

        public IResLoadCfg cfg {  set; get; }

        public static ResMgr Instance { get; set; }


        private void Awake()
        {
            Instance = this;
            if (ABMgr == null) ABMgr = new DefAssetBundleLoad();
            if (ResourceLoad == null) ResourceLoad = new DefResourceLoad();
            if (cfg == null) cfg = new DefResLoadCfg();
        }

        public bool HasAsset(string address)
        {
            return mAllRes.ContainsKey(address);
        }

        #region 同步获取资源



        public T GetRes<T>(string address) where T : Object
        {
            if (mAllRes.ContainsKey(address))
            {
                return mAllRes[address] as T;
            }

            return null;
        }

        public UnityEngine.Object GetRes(string address)
        {
            return GetRes<UnityEngine.Object>(address);
        }

        public T GetResByUrl<T>(string url) where T : Object
        {
            ResInfoData data = cfg.ResData.GetResInfoByUrl(url);
            if (data == null)
            {
                Debug.LogError("不存在这个url ：" + url);
            }
            else
            {
                return GetRes<T>(data.address);
            }

            return null;
        }

        public UnityEngine.Object GetResByUrl(string url)
        {
            return GetResByUrl<UnityEngine.Object>(url);
        }

        #endregion

        public void GetResAsync<T>(string address, Action<T> callback) where T : UnityEngine.Object
        {
            UnityEngine.Object asset = null;
            var data = cfg.ResData.GetResInfo(address);
            if (data == null)
            {
                Debug.LogError("不存在这个地址 addr:" + address);
            }
            else
            {
                if (data.isResourcesPath)
                {
                    LoadForResource(data.address, a => { asset = a; });
                }
                else
                {
                    LoadForAB(data.address, a => { asset = a; },true);
                }
            }

            if (callback != null)
            {
                callback(asset as T);
            }
        }

        public void GetResAsync(string address, Action<UnityEngine.Object> callback)
        {
            GetResAsync<UnityEngine.Object>(address, callback);
        }

        public void GetResAsyncByUrl<T>(string url, Action<T> callback) where T : UnityEngine.Object
        {
            var data = cfg.ResData.GetResInfoByUrl(url);
            if (data == null)
            {
                Debug.LogError("不存在这个url url:" + url);
                if (callback != null) callback(null);
            }
            else
            {
                GetResAsync(data.address, callback);
            }
        }

        public void GetResAsyncByUrl(string url, Action<UnityEngine.Object> callback)
        {
            GetResAsyncByUrl<UnityEngine.Object>(url, callback);
        }

        public LoadGroup LoadGroup(string groupName, Action<string> completed, Action<float, string> itemCompleted)
        {
            
            var req = new LoadGroup(groupName, completed, itemCompleted);
            return req.Load();
        }

        public void LoadForResource(string addr, Action<UnityEngine.Object> callback)
        {
            var info = cfg.ResData.GetResInfo(addr);
            if (info == null)
            {
                Debug.LogError("资源不存在 addr：" + addr);
                if (callback != null) callback(null);
            }
            else
            {
                LoadForResourceByUrl(info.url, callback);
            }
        }

        public void LoadForResourceByUrl(string url, Action<UnityEngine.Object> callback)
        {
            var info = cfg.ResData.GetResInfoByUrl(url);
            if (info == null)
            {
                Debug.LogError("资源路径不存在 url：" + url);
                if (callback != null) callback(null);
            }
            else
            {
                Object asset;
                if (mAllRes.ContainsKey(info.address)) asset = mAllRes[info.address];
                else
                {
                    asset = ResourceLoad.LoadAsset(url.Replace("r://", "").Replace(info.type, ""));
                    if (asset == null)
                    {
                        Debug.LogError("加载Resource资源失败 url:" + url);
                    }
                    else
                    {
                        mAllRes[info.address] = asset;
                    }
                }
                
                if (callback != null) callback(asset);
            }
        }

        public void LoadForABByUrl(string url, Action<UnityEngine.Object> callback,bool releaseAB = false)
        {
            bool needLoadAb = false;
            var info = cfg.ResData.GetResInfoByUrl(url);
            if (info == null)
            {
                Debug.LogError("资源路径不存在 url：" + url);
                if (callback != null) callback(null);
                return;
            }

            if (mAllRes.ContainsKey(info.address))
            {
                if (callback != null) callback(mAllRes[info.address]);
                return;
            }
            
#if UNITY_EDITOR
            needLoadAb = cfg.SimulateAssetBundle;
#else
            needLoadAb = true;
#endif
            if (!needLoadAb)
            {
#if UNITY_EDITOR
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(info.path);
                if (asset == null)
                {
                    Debug.LogError("资源加载失败：url=" + url + " /path=" + info.path);
                }
                else
                {
                    mAllRes[info.address] = asset;
                }
                
                if (callback != null) callback(asset);
                return;
#endif
            }

            ABMgr.GetAB(info.ABName, a =>
            {
                UnityEngine.Object asset = null;
                if (a == null)
                {
                    Debug.LogError("加载assetbundle失败url：" + url);
                }
                else
                {
                    string assetName = Path.GetFileName(url);
                    asset = ABMgr.LoadAsset(info.ABName, assetName);
                    if (asset == null)
                    {
                        Debug.LogError("资源加载失败：url=" + url);
                    }
                    else
                    {
                        mAllRes[info.address] = asset;
                    }

                    if (releaseAB)
                    {
                        ABMgr.Release(info.ABName);
                    }
                }

                if (callback != null) callback(asset);
            });
        }

        public void LoadForAB(string addr, Action<UnityEngine.Object> callback,bool releaseAB = false)
        {
            var info = cfg.ResData.GetResInfo(addr);
            if (info == null)
            {
                Debug.LogError("资源不存在 addr：" + addr);
                if (callback != null) callback(null);
            }
            else
            {

                LoadForABByUrl(info.url, callback,releaseAB);
            }
        }

        public void Release(string addr, bool force = false)
        {
            if (mAllRes.ContainsKey(addr))
            {
                if (force)
                {
                    Resources.UnloadAsset(mAllRes[addr]);
                }

                mAllRes.Remove(addr);
            }
        }

        public void UnloadUnused()
        {
            var list = new List<string>();
            var keys = mAllRes.Keys.ToList();
            for (var index = 0; index < keys.Count; index++)
            {
                var addr = keys[index];
                var info = cfg.ResData.GetResInfo(addr);
                if (!info.isKeepInMemory)
                {
                    list.Add(addr);
                }
            }

            for (var index = 0; index < list.Count; index++)
            {
                var addr = list[index];
                mAllRes.Remove(addr);
            }

            GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }
}