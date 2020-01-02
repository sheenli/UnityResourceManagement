using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Serialization;

namespace YKFramework.ResMgr.Editor
{
    public class ResFileEditor : EditorWindow
    {
        
        [HideInInspector] [SerializeField] public AssetGroupMgr m_ManageTab;
        [FormerlySerializedAs("mAaaetBundleEditor")] [HideInInspector] public AssetBundleEditor mAssetBundleEditor;
        [MenuItem("Window/ResMgr", priority = 2050)]
        static void ShowWindow()
        {
            var window = GetWindow<ResFileEditor>();
            window.titleContent = new GUIContent(Language.ResourceMgr);
            
            window.Show();
        }

        string[] m_ButtonStr = new string[2] {Language.AssetGroupEdit, Language.BuildAssetToBundles};

        protected void OnEnable()
        {
            Rect subPos = GetSubWindowArea();
            if (m_ManageTab == null) m_ManageTab = new AssetGroupMgr();
            if(mAssetBundleEditor == null) mAssetBundleEditor = new AssetBundleEditor();
            
            m_ManageTab.OnEnable(subPos, this);
            mAssetBundleEditor.OnEnable(subPos, this);
        }

        public enum SelectedPage
        {
            ResGroup,
            BuildAssetbundle
        }

        [HideInInspector] public SelectedPage m_SelectedPage;

        private SelectedPage old = SelectedPage.ResGroup;
        protected void OnGUI()
        {
            TreeView.DefaultStyles.label.richText = true;
            //DefaultStyles.label.richText = true;
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                m_SelectedPage = (SelectedPage) GUILayout.Toolbar((int) m_SelectedPage, new string[] {Language.ResourceMgr, Language.BuildAssetToBundles},
                    GUILayout.Height(40));
            }
            if (old != m_SelectedPage)
            {
                old = m_SelectedPage;
                OnEnable();
            }
            GUILayout.EndHorizontal();
            if (m_SelectedPage == SelectedPage.ResGroup)
            {
                m_ManageTab.OnGUI(GetSubWindowArea());
            }
            else
            {
                mAssetBundleEditor.OnGUI(GetSubWindowArea());
                //base.OnGUI();
            }

        }

        private static float m_UpdateDelay = 0f;

        private void Update()
        {
            if (Time.realtimeSinceStartup - m_UpdateDelay > 0.1f)
            {
                m_UpdateDelay = Time.realtimeSinceStartup;

                if (AssetMode.Update())
                {
                    Repaint();
                }
            }
        }

        /// <summary>
        /// 获取当前窗口区域
        /// </summary>
        /// <returns></returns>
        private Rect GetSubWindowArea()
        {
            Rect subPos = new Rect(0, 50, position.width, position.height - 10);
            return subPos;
        }

    }
}
