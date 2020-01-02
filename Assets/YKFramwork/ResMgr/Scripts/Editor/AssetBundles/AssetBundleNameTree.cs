
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace YKFramework.ResMgr.Editor
{
    public class AssetBundleNameTree:TreeView
    {
        private AssetsBundleAssetsTree mAssetsBundleAssetsTree;
        private TreeViewItem mRoot;
        protected override TreeViewItem BuildRoot()
        {
            mRoot = new TreeViewItem();
            mRoot.id = -1;
            mRoot.depth = -1;
            mRoot.children = new List<TreeViewItem>();
            int id = 0;
            foreach (string name in AssetMode.GetAllBundleNames())
            {
                id++;
                var t = new TreeViewItem(id, id, name);
                mRoot.AddChild(t);
            }

            return mRoot;
        }
        
        
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            
            return new MultiColumnHeaderState(GetColumns());
        }
        
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new[]
            {
                new MultiColumnHeaderState.Column()
            };
            retVal[0].headerContent = new GUIContent(Language.BundleName);
            retVal[0].minWidth = 0;
            retVal[0].width = 100;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Center;
            retVal[0].canSort = false;
            retVal[0].autoResize = true;
            retVal[0].allowToggleVisibility = false;
            
            return retVal;
        }
        
        /// <summary>
        /// 绘制行
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item, args.GetColumn(i), ref args);
        }
        
        private void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            if (column == 0)
            {
                DefaultGUI.Label(
                    new Rect(cellRect.x, cellRect.y, cellRect.width, cellRect.height),
                    item.displayName,
                    args.selected,
                    args.focused);
            }

            GUI.color = oldColor;
        }
        
        public void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0 &&
                rect.Contains(UnityEngine.Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        public AssetBundleNameTree(AssetsBundleAssetsTree assetsTree,TreeViewState state, MultiColumnHeaderState multiColumnHeader) 
            : base(state, new MultiColumnHeader(multiColumnHeader))
        {
            mAssetsBundleAssetsTree = assetsTree;
            showBorder = true;

            showAlternatingRowBackgrounds = false;
            //DefaultStyles.label.richText = true;
            Reload();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            List<string> abs = new List<string>();
            foreach (var id in selectedIds)
            {
                var xx = FindItem(id, rootItem);
                abs.Add(xx.displayName);
            }

            if (abs.Count == 1)
            {
                mAssetsBundleAssetsTree.SelectAB(abs[0]);
            }
            else
            {
                mAssetsBundleAssetsTree.SelectAB(string.Empty);
            }

            AssetMode.CurrentSelectsAbs = abs;
        }
    }
}