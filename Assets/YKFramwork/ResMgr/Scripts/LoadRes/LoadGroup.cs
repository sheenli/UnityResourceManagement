
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YKFramwork.ResMgr
{
    public class LoadGroup
    {
        private bool IsNeedLoadAB
        {
            get
            {
#if UNITY_EDITOR
                return ResMgr.Instance.cfg.SimulateAssetBundle;
#else
                return true;
#endif
            }
        }

        private int allNumber,currentNum;
        
        private string mGroupName;
        private List<string> mAssetAddrs;
        private List<string> mLoadAbs;
        private float progress = 0;
        private Action<string> OnCompleted;
        private Action<float,string> OnItemCompleted;
        public LoadGroup(string groupName, Action<string> completed,Action<float,string> itemCompleted)
        {
            OnCompleted = completed;
            OnItemCompleted = itemCompleted;
            mGroupName = groupName;
            mAssetAddrs = ResMgr.Instance.cfg.ResData.GetAssetsNames(groupName).ToList();
            mLoadAbs = new List<string>();

            for (var index = mAssetAddrs.Count - 1; index >= 0; index--)
            {
                var addr = mAssetAddrs[index];
                if (ResMgr.Instance.HasAsset(addr))
                {
                    mAssetAddrs.Remove(addr);
                    continue;
                }

                var info = ResMgr.Instance.cfg.ResData.GetResInfo(addr);
                if (info == null)
                {
                    Debug.LogError("无法找到资源信息addr：" + addr);
                }
                else
                {
                    if (!info.isResourcesPath && IsNeedLoadAB && !mLoadAbs.Contains(info.ABName))
                    {
                        mLoadAbs.Add(info.ABName);
                    }
                }
            }
        }

        public LoadGroup Load()
        {
            allNumber = mAssetAddrs.Count;
            currentNum = 0;
            LoadAb();
            return this;
        }


        private void LoadAb()
        {
            if (mLoadAbs.Count == 0)
            {
                LoadAsset();
            }
            else
            {
                ResMgr.Instance.ABMgr.GetAB(mLoadAbs[0],LoadABed);
            }
        }
        
        private List<string> allLoadABs = new List<string>();
        private void LoadABed(AssetBundle obj)
        {
            if (obj == null)
            {
                throw new Exception("加载ab失败ab名称："+mLoadAbs[0]);
            }
            else
            {
                allLoadABs.Add(mLoadAbs[0]);
                mLoadAbs.RemoveAt(0);
                LoadAb();
            }
        }

        private void LoadAsset()
        {
            if (mAssetAddrs.Count == 0)
            {
                Finished();
                return;
            }
            var assetAddr = mAssetAddrs[0];
            var resInfo = ResMgr.Instance.cfg.ResData.GetResInfo(assetAddr);
            if (!IsNeedLoadAB)
            {
                ResMgr.Instance.GetResAsync(assetAddr, a =>
                {
                    if (a == null)
                    {
                        Debug.LogError("加载资源失败：addr =" + assetAddr);
                    }
                    else
                    {
                        LoadAsseted(a);
                    }
                });
            }
            else
            {
                if (resInfo.isResourcesPath)
                    ResMgr.Instance.LoadForResource(resInfo.address,LoadAsseted);
                else
                {
                    ResMgr.Instance.LoadForAB(resInfo.address,LoadAsseted);
                }
            }
        }

        private void LoadAsseted(UnityEngine.Object asset)
        {
            currentNum++;
            progress = float.Parse((currentNum / (float) allNumber).ToString("0.00"));
            if(OnItemCompleted != null)
                OnItemCompleted(progress,mAssetAddrs[0]);
            mAssetAddrs.RemoveAt(0);
            if(mAssetAddrs.Count == 0) Finished();
            else
            {
                LoadAsset();
            }
        }

        private void Finished()
        {
            for (var index = 0; index < allLoadABs.Count; index++)
            {
                var abname = allLoadABs[index];
                ResMgr.Instance.ABMgr.Release(abname);
            }

            if(OnCompleted != null)
                OnCompleted(this.mGroupName);
        }
    }
}