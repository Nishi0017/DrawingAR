using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SyncVRRig : NetworkBehaviour
{
    [Header("PlayerPrefab 内の同期ターゲット")]
    [SerializeField] private Transform headTarget;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [Header("位置・回転のオフセット設定")]
    [Tooltip("頭の位置調整 (x, y, z)")]
    [SerializeField] private Vector3 headPosOffset;
    [Tooltip("頭の角度調整 (x, y, z)")]
    [SerializeField] private Vector3 headRotOffset;

    [Space(10)]
    [Tooltip("左手の位置調整")]
    [SerializeField] private Vector3 leftHandPosOffset;
    [Tooltip("左手の角度調整")]
    [SerializeField] private Vector3 leftHandRotOffset;

    [Space(10)]
    [Tooltip("右手の位置調整")]
    [SerializeField] private Vector3 rightHandPosOffset;
    [Tooltip("右手の角度調整")]
    [SerializeField] private Vector3 rightHandRotOffset;


    // ハードウェア側のリグ（OVRCameraRig）
    private Transform cameraRigCenterEye;
    private Transform cameraRigLeftHand;
    private Transform cameraRigRightHand;

    // デバッグ用タイマー
    private float debugTimer = 0f;
    private float debugInterval = 2.0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"[VRRigSync] Spawned via Netcode. IsOwner: {IsOwner}, NetworkObjectId: {NetworkObjectId}");
    }

    void Update()
    {
        if (!IsOwner) return;

        // --- 1. 参照取得チェック ---
        if (cameraRigCenterEye == null || cameraRigLeftHand == null || cameraRigRightHand == null)
        {
            FindVRRig();
            return;
        }

        // --- 2. 座標適用（オフセット計算込み） ---

        // 頭
        if (headTarget != null)
        {
            // 位置: リグの位置 + (リグの向き基準でのオフセット)
            Vector3 finalPos = cameraRigCenterEye.position + (cameraRigCenterEye.rotation * headPosOffset);
            // 回転: リグの回転 * オフセット回転
            Quaternion finalRot = cameraRigCenterEye.rotation * Quaternion.Euler(headRotOffset);

            headTarget.SetPositionAndRotation(finalPos, finalRot);
        }

        // 左手
        if (leftHandTarget != null)
        {
            Vector3 finalPos = cameraRigLeftHand.position + (cameraRigLeftHand.rotation * leftHandPosOffset);
            Quaternion finalRot = cameraRigLeftHand.rotation * Quaternion.Euler(leftHandRotOffset);

            leftHandTarget.SetPositionAndRotation(finalPos, finalRot);
        }

        // 右手
        if (rightHandTarget != null)
        {
            Vector3 finalPos = cameraRigRightHand.position + (cameraRigRightHand.rotation * rightHandPosOffset);
            Quaternion finalRot = cameraRigRightHand.rotation * Quaternion.Euler(rightHandRotOffset);

            rightHandTarget.SetPositionAndRotation(finalPos, finalRot);
        }

        // --- 3. 定期デバッグログ ---
        debugTimer += Time.deltaTime;
        if (debugTimer > debugInterval)
        {
            debugTimer = 0f;
            // ログが多くなりすぎないよう、オフセット適用後の値はInspectorで見る想定でログはシンプルに
            // LogStatus(); 
        }
    }

    private void FindVRRig()
    {
        if (cameraRigCenterEye == null)
        {
            var obj = GameObject.FindWithTag("MainCamera");
            if (obj) cameraRigCenterEye = obj.transform;
        }

        if (cameraRigLeftHand == null)
        {
            var obj = GameObject.FindWithTag("MultiLeftHand");
            if (obj) cameraRigLeftHand = obj.transform;
        }

        if (cameraRigRightHand == null)
        {
            var obj = GameObject.FindWithTag("MultiRightHand");
            if (obj) cameraRigRightHand = obj.transform;
        }
    }

    private void LogStatus()
    {
        string log = $"[VRRigSync Status] Owner: {IsOwner}\n";
        if (headTarget && cameraRigCenterEye)
            log += $"Head applied.\n";
        Debug.Log(log);
    }
}