
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YKFramwork.ResMgr
{
    public class DefResLoadCfg : IResLoadCfg
    {
        private bool mSimulateAssetBundle = false;
        private const string editorExternalResDir = "Assets/ExternalRes";
        private const string editorResJsonPath = "Assets/defaultres.json";

        public DefResLoadCfg()
        {
            RootABPath = Application.streamingAssetsPath;
            AssetBundleVariant = "bytes";
#if !UNITY_EDITOR && UNITY_ANDROID
            RootABPath = Application.dataPath + "!assets";
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
            RootABUrl = "file:///" + Application.streamingAssetsPath;
#elif !UNITY_EDITOR && UNITY_IOS
            RootABUrl = "file://" + Application.streamingAssetsPath;
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
        public ResJsonData ResData
        {
            get
            {
                
                bool editor = false;
#if UNITY_EDITOR
                editor = !SimulateAssetBundle;
                
#endif
                if (mResJsonData == null)
                {
                    if (editor)
                    {
                        var text = AssetDatabase.LoadAssetAtPath<TextAsset>(editorResJsonPath);
                        mResJsonData = JsonUtility.FromJson<ResJsonData>(text.text);
                        text = null;
                    }
                    else
                    {
                        string path = RootABPath + "/defaultres.json";
                        var text = File.ReadAllText(path);
                        mResJsonData = JsonUtility.FromJson<ResJsonData>(text);
                    }
                }
                return mResJsonData;
            }
        }
        public string RootABPath { get; private set; }
        public string RootABUrl { get; private set; }
        public string AssetBundleVariant { get; private set; }
        public string EditorResJsonPath
        {
            get { return editorResJsonPath; }
        }
    }
}