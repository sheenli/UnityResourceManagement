using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using YKFramwork.ResMgr.Utils;

namespace YKFramwork.ResMgr.Editor
{
    public class AssetBuildView
    {
        private BuildTarget mBuildTarget = BuildTarget.Android;
        private BuildAssetBundleOptions mBuildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle
                                                                   | BuildAssetBundleOptions.DeterministicAssetBundle
                                                                   | BuildAssetBundleOptions.ForceRebuildAssetBundle;

        private string outPath;
        private string ver = "1.0.0";
        public void OnGUI(Rect rect)
        {
            
            GUILayout.BeginArea(rect, GUI.skin.GetStyle("CN Box"));
            GUILayout.Space(20);
            GUILayout.Width(100);
            mBuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(Language.BuildTarget, mBuildTarget);
            mBuildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(Language.BuildAssetBundleOptions, mBuildAssetBundleOptions);
            EditorPrefs.SetInt("mBuildTarget",(int)mBuildTarget);
            EditorPrefs.SetInt("BuildAssetBundleOptions",(int)mBuildAssetBundleOptions);
            ver = EditorGUILayout.TextField("version",ver);
            EditorGUILayout.BeginHorizontal();
            outPath = EditorGUILayout.TextField(Language.SavePath+"：",outPath);
            if (GUILayout.Button( "...",GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("save", Application.streamingAssetsPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    outPath = path;
                }
                EditorPrefs.SetString("ABoutPath",outPath);
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(15);
            
            EditorGUILayout.BeginHorizontal();
            var w = rect.width - 30;
            GUILayout.Space(10);
            var height = rect.height * 0.4;
            if (GUILayout.Button( Language.BuildSelctABs,GUILayout.Height((int)height),GUILayout.Width(w/2)))
            {
                BuildSelect(outPath,AssetMode.CurrentSelectsAbs,mBuildTarget,mBuildAssetBundleOptions);
            }
            GUILayout.Space(10);
            if (GUILayout.Button( Language.BuildAllABS,GUILayout.Height((int)height),GUILayout.Width(w/2)))
            {
                BuildAll(ver,outPath,mBuildTarget,mBuildAssetBundleOptions);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            
        }

        public void Reload()
        {
            mBuildTarget = (BuildTarget)EditorPrefs.GetInt("mBuildTarget", (int)EditorUserBuildSettings.activeBuildTarget);
            int flag = (int)(BuildAssetBundleOptions.UncompressedAssetBundle
                        | BuildAssetBundleOptions.DeterministicAssetBundle
                        | BuildAssetBundleOptions.ForceRebuildAssetBundle);
            mBuildAssetBundleOptions = (BuildAssetBundleOptions)EditorPrefs.GetInt("BuildAssetBundleOptions", flag);
            outPath = EditorPrefs.GetString("ABoutPath", Application.streamingAssetsPath);
            //throw new System.NotImplementedException();
        }


        public static void BuildAll(string ver,string outPath,BuildTarget target,BuildAssetBundleOptions options)
        {
            AssetMode.Rebuild();
            var buils = BuildSelect(outPath, AssetMode.GetAllBundleNames(), target, options);
            string newFileName = outPath+"/" + Path.GetFileName(ResConfig.ResJsonCfgFilePath);
            File.Copy(ResConfig.ResJsonCfgFilePath,newFileName,true);
            BuildABVersionInfo(outPath, ver, buils);
            GC.Collect();
            Resources.UnloadUnusedAssets();
            AssetDatabase.Refresh();
        }

        public static List<AssetBundleBuild> BuildSelect(string outPath,List<string> abs,BuildTarget target,BuildAssetBundleOptions options)
        {
            var needAdds = new List<AssetBundleBuild>();
            foreach (var ab in abs)
            {
                List<ResInfoData> list = AssetMode.getABAssets(ab);
                if (list.Count > 0)
                {
                    var bs = new AssetBundleBuild();
                    bs.assetBundleName = ab.ToLower();
                    bs.assetBundleVariant = "bytes";
                    List<string> assets = new List<string>();
                    foreach (var v in list)
                    {
                        if(!assets.Contains(v.path))
                            assets.Add(v.path);
                        Debug.Log("abname:"+v.ABName+"  /path:"+v.path);
                    }

                    bs.assetNames = assets.ToArray();
                    if(bs.assetNames.Length > 0)
                        needAdds.Add(bs);
                }
            }

            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            BuildPipeline.BuildAssetBundles(outPath, needAdds.ToArray(), options, target);
            return needAdds;
        }
        
        static string GetHash(string path)
        {
            //var hash = SHA256.Create();
            //var hash = MD5.Create();
            var hash = SHA1.Create();
            var stream = new FileStream(path, FileMode.Open);
            byte[] hashByte = hash.ComputeHash(stream);
            stream.Close();
            return BitConverter.ToString(hashByte).Replace("-", "");
        }

        static void BuildABVersionInfo(string path,string ver,List<AssetBundleBuild> builds)
        {
            try
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteString(ver);
                buffer.WriteInt(builds.Count);
                foreach (var b in builds)
                {
                    FileInfo fi = new FileInfo(path+"/"+b.assetBundleName.ToLower()+"."+b.assetBundleVariant);
                    buffer.WriteString(b.assetBundleName);
                    buffer.WriteString(GetHash(fi.FullName));
                    buffer.WriteLong(fi.Length);
                }
               
                File.WriteAllBytes(path+"/version",buffer.ToBytes());
                buffer.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}