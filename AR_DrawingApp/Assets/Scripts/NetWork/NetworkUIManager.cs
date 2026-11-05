using UnityEngine;
using Unity.Netcode;

public class NetworkUIManager : MonoBehaviour
{
    // コンテキストメニューを追加。インスペクターから呼び出せるようになります
    [ContextMenu("ホストとして起動 (Start Host)")]
    public void OnClickStartHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("ホストとして起動しました。");
        }
        else
        {
            Debug.LogError("シーンにNetworkManagerが存在しません！");
        }
    }

    [ContextMenu("クライアントとして接続 (Start Client)")]
    public void OnClickStartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("クライアントとして接続しました。");
        }
        else
        {
            Debug.LogError("シーンにNetworkManagerが存在しません！");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            OnClickStartHost();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            OnClickStartClient();
        }

    }
}