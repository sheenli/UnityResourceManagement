using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YKFramwork.ResMgr.Editor
{
    public class AssetMode
    {
        public static List<string> CurrentSelectsAbs = new List<string>();

        /// <summary>
        /// 资源信息
        /// </summary>
        public class AssetInfo
        {
            public ResInfoData data { get; }
            public int NameHashCode = 0;
            public string Name;
            public long size;

            public AssetInfo(int id, string name)
            {
                NameHashCode = id;
                this.Name = name;
                data = resInfo.GetResInfo(name);
                if (File.Exists(data.path))
                {
                    FileInfo info = new FileInfo(data.path);
                    size = info.Length;
                }
            }

            public string GetSizeString()
            {
                if (size == 0)
                    return "--";
                return EditorUtility.FormatBytes(size);
            }
        }

        /// <summary>
        /// 本地配置表的文件
        /// </summary>
        public static ResJsonData loaclFileresInfo;

        public static /*const*/ Color k_LightGrey = Color.grey * 1.5f;

        /// <summary>
        /// 资源信息
        /// </summary>
        public static ResJsonData resInfo;

        /// <summary>
        /// 重新查找所有资源
        /// </summary>
        public static void Rebuild()
        {

            if (File.Exists(ResConfig.ResJsonCfgFilePath))
            {
                string json = File.ReadAllText(ResConfig.ResJsonCfgFilePath);
                if (!string.IsNullOrEmpty(json))
                {
                    loaclFileresInfo = JsonUtility.FromJson<ResJsonData>(json);
                }
                else
                {
                    resInfo = new ResJsonData();
                }

                Refresh();
            }
            else
            {
                resInfo = new ResJsonData();
            }
        }

        public static bool ListIsSame(List<string> l1, List<string> l2)
        {
            if (l1.Count == l2.Count)
            {
                foreach (var v in l1)
                {
                    if (!l2.Contains(v))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
        
        public static bool ListIsSame(List<ResGroupCfg> l1, List<ResGroupCfg> l2)
        {
            if (l1.Count == l2.Count)
            {
                foreach (var v in l1)
                {
                    if (!l2.Exists(v2 => v2.groupName == v.groupName))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
        
        public static bool Update()
        {
            bool ret = false;
            if (resInfo == null)
            {
                return ret;
            }

            if (loaclFileresInfo == null || loaclFileresInfo.groups.Count != resInfo.groups.Count ||
                loaclFileresInfo.resources.Count != resInfo.resources.Count)
            {
                ret = true;
            }
            else
            {
                if (loaclFileresInfo == null || loaclFileresInfo.groups.Count == resInfo.groups.Count)
                {
                    foreach (ResGroupCfg da in loaclFileresInfo.groups)
                    {
                        bool flag = false;
                        foreach (ResGroupCfg da2 in resInfo.groups)
                        {
                            if (da.groupName == da2.groupName && !ListIsSame(da.keys,da2.keys))
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (flag)
                        {
                            ret = true;
                            break;
                        }
                    }

                    if (!ListIsSame(loaclFileresInfo.groups,resInfo.groups))
                    {
                        ret = true;
                    }
                    
                }

                if (loaclFileresInfo == null || loaclFileresInfo.resources.Count == resInfo.resources.Count)
                {
                    foreach (ResInfoData da in loaclFileresInfo.resources)
                    {
                        ResInfoData da2 = resInfo.GetResInfo(da.address);
                        if (da2 == null || da.IsDirty(da2))
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }

            if (!ret) return false;
            string jsonStr = JsonUtility.ToJson(resInfo, true);
            loaclFileresInfo = JsonUtility.FromJson<ResJsonData>(jsonStr);
            if (!System.IO.Directory.Exists(Path.GetDirectoryName(ResConfig.ResJsonCfgFilePath)))
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(ResConfig.ResJsonCfgFilePath));
            }

            File.WriteAllText(ResConfig.ResJsonCfgFilePath, jsonStr);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            return true;
        }
        
        

        

        public static List<string> GetAllBundleNames()
        {
            List<string> names = new List<string>();
            Dictionary<string,List<ResInfoData>> dic = new Dictionary<string, List<ResInfoData>>();
            foreach (var V in resInfo.resources)
            {
               if(!V.isResourcesPath && !names.Contains(V.ABName)) names.Add(V.ABName);
            }

            return names;
        }

        public static List<ResInfoData> getABAssets(string abName)
        {
            List<ResInfoData> list = new List<ResInfoData>();
            foreach (var V in resInfo.resources)
            {
                if (abName == V.ABName)
                {
                    list.Add(V);
                }
            }

            return list;
        }

        /// <summary>
        /// 是否能改名
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static bool HandleGroupRename(string groupName, string newName)
        {
            bool ret = resInfo != null && resInfo.groups.Exists(a => a.groupName == groupName)
                                       && !resInfo.groups.Exists(a => a.groupName == newName);
            if (ret)
            {
                foreach (ResGroupCfg sceneData in resInfo.groups)
                {
                    if (sceneData.groupName == groupName)
                    {
                        sceneData.groupName = newName;
                        break;
                    }
                }
                Update();
            }

            
            return ret;
        }

        public static bool HandleAssetRename(string assetName, string newName)
        {
            bool ret = resInfo != null && resInfo.resources.Exists(a => a.address == assetName)
                                       && !resInfo.resources.Exists(a => a.address == newName);
            if (ret)
            {
                var res = resInfo.GetResInfo(assetName);
                res.address = newName;
                //var groups = resInfo.groups.FindAll(a => a.keys.Contains(res.name));
                
                foreach (var v in resInfo.groups)
                {
                    for (var index = 0; index < v.keys.Count; index++)
                    {
                        if (v.keys[index] == assetName)
                        {
                            v.keys[index] = newName;
                        }
                    }
                }
                Update();
            }
            return ret;
        }

        /// <summary>
        /// 删除所有某个资源组
        /// </summary>
        /// <param name="b"></param>
        internal static void HandleGroupsDelete(List<string> groupNames)
        {
            List<string> delAssets = new List<string>();
            foreach (string groupName in groupNames)
            {
                ResGroupCfg load = null;
                foreach (ResGroupCfg da in resInfo.groups)
                {
                    if (da.groupName == groupName)
                    {
                        load = da;
                        break;
                    }
                }

                if (load != null)
                {
                    resInfo.groups.Remove(load);
                    delAssets.AddRange(load.keys);
                }
            }

            foreach (string assetName in delAssets)
            {
                if (!resInfo.groups.Exists(a=>a.keys.Contains(assetName)))
                {
                    resInfo.resources.Remove(resInfo.GetResInfo(assetName));
                }
            }

            Update();
        }

        /// <summary>
        /// 创建一个资源组
        /// </summary>
        internal static string HandleGroupCreate()
        {
            ResGroupCfg data = new ResGroupCfg();
            data.keys = new List<string>();
            data.groupName = GetGetUniqueName();
            resInfo.groups.Add(data);
            return data.groupName;
        }

        /// <summary>
        /// 拖了一个文件到资源里面
        /// </summary>
        /// <param name="paths"></param>
        internal static List<string> AddAssetToGroup(string[] paths, string groupName)
        {
            List<string> fileNames = new List<string>();
            ResGroupCfg data = resInfo.GetGroupInfo(groupName);
            if (data == null)
            {
                return fileNames;
            }

            foreach (string str in paths)
            {
                if (AssetDatabase.IsValidFolder(str))
                {
                    string[] files = Directory.GetFiles(str);
                    foreach (string fileName in files)
                    {
                        if (Path.GetExtension(fileName) == ".meta") continue;
                        string assetName = AddAssetToGroup(fileName, groupName);
                        if (!string.IsNullOrEmpty(assetName))
                        {
                            fileNames.Add(assetName);
                        }
                    }
                }
                else
                {
                    string assetName = AddAssetToGroup(str, groupName);
                    if (!string.IsNullOrEmpty(assetName))
                    {
                        fileNames.Add(assetName);
                    }
                }
            }

            return fileNames;
        }

        /// <summary>
        /// 添加文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="groupName"></param>
        public static string AddAssetToGroup(string path, string groupName)
        {
            path = path.Replace("\\", "/");
            var data = resInfo.GetGroupInfo(groupName);
            if (data == null)
            {
                return "";
            }

            if (!path.Contains("/Resources/") && !path.Contains(ResConfig.ExternalResDir))
            {
                Debug.LogWarning("不能能识别的路径 path："+path +"/"+ResConfig.ExternalResDir+"/"+ path.Contains(ResConfig.ExternalResDir));
                return "";
            }

            foreach (var v in resInfo.resources)
            {
                //Debug.LogWarning("不能重复添加");
                if (v.path == path)
                {
                    if(!data.keys.Contains(v.address)) data.keys.Add(v.address);
                    return v.address;
                }
            }
            var assetInfo = InitAssetInfo(groupName, path);
            resInfo.resources.Add(assetInfo);
            data.keys.Add(assetInfo.address);
            return assetInfo.address;
        }

        public static string CheckAssetName(string path)
        {
            int flag = 0;
            var resName = Path.GetFileNameWithoutExtension(path);
            var resExtension = Path.GetExtension(path).Replace(".", "_");
            
            
            while (true)
            {
                var uid = flag == 1 || flag == 0 ? "" : flag.ToString();
                var assetName = resName + uid + resExtension;
                if (resInfo.GetResInfo(assetName) == null) return assetName;
                flag++;
            }
        }

        public static ResInfoData InitAssetInfo(string groupName,string path)
        {
            string assetName = Path.GetFileNameWithoutExtension(path) +  Path.GetExtension(path).Replace(".","_");
            
            var da = new ResInfoData(path)
            {
                path = path,
                type = Path.GetExtension(path)
            };
            da.address = CheckAssetName(da.path);
            
            //url = string.Format("{0}:{1}", groupName, name);
            da.isResourcesPath = path.Contains("/Resources/");

            var rootPath = Path.GetDirectoryName(path);
            rootPath = rootPath.Replace("\\", "/");
            var name1 = Path.GetFileName(path);
            string abName = string.Empty;
            string url = string.Empty;
            if (rootPath != null)
            {
                if (da.isResourcesPath)
                {
                    int index = rootPath.LastIndexOf("/Resources", StringComparison.Ordinal);
                    if (rootPath.Length <= index + 11)
                    {
                        abName = string.Empty;
                        url = "r://" + name1;
                    }
                    else
                    {
                        abName = rootPath.Substring(index+11);
                        url = "r://" + abName + "/" + name1;
                    }
                }
                else
                {
                    var path1 = rootPath.Replace(ResConfig.ExternalResDir + "/", "");;
                    url = "e://" + path1+"/" + name1;
                    abName = path1.Replace("/", "_");
                    
                }
                //AssetBundle.LoadFromFileAsync()
            }

            da.ABName = abName;
            da.url = url;
            return da;
        }


        internal static void RemoveAssets(List<string> reAssets, string name)
        {
            ResGroupCfg data = resInfo.GetGroupInfo(name);
            if (data == null)
            {
                return;
            }

            foreach (string assetName in reAssets)
            {
                RemoveAsset(assetName, name);
            }

            Update();
        }

        /// <summary>
        /// 在某个资源组里面移出某个资源
        /// </summary>
        /// <param name="assetName">资源名称</param>
        /// <param name="name">资源组名称</param>
        public static void RemoveAsset(string assetName, string name)
        {
            ResGroupCfg data = resInfo.GetGroupInfo(name);
            if (data != null)
            {
                if (data.keys.Contains(assetName))
                {
                    data.keys.Remove(assetName);
                }
            }

            if (!resInfo.groups.Exists(a=>a.keys.Contains(assetName)))
            {
                var res = resInfo.GetResInfo(assetName);
                if (res != null)
                {
                    resInfo.resources.Remove(res);
                }
            }
        }

        /// <summary>
        /// 获取一个唯一的组名
        /// </summary>
        /// <returns></returns>
        public static string GetGetUniqueName()
        {
            string name = "NewGroupName";
            int index = 0;
            bool foundExisting = resInfo.HasGroup(name);
            while (foundExisting)
            {
                index++;
                foundExisting = resInfo.HasGroup(name + " " + index);
            }

            if (index > 0)
            {
                name = name + " " + index;
            }

            return name;
        }

        public static void Refresh()
        {
            if (loaclFileresInfo != null)
            {
                resInfo = JsonUtility.FromJson<ResJsonData>(JsonUtility.ToJson(loaclFileresInfo));
            }
        }
    }
}
