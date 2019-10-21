using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace YKFramwork.ResMgr.Editor
{
    public class ResAssetsTreeEitor : TreeView
    {
       

        /// <summary>
        /// 当前所选资源组名称
        /// </summary>
        public string groupName = null;

        public List<AssetMode.AssetInfo> assets = new List<AssetMode.AssetInfo>();


        public AssetGroupMgr mController;
        private bool mContextOnItem = false;

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };
            retVal[0].headerContent = new GUIContent(Language.FileName);
            retVal[0].minWidth = 100;
            retVal[0].width = 100;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Center;
            retVal[0].canSort = false;
            retVal[0].autoResize = true;
            retVal[0].allowToggleVisibility = false;

            retVal[1].headerContent = new GUIContent(Language.FileSize);
            retVal[1].minWidth = 50;
            retVal[1].width = 75;
            retVal[1].maxWidth = 75;
            retVal[1].headerTextAlignment = TextAlignment.Center;
            retVal[1].canSort = false;
            retVal[1].autoResize = true;
            retVal[1].allowToggleVisibility = true;

            retVal[2].headerContent = new GUIContent(Language.Path);
            retVal[2].minWidth = 100;
            retVal[2].width = 400;
            retVal[2].maxWidth = 1000;
            retVal[2].headerTextAlignment = TextAlignment.Center;
            retVal[2].canSort = false;
            retVal[2].autoResize = true;
            retVal[2].allowToggleVisibility = true;

            return retVal;
        }

        enum MyColumns
        {
            Asset,
            Size,
            Path
        }

        public ResAssetsTreeEitor(TreeViewState state, MultiColumnHeaderState mchs, AssetGroupMgr ctrl)
            : base(state, new MultiColumnHeader(mchs))
        {
            mController = ctrl;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            
            Reload();
        }

        public void Update()
        {
            if (AssetMode.Update())
            {
                Reload();
            }

        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            
            return true;
        }

        public class AssetTreeViewItem : TreeViewItem
        {
            private AssetMode.AssetInfo m_asset;

            public AssetMode.AssetInfo asset
            {
                get { return m_asset; }
            }

            public AssetTreeViewItem(AssetMode.AssetInfo info) : base(info.NameHashCode, info.NameHashCode, info.Name)
            {
                m_asset = info;
                icon = AssetDatabase.GetCachedIcon(info.data.path) as Texture2D;
            }
            
        }


        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem();
            root.children = new System.Collections.Generic.List<UnityEditor.IMGUI.Controls.TreeViewItem>();
            root.id = -1;
            root.depth = -1;
            foreach (AssetMode.AssetInfo asset in assets)
            {
                root.children.Add(new AssetTreeViewItem(asset));
            }
            return root;
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }
        
        
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as AssetTreeViewItem, args.GetColumn(i), ref args);
        }
        
        private void CellGUI(Rect cellRect, AssetTreeViewItem item, int column, ref RowGUIArgs args)
        {
            
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            //if (column != 3)
            //    GUI.color = item.itemColor;
            if (!File.Exists(item.asset.data.path))
            {
                GUI.color = Color.red;
            }

            switch ((MyColumns) column)
            {
                case MyColumns.Asset:
                {
                    var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    if (item.icon != null)
                    {
                        GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                    }

                    var nameRect = new Rect(cellRect.x + iconRect.xMax + 1
                        , cellRect.y, cellRect.width - iconRect.width, cellRect.height);
                    
                    DefaultGUI.Label(nameRect,
                        item.displayName,
                        args.selected,
                        args.focused);
                }
                    break;
                case MyColumns.Size:
                    DefaultGUI.Label(cellRect, item.asset.GetSizeString(), args.selected, args.focused);
                    break;
                case MyColumns.Path:
                    DefaultGUI.Label(cellRect, item.asset.data.path, args.selected, args.focused);
                    break;

            }

            GUI.color = oldColor;
        }

        protected override void DoubleClickedItem(int id)
        {
            var assetItem = FindItem(id, rootItem) as AssetTreeViewItem;
            if (assetItem != null)
            {
               

                UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetItem.asset.data.path);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        /// <summary>
        /// 有东西被拖动到了这
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (IsValidDragDrop(args))
            {
                if (args.performDrop)
                {
                    List<string> name = AssetMode.AddAssetToGroup(DragAndDrop.paths, groupName);
                    foreach (string path in DragAndDrop.paths)
                    {
                        Debug.Log("SetupDragAndDrop 2" + path);
                    }

                    AddAssetToGroup(name);
                }

                return DragAndDropVisualMode.Copy; //Move;
            }


            return DragAndDropVisualMode.Rejected;
        }

        /// <summary>
        /// 这个文件是否能拖过来
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected bool IsValidDragDrop(DragAndDropArgs args)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return false;
            }

            if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
                return false;

            //从场景拖过来的不行//
            foreach (var assetPath in DragAndDrop.paths)
            {
                string path = FileUtil.GetProjectRelativePath(assetPath);
                string rootPath = Path.GetDirectoryName(assetPath)?.Replace("\\","/");
                if (!assetPath.StartsWith(FileUtil.GetProjectRelativePath(assetPath)) &&
                    !assetPath.Contains("/Resources/") &&
                    !assetPath.Contains(ResConfig.ExternalResDir))
                {
                    return false;
                }

                if (rootPath == ResConfig.ExternalResDir) return false;
                Type t = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

                if (!ResConfig.CanDropType.Contains(t) && !AssetDatabase.IsValidFolder(assetPath))
                {
                    return false;
                }
            }

            return true;
        }

        private int currentSelect = -1;
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            List<AssetMode.AssetInfo> list = new List<AssetMode.AssetInfo>();
            foreach (int nodeId in selectedIds)
            {
                AssetTreeViewItem item = FindItem(nodeId, rootItem) as AssetTreeViewItem;
                if (item != null)
                {
                    list.Add(item.asset);
                }
            }

            if (list.Count == 1)
            {
                UpdateSelectedAssets(list);
                
            }
            else
            {
                UpdateSelectedAssets(new List<AssetMode.AssetInfo>());
            }
        }
        
        internal void UpdateSelectedAssets(List<AssetMode.AssetInfo> list)
        {
            if (list == null || list.Count == 0)
            {
                currentSelect = -1;
            }
            
            if (mController.mAssetInfoEditor != null)
            {
                mController.mAssetInfoEditor.SelectedAssets(list.Count == 1 ? list[0] : null);
            }
        }

        protected override void ContextClickedItem(int id)
        {
            List<AssetTreeViewItem> selectedNodes = new List<AssetTreeViewItem>();
            foreach (var nodeID in GetSelection())
            {
                selectedNodes.Add(FindItem(nodeID, rootItem) as AssetTreeViewItem);
            }

            if (selectedNodes.Count > 0)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent(Language.RemoveSelectAssets), false, RemoveAssets, selectedNodes);
                menu.ShowAsContext();
            }
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
            if (currentSelect == id)
            {
                BeginRename(FindItem(id, rootItem));
            }
            else
            {
                currentSelect = id;
            }
        }

        /// <summary>
        /// 更改名称完成
        /// </summary>
        /// <param name="args"></param>
        protected override void RenameEnded(RenameEndedArgs args)
        {
            var item = FindItem(args.itemID, rootItem);
            currentSelect = -1;
            if (!string.IsNullOrEmpty(args.newName) && args.newName != args.originalName)
            {
                args.acceptedRename = AssetMode.HandleAssetRename(args.originalName, args.newName);
                ReloadAndSelect(new List<int>(){args.itemID});
            }
            else
            {
                args.acceptedRename = false;
            }
           
            
            //base.RenameEnded(args);
//
//            if (args.newName != null && args.newName.Length > 0 && args.newName != args.originalName)
//            {
//                //args.newName = args.newName.ToLower();
//                args.acceptedRename = true;
//
//                args.acceptedRename = AssetMode.HandleGroupRename(args.originalName, args.newName);
//                if (args.acceptedRename)
//                {
//                    ReloadAndSelect(args.itemID, false);
//                }
//            }
//            else
//            {
//                args.acceptedRename = false;
//            }
        }
        

        protected override bool CanRename(TreeViewItem item)
        {
            //item.id == GetColumns[1]
            return false ;base.CanRename(item); 
        }
        
        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            return rowRect;
        }

        void RemoveAssets(object obj)
        {
            List<AssetTreeViewItem> list = obj as List<AssetTreeViewItem>;
            List<string> reAssets = new List<string>();
            foreach (AssetTreeViewItem item in list)
            {
                reAssets.Add(item.asset.data.address);
            }

            AssetMode.RemoveAssets(reAssets, groupName);
            RelodGroup(groupName);
        }

        protected override void KeyEvent()
        {
            if (!string.IsNullOrEmpty(groupName) && Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                List<AssetTreeViewItem> selectedNodes = new List<AssetTreeViewItem>();
                foreach (var nodeID in GetSelection())
                {
                    selectedNodes.Add(FindItem(nodeID, rootItem) as AssetTreeViewItem);
                }

                RemoveAssets(selectedNodes);
            }
        }

        private void ReloadAndSelect(IList<int> hashCodes)
        {
            //SetSelectedGroups(group.Name);
            Reload();
            SetSelection(hashCodes);
            SelectionChanged(hashCodes);
        }

        public void RelodGroup(string groupName)
        {
            assets.Clear();
            this.groupName = groupName;
            AddAsset(AssetMode.resInfo.GetAssetsNames(groupName));
            ReloadAndSelect(new List<int>());
            UpdateSelectedAssets(new List<AssetMode.AssetInfo>());
            currentSelect = -1;
        }

        public void AddAsset(string[] datas)
        {
            int id = 0;
            foreach (string str in datas)
            {
                AssetMode.AssetInfo info = new AssetMode.AssetInfo(id++, str);
                assets.Add(info);
            }
        }
        private void AddAssetToGroup(IList<string> names)
        {
            assets.Clear();
            if (!string.IsNullOrEmpty(groupName))
            {
                
                string[] datas = AssetMode.resInfo.GetAssetsNames(groupName);
                AddAsset(datas);
            }
            else
            {
                return;
            }
            List<int> assetNames = new List<int>();
            List<AssetMode.AssetInfo> infos = new List<AssetMode.AssetInfo>();
            foreach (string asName in names)
            {
                AssetMode.AssetInfo info = assets.Find(a => a.Name == asName);
                assetNames.Add(info.NameHashCode);
                infos.Add(info);
            }
            
            ReloadAndSelect(assetNames);
            UpdateSelectedAssets(infos);
        }
    }
}