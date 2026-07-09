using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Drawing : MonoBehaviour
{
    [Header("描画設定")]
    [SerializeField] private GameObject lineObjPrefab;
    private GameObject currentLineObj;
    private LineRenderer lineRenderer;
    private Vector3 lastDrawPosition;
    public float minPointDistance = 0.005f; // 点を追加する最小距離
    private bool isDrawing;
    
    private bool canDraw = true;
    public bool CanDraw { set { canDraw = value; } }

    [Header("トラッキング情報")]
    [SerializeField] private OVRHand rightOVRHand;
    [SerializeField] private OVRHand leftOVRHand;
    [SerializeField] private OVRSkeleton rightSkeleton;
    [SerializeField] private OVRSkeleton leftSkeleton;

    private Transform indexTipTransform;

    // ★修正：距離計算用の変数（touchDistanceThreshold等）は不要になったので削除！

    [Header("描画情報")]
    [SerializeField] private Renderer rightHandRenderer;
    private Color lineColor;
    public float LineWidth { get; set; } = 0.01f;

    [SerializeField] private GameObject paletteObjPrefab;
    [SerializeField] private GameObject eraserObjPrefab;

    [SerializeField] private Vector3 eraserRotationOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private Transform centerCamera;
    [SerializeField] private float during = 5.0f;
    private GameObject paletteObj;
    private bool havePalette = false;

    private GameObject eraserObj;
    private bool haveEraser = false;

    [Header("全消去アニメーション設定")]
    [SerializeField] private Transform returnPos;
    private GameObject lineParent;
    private bool isAnimation = false;

    private void Start()
    {
        lineColor = rightHandRenderer.material.color;
    }

private void Update()
    {
        // ハンドトラッキングの信用度が低ければ処理をスキップ
        if (!rightOVRHand.IsTracked || rightOVRHand.HandConfidence == OVRHand.TrackingConfidence.Low) return;
        if (!canDraw) return;

        // ★安全対策1：骨格データ（Skeleton）のロードが完了するまで処理を待つ！
        if (!rightSkeleton.IsInitialized) return;

        // ★安全対策2：インデックス番号を直接指定せず、確実に人差し指を探して記憶する
        if (indexTipTransform == null)
        {
            foreach (var bone in rightSkeleton.Bones)
            {
                // OpenXR形式でも、旧OVR形式でも、どちらが設定されていても確実に見つけ出すのだ！
                if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip ||
                    bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                {
                    indexTipTransform = bone.Transform;
                    break;
                }
            }

            // まだ見つかっていなければ、見つかるまでスキップ
            if (indexTipTransform == null) return;
        }

        // ★Meta公式のピンチ判定を使用
        bool isPinching = rightOVRHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // キャッシュ（記憶）した人差し指の先端の座標を使う
        Vector3 indexTipPosR = indexTipTransform.position;

        // 指先がくっついている（ピンチしている）かどうかを判定
        if (isPinching)
        {
            // くっついた瞬間：新しい線を作り始める
            if (currentLineObj == null)
            {
                if (lineParent == null)
                {
                    lineParent = new GameObject("LineParent");
                }

                isDrawing = true;
                currentLineObj = Instantiate(lineObjPrefab, Vector3.zero, Quaternion.identity);
                currentLineObj.tag = "DrawnLine";
                currentLineObj.transform.parent = lineParent.transform;

                lineRenderer = currentLineObj.GetComponent<LineRenderer>();
                lineRenderer.material.color = lineColor;
                lineRenderer.startWidth = LineWidth;
                lineRenderer.endWidth = LineWidth;

                lineRenderer.positionCount = 1;
                lineRenderer.SetPosition(0, indexTipPosR);
                lastDrawPosition = indexTipPosR;
            }
            // くっついたまま移動中：線を伸ばす
            else
            {
                // エリア内 かつ 指が一定距離動いた時だけ点を追加
                if (Vector3.SqrMagnitude(lastDrawPosition - indexTipPosR) > (minPointDistance * minPointDistance))
                {
                    int nextPosIndex = lineRenderer.positionCount;
                    lineRenderer.positionCount = nextPosIndex + 1;
                    lineRenderer.SetPosition(nextPosIndex, indexTipPosR);
                    lastDrawPosition = indexTipPosR;
                }
            }
        }
        else
        {
            // 指が離れた時：現在の線を終了する
            if (currentLineObj != null)
            {
                // 線を引き終わった瞬間に当たり判定(BoxCollider)を生成
                GenerateColliderForLine(lineRenderer);

                isDrawing = false;
                currentLineObj = null;
                lineRenderer = null;
            }
        }
    }

    // 描いた線をすっぽり覆う大きなBoxColliderを1つだけ生成する処理
    private void GenerateColliderForLine(LineRenderer line)
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

    // --- パレット出現・消去 ---
    public void PaletteController()
    {
        if (!havePalette)
        {
            Vector3 createPos = leftSkeleton.gameObject.transform.position + centerCamera.forward * during;
            paletteObj = Instantiate(paletteObjPrefab, createPos, Quaternion.identity);
            paletteObj.transform.LookAt(centerCamera);
            havePalette = true;
        }
        else
        {
            DestroyPalette();
        }
    }

    public void DestroyPalette()
    {
        if (paletteObj != null)
        {
            Destroy(paletteObj);
            paletteObj = null;
            havePalette = false;
        }
    }

    // --- 消しゴム出現・消去
    public void EraserController()
    {
        if (!haveEraser)
        {
            Vector3 createPos = leftSkeleton.gameObject.transform.position + centerCamera.forward * during;
            eraserObj = Instantiate(eraserObjPrefab, createPos, Quaternion.identity);
            eraserObj.transform.LookAt(centerCamera);
            haveEraser = true;
        }
        else
        {
            DestroyEraser();
        }
    }

    public void DestroyEraser()
    {
        if (eraserObj != null)
        {
            Destroy(eraserObj);
            eraserObj = null;
            haveEraser = false;
        }
    }

    // --- 色・太さ変更 ---
    public void ColorChange(float r, float g, float b, float w)
    {
        lineColor = new Color(r, g, b);
        
        if (rightHandRenderer != null)
        {
            rightHandRenderer.material.color = lineColor;
        }

        LineWidth = w / 500.0f;

        // 描画中に色を変えた場合、元の線にColliderを付けてから新しく切り替える
        if (isDrawing && currentLineObj != null)
        {
            GenerateColliderForLine(lineRenderer);
            isDrawing = false;
            currentLineObj = null;
            lineRenderer = null;
        }
    }

    // --- 全消去アニメーション ---
    public void DeleteLines()
    {
        if (isAnimation || lineParent == null) return;
        canDraw = false;
        isAnimation = true;

        lineParent.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InSine)
            .OnComplete(FinAnimation);

        lineParent.transform.DOMove(returnPos.position, 0.5f)
            .SetEase(Ease.InSine);
    }

    private void FinAnimation()
    {
        Destroy(lineParent);
        lineParent = null;
        isAnimation = false;
        canDraw = true;
    }
}