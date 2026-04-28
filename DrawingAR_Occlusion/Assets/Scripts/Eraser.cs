using System.Collections.Generic;
using UnityEngine;

public class Eraser : MonoBehaviour
{
    [Header("消しゴムの設定")]
    public float eraseRadius = 0.05f; // 消す範囲の半径
    public GameObject linePrefab;     // 分割して新しく線を作るためのプレハブ

    // 計算を軽くするための「半径の2乗」を入れておく変数
    private float squaredEraseRadius;

    void Start()
    {
        // 毎回ルート計算をしないように、スタート時に半径を2乗しておく
        squaredEraseRadius = eraseRadius * eraseRadius;
    }

    // 消しゴム（Trigger）が何かに触れ続けている間呼ばれる
    void OnTriggerStay(Collider other)
    {
        // 触れたものが描画した線（DrawnLine）だったら
        if (other.CompareTag("DrawnLine"))
        {
            LineRenderer targetLine = other.GetComponent<LineRenderer>();
            if (targetLine != null)
            {
                EraseAndSplitLine(targetLine);
            }
        }
    }

    // 線を分割するメイン処理
    private void EraseAndSplitLine(LineRenderer line)
    {
        int pointCount = line.positionCount;
        if (pointCount < 2) return;

        // 今の線の頂点座標をすべて取得する
        Vector3[] positions = new Vector3[pointCount];
        line.GetPositions(positions);

        // 分割後の「前半」と「後半」を入れるリストを用意
        List<Vector3> part1 = new List<Vector3>();
        List<Vector3> part2 = new List<Vector3>();
        
        bool isPart2 = false; // 今後半の線を処理しているかどうかのフラグ
        bool hasErased = false; // 1箇所でも消したかどうかのフラグ

        for (int i = 0; i < pointCount; i++)
        {
            // 消しゴムの現在地と、頂点の距離を比較（爆速のsqrMagnitude）
            if ((positions[i] - transform.position).sqrMagnitude < squaredEraseRadius)
            {
                // 消しゴムの範囲内に入った点＝消す！
                // ここを境目に、次からは後半(part2)の線として扱う
                isPart2 = true;
                hasErased = true;
            }
            else
            {
                // 消しゴムの範囲外の点（残す点）
                if (!isPart2)
                {
                    part1.Add(positions[i]); // 消す場所より前なら前半に追加
                }
                else
                {
                    part2.Add(positions[i]); // 消す場所より後なら後半に追加
                }
            }
        }

        // もし1箇所も消していなければ、何もしない（ここで無限ループを防いでいるのだ！）
        if (!hasErased) return;


        // ーーー ここから線の更新と分割 ーーー

        // 【前半の線】元のオブジェクトの長さを縮めて使い回す
        if (part1.Count >= 2)
        {
            line.positionCount = part1.Count;
            line.SetPositions(part1.ToArray());
            UpdateCollider(line); // ★短くなった線で安全なBoxColliderを作り直す
        }
        else
        {
            // 残った点が少なすぎる場合は、線ごと消滅させる
            Destroy(line.gameObject);
        }

        // 【後半の線】新しくプレハブを生成して残りの頂点を入れる
        if (part2.Count >= 2)
        {
            GameObject newLineObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            newLineObj.tag = "DrawnLine";
            LineRenderer newLine = newLineObj.GetComponent<LineRenderer>();
            
            newLine.positionCount = part2.Count;
            newLine.SetPositions(part2.ToArray());
            UpdateCollider(newLine); // ★新しい線の安全なBoxColliderを作る
        }
    }

    // ★修正箇所：BakeMeshをやめて、Drawing.csと同じ安全なBoxCollider方式に統一したのだ！
    private void UpdateCollider(LineRenderer line)
    {
        int pointCount = line.positionCount;
        if (pointCount < 2) return;

        Vector3[] positions = new Vector3[pointCount];
        line.GetPositions(positions);

        Bounds bounds = new Bounds(positions[0], Vector3.zero);
        for (int i = 1; i < pointCount; i++)
        {
            bounds.Encapsulate(positions[i]);
        }
        bounds.Expand(line.startWidth * 2f);

        BoxCollider boxCollider = line.gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = line.gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;
        boxCollider.isTrigger = true;
    }
}