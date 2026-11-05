using Unity.Netcode;
using UnityEngine;

public struct HandData : INetworkSerializable
{
    public Quaternion[] BoneRotations;
    // 【追加】ボーンの位置情報
    public Vector3[] BonePositions; 

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = 0;
        if (!serializer.IsReader) length = BoneRotations?.Length ?? 0;
        serializer.SerializeValue(ref length);

        if (serializer.IsReader) 
        {
            BoneRotations = new Quaternion[length];
            BonePositions = new Vector3[length]; // 【追加】初期化
        }

        for (int n = 0; n < length; n++)
        {
            serializer.SerializeValue(ref BoneRotations[n]);
            serializer.SerializeValue(ref BonePositions[n]); // 【追加】シリアライズ
        }
    }
}