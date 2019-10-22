using System;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using YKFramwork.ResMgr;

public class ExampleVersion:MonoBehaviour
{
    private void Start()
    {
        var getUrl = UnityWebRequest.Get(ResMgr.Instance.cfg.RootABPath + "/version");
        getUrl.SendWebRequest().completed += op =>
        {
            UnityWebRequestAsyncOperation wepop = op as UnityWebRequestAsyncOperation;
            if (string.IsNullOrEmpty(wepop.webRequest.error))
            {
                var ver = new YKFramwork.ResMgr.VersionCtrl.Version(wepop.webRequest.downloadHandler.data);
                Debug.Log(ver.version);
                foreach (var abInfo in ver.AllAB)
                {
                    Debug.Log("name:"+abInfo.name+"/sha1:"+abInfo.sha1+"/size:"+abInfo.size);
                }
            }
        };
    }
}