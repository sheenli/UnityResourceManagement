
using UnityEngine;
using UnityEngine.Networking;
using YKFramework.ResMgr;

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
                var ver = new YKFramework.ResMgr.VersionCtrl.Version(wepop.webRequest.downloadHandler.data);
                Debug.Log(ver.version);
                for (var index = 0; index < ver.AllAB.Count; index++)
                {
                    var abInfo = ver.AllAB[index];
                    Debug.Log("name:" + abInfo.name + "/sha1:" + abInfo.sha1 + "/size:" + abInfo.size);
                }
            }
        };
    }
}