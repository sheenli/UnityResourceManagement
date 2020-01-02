using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YKFramework.ResMgr.Editor;

namespace YKFramework.ResMgr.Editor
{
    public class AssetsBundleAssetsTree:TreeView
    {
        public AssetsBundleAssetsTree(TreeViewState state) : base(state)
        {
        }

        public AssetsBundleAssetsTree(TreeViewState state, MultiColumnHeaderState multiColumnHeader) :
            base(state, new MultiColumnHeader(multiColumnHeader))
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            
            Reload();
        }
        public List<AssetMode.AssetInfo> assets = new List<AssetMode.AssetInfo>();
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem();
            root.children = new System.Collections.Generic.List<UnityEditor.IMGUI.Controls.TreeViewItem>();
            root.id = -1;
            root.depth = -1;
            foreach (AssetMode.AssetInfo asset in assets)
            {
                root.children.Add(new ResAssetsTreeEitor.AssetTreeViewItem(asset));
            }
            return root;
        }
        
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
        
        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0 &&
                rect.Contains(UnityEngine.Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }
        
        
        protected override void RowGUI(RowGUIArgs args)
        {
           
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as ResAssetsTreeEitor.AssetTreeViewItem, args.GetColumn(i), ref args);
        }
        
        private void CellGUI(Rect cellRect, ResAssetsTreeEitor.AssetTreeViewItem item, int column, ref RowGUIArgs args)
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
            var assetItem = FindItem(id, rootItem) as ResAssetsTreeEitor.AssetTreeViewItem;
            if (assetItem != null)
            {
               

                UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetItem.asset.data.path);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        public void SelectAB(string abName)
        {
            this.assets.Clear();
            if (string.IsNullOrEmpty(abName))
            {
                Reload();
                return;
            }
            var assets = AssetMode.getABAssets(abName);
            int id = 0;
            foreach (var a in assets)
            {
                id++;
                AssetMode.AssetInfo assetInfo = new AssetMode.AssetInfo(id,a.address);
                //assetInfo.data = a;
                this.assets.Add(assetInfo);
            }
            Reload();
        }
    }
}