using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YKFramework.ResMgr.Editor;

namespace YKFramework.ResMgr.Editor
{

    public class AssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {

        public static bool CheckIsResPath(string oldPath)
        {
            
            if (!oldPath.Contains("/Resources/") && !oldPath.Contains(ResConfig.ExternalResDir))
            {
                return false;
            }

            var isFolder = Directory.Exists(oldPath);
            var isSaveRes = false;
            foreach (var resInfo in AssetMode.resInfo.resources)
            {
                var resDir = Path.GetDirectoryName(resInfo.path).Replace("\\", "/");
                if (isFolder)
                {
                    if (resDir != oldPath) continue;
                    isSaveRes = true;
                    break;
                }
                else
                {
                    if (resInfo.path != oldPath) continue;
                    isSaveRes = true;
                    break;
                }
            }

            return isSaveRes;
        }

        public static AssetMoveResult OnWillMoveAsset(string oldPath, string newPath)
        {
            AssetMoveResult result = AssetMoveResult.DidNotMove;
            if (!oldPath.Contains("/Resources/") && !oldPath.Contains(ResConfig.ExternalResDir))
            {
                result = AssetMoveResult.DidNotMove;
                return result;
            }

            AssetMode.Rebuild();
            var isSaveRes = CheckIsResPath(oldPath);
            var isFolder = Directory.Exists(oldPath);
            if (isSaveRes)
            {
                if (!newPath.Contains("/Resources/") && !newPath.Contains(ResConfig.ExternalResDir))
                {
                    result = AssetMoveResult.FailedMove;
                }

                if (!isFolder && newPath.Contains(ResConfig.ExternalResDir) &&
                    Path.GetDirectoryName(newPath).Replace("\\", "/") == ResConfig.ExternalResDir)
                {
                    result = AssetMoveResult.FailedMove;
                }

                if (result == AssetMoveResult.FailedMove)
                {
                    UnityEditor.EditorUtility.DisplayDialog("tips", "移动路径不符合规范", "确定");
                }
                else
                {
                    if (UnityEditor.EditorUtility.DisplayDialog("tips", "此资源已经被添加至了资源编辑器，改变后会自动修改\n 确定要修改吗？", "确定"))
                    {
                        if (isFolder) MoveFolder(oldPath, newPath);
                        else
                        {
                            ChangeAssetInfo(oldPath, newPath);
                        }
                    }
                    else
                    {
                        result = AssetMoveResult.FailedMove;
                    }
                }
            }
            else
            {
                result = AssetMoveResult.DidNotMove;
            }

            return result;
        }

        public static void MoveFolder(string old, string newPath)
        {
            var dirs = Directory.GetDirectories(old);
            var files = Directory.GetFiles(old);
            foreach (var dir in dirs)
            {
                var fileName = Path.GetFileName(dir).Replace("\\", "/");
                MoveFolder(dir.Replace("\\", "/"), newPath + "/" + fileName);
            }

            foreach (var filePath in files)
            {
                var path = filePath.Replace("\\", "/");
                if (AssetMode.resInfo.resources.Exists(a => a.path == path))
                {
                    Debug.Log("移动文件" + filePath + "到" + newPath + "/" + Path.GetFileName(path));
                    ChangeAssetInfo(path, newPath + "/" + Path.GetFileName(path));
                }
            }
        }

        private static void ChangeAssetInfo(string oldPath, string newPath)
        {
            var resInfo = AssetMode.resInfo.resources.Find(a => a.path == oldPath);
            resInfo.path = newPath;
            var rootPath = Path.GetDirectoryName(newPath);
            rootPath = rootPath.Replace("\\", "/");
            var name1 = Path.GetFileName(newPath);
            string abName = string.Empty;
            string url = string.Empty;
            resInfo.isResourcesPath = newPath.Contains("/Resources/");
            if (rootPath != null)
            {
                if (resInfo.isResourcesPath)
                {
                    int index = rootPath.LastIndexOf("/Resources", StringComparison.Ordinal);
                    if (rootPath.Length <= index + 11)
                    {
                        abName = string.Empty;
                        url = "r://" + name1;
                    }
                    else
                    {
                        abName = rootPath.Substring(index + 11);
                        url = "r://" + abName + "/" + name1;
                    }
                }
                else
                {
                    var path1 = rootPath.Replace(ResConfig.ExternalResDir + "/", "");
                    ;
                    url = "e://" + path1 + "/" + name1;
                    abName = path1.Replace("/", "_");

                }
            }

            resInfo.path = newPath;
            resInfo.ABName = abName;
            resInfo.url = url;
            AssetMode.Update();
        }

        private static void DelAssset(string path)
        {
            var resInfoData = AssetMode.resInfo.resources.Find(a => a.path == path);
            if (resInfoData == null) Debug.LogError("不能删除不存在的资源" + path);
            else
            {
                var group = AssetMode.resInfo.groups.Find(a => a.keys.Contains(resInfoData.address));
                group.keys.Remove(resInfoData.address);
                AssetMode.resInfo.resources.Remove(resInfoData);
                AssetMode.Update();
            }
        }

        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            AssetMode.Rebuild();
            AssetDeleteResult r = AssetDeleteResult.DidNotDelete;
            var isFolder = Directory.Exists(assetPath);
            var isres = CheckIsResPath(assetPath);
            if (isres)
            {
                if (UnityEditor.EditorUtility.DisplayDialog("tips", "此资源已经被添加至了资源编辑器，删除后将会从管理器中移除\n 确定要删除吗？", "确定"))
                {
                    DelAssset(assetPath);
                }
                else
                {
                    r = AssetDeleteResult.FailedDelete;
                }
            }

            return r;
        }
    }
}