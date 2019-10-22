using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YKFramwork.ResMgr;

public class Example : MonoBehaviour
{
    private DefResLoadCfg cfg;
    // Start is called before the first frame update
    void Start()
    {
        cfg = ResMgr.Instance.cfg as DefResLoadCfg;
        cfg.Init(() =>
        {
            Debug.LogError("初始化配置成功");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private GameObject mCubePrefab;
    private GameObject mp3;
    private GameObject mCubeRed;
    private void OnGUI()
    {
        var stH = GUILayout.Height(50);
        var stW = GUILayout.Width(200);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("load test group",stH,stW))
        {
            TestLoadGroup();
        }

        if (GUILayout.Button("del",stH,stW))
        {
            if (mCubePrefab)
            {
                GameObject.Destroy(mCubePrefab);
                mCubePrefab = null;
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("get test Group Mp3 ",stH,stW))
        {
           TestGetRes();
        }
        
        if (GUILayout.Button("del",stH,stW))
        {
            if (mp3)
            {
                GameObject.Destroy(mp3);
                mp3 = null;
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("get async by url",stH,stW))
        {
           TestResAsyncByUrl();
        }
        if (GUILayout.Button("del",stH,stW))
        {
            if (mCubeRed)
            {
                GameObject.Destroy(mCubeRed);
                mCubeRed = null;
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("UnLoad",stH,stW))
        {
            TestUnLoad();
        }
        GUILayout.EndHorizontal();
        
    }


    public void TestLoadGroup()
    {
        var rep = ResMgr.Instance.LoadGroup("test", a =>
        {
            if(mCubePrefab == null)
                mCubePrefab = GameObject.Instantiate(ResMgr.Instance.GetRes("Cube_prefab")) as GameObject;
        },null);
    }

    public void TestGetRes()
    {
        if(mp3 != null) return;
        var rep = ResMgr.Instance.GetRes("AchievementUnlocked_mp3");
        if (rep == null)
        {
            Debug.LogError("请先加载资源再同步获取资源");
            return;
        }
        mp3 = new GameObject("mp3");
        var audioSource = mp3.AddComponent<AudioSource>();
        audioSource.clip = rep as AudioClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void TestResAsyncByUrl()
    {
        if(mCubeRed != null) return;
        ResMgr.Instance.GetResAsyncByUrl("e://New Folder 2/New Folder/Cube_Red.prefab", cubeRed =>
        {
            if (cubeRed == null)
            {
                Debug.LogError("e://New Folder 2/New Folder/Cube_Red.prefab");
            }
            else
            {
                mCubeRed = GameObject.Instantiate(cubeRed) as GameObject;
            }
        });
    }

    public void TestUnLoad()
    {
        ResMgr.Instance.UnloadUnused();
    }
}
