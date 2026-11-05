using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class SyncNetworkHand : NetworkBehaviour
{
    [SerializeField] private OVRHand hand;
    [SerializeField] private OVRCustomSkeleton skeleton;

    private NetworkVariable<HandData> networkHandRotations = new NetworkVariable<HandData>(
        new HandData(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (hand != null) hand.enabled = false;
            if (skeleton != null) skeleton.enabled = false;
        }
    }

    private void Update()
    {
        if (skeleton == null) return;
        if (skeleton.CustomBones == null || skeleton.CustomBones.Count == 0)
        {
            if (Time.frameCount % 100 == 0) Debug.LogWarning($"[{gameObject.name}] Skeleton not initialized or CustomBones empty.");
            return;
        }

        if (IsOwner)
        {
            UpdateHandDataToServer();
        }
        else
        {
            ApplyHandDataFromNetwork();
        }
    }

    private void UpdateHandDataToServer()
    {
        var bones = skeleton.CustomBones;
        HandData currentData = networkHandRotations.Value;

        // 配列の初期化（初回のみ）
        if (currentData.BoneRotations == null || currentData.BoneRotations.Length != bones.Count)
        {
            currentData.BoneRotations = new Quaternion[bones.Count];
            currentData.BonePositions = new Vector3[bones.Count]; // 【追加】
        }

        // ボーンの回転と位置をコピー
        for (int i = 0; i < bones.Count; i++)
        {
            if (bones[i] != null)
            {
                currentData.BoneRotations[i] = bones[i].localRotation;
                currentData.BonePositions[i] = bones[i].localPosition; // 【追加】
            }
        }

        networkHandRotations.Value = currentData;
        networkHandRotations.SetDirty(true);
    }

    private void ApplyHandDataFromNetwork()
    {
        var netRotations = networkHandRotations.Value.BoneRotations;
        var netPositions = networkHandRotations.Value.BonePositions; // 【追加】
        var bones = skeleton.CustomBones;

        if (netRotations == null || netRotations.Length == 0 || bones.Count != netRotations.Length) return;

        for (int i = 0; i < bones.Count; i++)
        {
            if (bones[i] != null)
            {
                bones[i].localRotation = netRotations[i];
                bones[i].localPosition = netPositions[i]; // 【追加】
            }
        }
    }
}