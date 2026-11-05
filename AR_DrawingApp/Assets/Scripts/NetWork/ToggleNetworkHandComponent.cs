using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ToggleNetworkHandComponent : NetworkBehaviour
{
    [Header("自分(Owner)の時だけ有効にするもの")]
    [SerializeField] private MonoBehaviour[] ownerOnlyComponents;

    [Header("他人(Remote)の時だけ有効にするもの")]
    [SerializeField] private MonoBehaviour[] remoteOnlyComponents;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // 自分の場合：Remote用をオフ、Owner用をオン
            ToggleComponents(ownerOnlyComponents, true);
            ToggleComponents(remoteOnlyComponents, false);
        }
        else
        {
            // 他人の場合：Remote用をオン、Owner用をオフ
            ToggleComponents(ownerOnlyComponents, false);
            ToggleComponents(remoteOnlyComponents, true);
        }
    }

    private void ToggleComponents(MonoBehaviour[] components, bool state)
    {
        foreach (var c in components)
        {
            if (c != null) c.enabled = state;
        }
    }
}