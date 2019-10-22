using System;
using System.Collections.Generic;

namespace YKFramwork.ResMgr.Editor
{
    public interface IResConfig
    {
        string JsonFilePath { get; }
        string ExternalResDir { get; }
        List<Type> CanDropType { get; }
    }
    
    public class DefResCfg : IResConfig 
    {
        private List<Type> mCanDropType = new List<Type>()
        {
            typeof(UnityEngine.TextAsset),
            typeof(UnityEngine.Texture),
            typeof(UnityEngine.Texture2D),
            typeof(UnityEngine.Texture3D),
            typeof(UnityEngine.AudioClip),
            typeof(UnityEngine.Material),
            typeof(UnityEngine.GameObject),
        };

        public List<Type> CanDropType {
            get { return mCanDropType; }
        }
        public string JsonFilePath
        {
            get { return "Assets/defaultres.json";}
        }

        public string ExternalResDir
        {
            get { return "Assets/ExternalRes"; }
        }
    }
    
    public static class ResConfig
    {
        private static IResConfig mResCfgJsonPath;
        
        static ResConfig()
        {
            mResCfgJsonPath = new DefResCfg();
        }
        
        public static string ResJsonCfgFilePath
        {
            get
            {
                return mResCfgJsonPath.JsonFilePath;
            }
        }

        public static string ExternalResDir
        {
            get { return mResCfgJsonPath.ExternalResDir; }
        }

        public static List<Type> CanDropType
        {
            get { return mResCfgJsonPath.CanDropType; }
        }
    }
}