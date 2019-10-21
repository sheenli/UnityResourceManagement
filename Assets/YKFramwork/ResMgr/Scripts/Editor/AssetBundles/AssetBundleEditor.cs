
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YKFramwork.ResMgr.Editor;

namespace YKFramwork.ResMgr.Editor
{
    public class AssetBundleEditor
    {
        /// <summary>
        /// 资源多列表标签
        /// </summary>
        [SerializeField] MultiColumnHeaderState m_GroupTreeMCHState;
        
        /// <summary>
        /// 调整了水平方向的分割方式
        /// </summary>
        bool m_ResizingHorizontalSplitter = false;

        /// <summary>
        /// 调整了垂直方向上
        /// </summary>
        bool m_ResizingVerticalSplitter = false;
        /// <summary>
        /// 水平方向和垂直方向上的分割矩形
        /// </summary>
        Rect m_HorizontalSplitterRect, m_VerticalSplitterRect;

        /// <summary>
        /// 水平方向占比
        /// </summary>
        [SerializeField] float m_HorizontalSplitterPercent;

        /// <summary>
        /// 垂直方向占比
        /// </summary>
        [SerializeField] float m_VerticalSplitterPercent;

        /// <summary>
        /// 当前窗口位置
        /// </summary>
        public Rect m_Position;
        const float k_SplitterWidth = 3f;
        
        /// <summary>
        /// 父窗体
        /// </summary>
        EditorWindow m_Parent = null;

        private AssetBundleNameTree mAssetBundleNameTree;

        private AssetsBundleAssetsTree mAssetsBundleAssetsTree;

        public AssetBuildView mAssetBuildView;
        
        /// <summary>
        /// 资源组树状图列表
        /// </summary>
        [SerializeField] TreeViewState m_GroupTreeState;

        private TreeViewState m_AssetListState;
        private MultiColumnHeaderState m_AssetListMCHState;

        public AssetBundleEditor()
        {
            m_HorizontalSplitterPercent = 0.3f;
            m_VerticalSplitterPercent = 0.7f;
        }
        
        
        public void OnEnable(Rect pos, EditorWindow parent)
        {
            m_Parent = parent;
            m_Position = pos;
            m_HorizontalSplitterRect = new Rect(
                (int) (m_Position.x + m_Position.width * m_HorizontalSplitterPercent),
                m_Position.y,
                k_SplitterWidth,
                m_Position.height);

            m_VerticalSplitterRect = new Rect(
                m_HorizontalSplitterRect.x,
                (int) (m_Position.y + m_HorizontalSplitterRect.height * m_VerticalSplitterPercent),
                (m_Position.width - m_HorizontalSplitterRect.width) - k_SplitterWidth,
                k_SplitterWidth);
            if (mAssetsBundleAssetsTree == null)
            {
                m_AssetListState = new TreeViewState();
                var assetHeaderState = AssetsBundleAssetsTree.CreateDefaultMultiColumnHeaderState();
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_AssetListMCHState, assetHeaderState))
                {
                    MultiColumnHeaderState.OverwriteSerializedFields(m_AssetListMCHState, assetHeaderState);
                }

                m_AssetListMCHState = assetHeaderState;
                mAssetsBundleAssetsTree = new AssetsBundleAssetsTree(m_AssetListState,m_AssetListMCHState);
            }
            if (mAssetBundleNameTree == null)
            {
                m_GroupTreeState = new TreeViewState();
               
                var headerState = AssetBundleNameTree.CreateDefaultMultiColumnHeaderState(); // 
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_GroupTreeMCHState, headerState))
                {
                    MultiColumnHeaderState.OverwriteSerializedFields(m_GroupTreeMCHState, headerState);
                }
                m_GroupTreeMCHState = headerState;
                mAssetBundleNameTree = new AssetBundleNameTree(mAssetsBundleAssetsTree,m_GroupTreeState,m_GroupTreeMCHState);
            } 

           

            if (mAssetBuildView == null)
            {
                mAssetBuildView = new AssetBuildView();
            }
            AssetMode.Rebuild();
            mAssetBuildView.Reload();
            mAssetBundleNameTree.Reload();
            mAssetsBundleAssetsTree.Reload();
            
        }
        
        
         private void HandleHorizontalResize()
        {
            m_HorizontalSplitterRect.x = m_Position.width * m_HorizontalSplitterPercent;
            m_HorizontalSplitterRect.height = m_Position.height;

            EditorGUIUtility.AddCursorRect(m_HorizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown &&
                m_HorizontalSplitterRect.Contains(Event.current.mousePosition))
                m_ResizingHorizontalSplitter = true;

            if (m_ResizingHorizontalSplitter)
            {
                m_HorizontalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.x / m_Position.width, 0.1f, 0.9f);
                m_HorizontalSplitterRect.x = (int) (m_Position.width * m_HorizontalSplitterPercent);
            }

            if (Event.current.type == EventType.MouseUp)
                m_ResizingHorizontalSplitter = false;
        }

        private void HandleVerticalResize()
        {
            m_VerticalSplitterRect = new Rect(
                m_HorizontalSplitterRect.x,
                (int) (m_Position.y + m_HorizontalSplitterRect.height * m_VerticalSplitterPercent),
                (m_Position.width - m_HorizontalSplitterRect.width) - k_SplitterWidth,
                k_SplitterWidth);

            m_VerticalSplitterRect.x = m_HorizontalSplitterRect.x;
            m_VerticalSplitterRect.y = m_HorizontalSplitterRect.height * m_VerticalSplitterPercent;

            m_VerticalSplitterRect.width = m_Position.width - m_HorizontalSplitterRect.x;



            EditorGUIUtility.AddCursorRect(m_VerticalSplitterRect, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown &&
                m_VerticalSplitterRect.Contains(Event.current.mousePosition))
                m_ResizingVerticalSplitter = true;

            if (m_ResizingVerticalSplitter)
            {
                m_VerticalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.y / m_HorizontalSplitterRect.height,
                    0.4f, 0.7f);
                m_VerticalSplitterRect.y = (int) (m_HorizontalSplitterRect.height * m_VerticalSplitterPercent);
            }


            if (Event.current.type == EventType.MouseUp)
            {
                m_ResizingVerticalSplitter = false;
            }
        }

        public  void OnGUI(Rect pos)
        {
            TreeView.DefaultStyles.label.richText = true;
            if(m_GroupTreeMCHState == null) return;
            m_Position = pos;
            m_Parent.Repaint();
            HandleHorizontalResize();
            HandleVerticalResize();
            var groupTreeRect = new Rect(
                m_Position.x + k_SplitterWidth,
                m_Position.y,
                m_HorizontalSplitterRect.x,
                m_HorizontalSplitterRect.height - m_Position.y);
            m_GroupTreeMCHState.columns[0].width = groupTreeRect.width;
            
            
            mAssetBundleNameTree.OnGUI(groupTreeRect);
            
            var assetTreeRect = new Rect(
                m_HorizontalSplitterRect.x + k_SplitterWidth * 3, m_HorizontalSplitterRect.y,
                m_VerticalSplitterRect.width - k_SplitterWidth * 5,
                m_VerticalSplitterRect.y - m_HorizontalSplitterRect.y
            );
            mAssetsBundleAssetsTree.OnGUI(assetTreeRect);
            var assetInfoRect = new Rect(
                assetTreeRect.x, m_VerticalSplitterRect.y+ k_SplitterWidth * 3,
                m_VerticalSplitterRect.width - k_SplitterWidth * 5, m_HorizontalSplitterRect.height - m_VerticalSplitterRect.y - k_SplitterWidth * 3
            );
            mAssetBuildView.OnGUI(assetInfoRect);
            if (m_ResizingHorizontalSplitter || m_ResizingVerticalSplitter)
            {
                m_Parent.Repaint();
            }
            
//            var assetInfoRect = new Rect(
//                assetTreeRect.x, assetTreeRect.width,
//                assetTreeRect.width - 1, assetTreeRect.y - m_HorizontalSplitterRect.y
//            );
           
            //m_ResGroupTree.OnGUI(groupTreeRect);
//        var assets = AssetDatabase.GetAllAssetPaths()
//              .Where(x =>
//              {
//                  return x.StartsWith("Assets/YKFramwork", StringComparison.InvariantCultureIgnoreCase);
//              });
//
//        List<BuildCollectionResInfo> list = new List<BuildCollectionResInfo>();
//        List<string> dis = new List<string>();
//        foreach (var assetPath in assets)
//        {
//            BuildCollectionResInfo info = AssetDatabase.LoadAssetAtPath(assetPath, typeof(BuildCollectionResInfo)) as BuildCollectionResInfo;
//            if (info != null)
//            {
//                list.Add(info);
//                dis.Add(Path.GetFileNameWithoutExtension( assetPath));
//            }
//        }
//        EditorGUILayout.BeginHorizontal();
//        {
//           
//            GUILayout.Space(5);
//            GUIStyle st = new GUIStyle("LargePopup");
//            //st.CalcScreenSize(new Vector2(20,200));
//            st.margin = new RectOffset(0, 0, 0, 20);
////             st.fixedHeight = 20;
////             st.fixedWidth = 200;
//            st.alignment = TextAnchor.MiddleCenter;
//
//            GUIStyle la = new GUIStyle("Label");
//            la.margin = new RectOffset(0, 0, 0, 20);
//            la.fontSize = 20;
//            la.alignment = TextAnchor.MiddleLeft;
//            GUILayout.Label(new GUIContent("准备生成的配置："), la);
//            selectBuilds = EditorGUILayout.Popup(selectBuilds,
//           dis.ToArray(), st);
//            //GUI.color = Color.green;
//            if (GUILayout.Button("Add", st))
//            {
//                string path = EditorUtility.SaveFilePanel("保存文件位置", "Assets/YKFramwork/Editor/BuildAbInfo/Collections", "","asset").Replace("\\", "/");
//                path = FileUtil.GetProjectRelativePath(path);
//                if (!string.IsNullOrEmpty(path))
//                {
//                    BuildCollectionResInfo data = ScriptableObject.CreateInstance<BuildCollectionResInfo>();
//                    Debug.LogError(path);
//                    AssetDatabase.CreateAsset(data, path);
//                    AssetDatabase.SaveAssets();
//                    AssetDatabase.Refresh();
//                }
//            }
//            GUILayout.FlexibleSpace();
//        }
//        EditorGUILayout.EndHorizontal();
//        if (list.Count > selectBuilds)
//        {
//           
//            BuildCollectionResInfo selectinfo = list[selectBuilds];
//            if (show == null)
//            {
//                show = Editor.CreateEditor(selectinfo);
//            }
//            else
//            {
//                if(show.target != selectinfo)
//                {
//                    show = Editor.CreateEditor(selectinfo);
//                }
//            }
//        }
//            show.OnInspectorGUI();


        }
    }
}
