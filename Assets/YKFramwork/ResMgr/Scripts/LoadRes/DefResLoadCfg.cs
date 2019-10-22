
using System;
using System.Collections;
using System.IO;
using System.Net;
#if UNITY_EDITOR
    
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace YKFramwork.ResMgr
{
    public class DefResLoadCfg : IResLoadCfg
    {
        private bool mSimulateAssetBundle = true;
        private const string editorExternalResDir = "Assets/ExternalRes";
        private const string editorResJsonPath = "Assets/defaultres.json";

        public DefResLoadCfg()
        {
            RootABPath = Application.streamingAssetsPath;
            AssetBundleVariant = "bytes";
            mRootAbUrl = "file:///" + Application.streamingAssetsPath;
#if !UNITY_EDITOR && UNITY_ANDROID
            RootABPath = Application.dataPath + "!assets";
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
            mRootAbUrl = Application.streamingAssetsPath;
#elif !UNITY_EDITOR && UNITY_IOS
            mRootAbUrl = "file://" + Application.streamingAssetsPath;
#endif

        }
        public bool SimulateAssetBundle 
        {
            get { return mSimulateAssetBundle; }
        }

        public string EditorExternalResDir
        {
            get { return editorExternalResDir; }
        }

        private ResJsonData mResJsonData;
        private readonly string mRootAbUrl;

        public ResJsonData ResData
        {
            get
            {
                return mResJsonData;
            }
        }
        public string RootABPath { get; }

        public string RootABUrl
        {
            get { return mRootAbUrl; }
        }

        public string AssetBundleVariant { get; }
        public string EditorResJsonPath
        {
            get { return editorResJsonPath; }
        }

        public void Init(Action callback)
        {
            bool editor = false;
#if UNITY_EDITOR
            editor = !SimulateAssetBundle;
                
#endif
            if (mResJsonData == null)
            {
                if (editor)
                {
#if UNITY_EDITOR
                    var text = AssetDatabase.LoadAssetAtPath<TextAsset>(editorResJsonPath);
                    mResJsonData = JsonUtility.FromJson<ResJsonData>(text.text);
                    text = null;
                    if (callback != null)
                    {
                        callback();
                    }
#endif
                }
                else
                {
                    var path = RootABUrl + "/defaultres.json";
                    var getUrl = UnityWebRequest.Get(path);
                    getUrl.SendWebRequest().completed += op =>
                    {
                        var resp = op as UnityWebRequestAsyncOperation;
                        if (string.IsNullOrEmpty(resp.webRequest.error))
                        {
                            mResJsonData = JsonUtility.FromJson<ResJsonData>(resp.webRequest.downloadHandler.text);
                        }
                        else
                        {
                            throw new Exception("加载配置文件失败"+resp.webRequest.error);
                        }
                    };
//                    ResMgr.Instance.StartCoroutine(loadJson(callback));
                }
            }
            
            
        }
    }
}