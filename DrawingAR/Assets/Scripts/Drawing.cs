using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Drawing : MonoBehaviour
{
    [SerializeField] GameObject lineObjPrefab;
    private GameObject currentLineObj;

    [SerializeField] OVRHand rightOVRHand;
    [SerializeField] OVRHand leftOVRHand;

    [SerializeField] OVRSkeleton rightSkeleton;
    [SerializeField] OVRSkeleton leftSkeleton;
    public float touchDistanceThreshold = 0.02f; // 指先がくっついているか判定する閾値

    private Color lineColor;
    [SerializeField] Renderer rightHandRenderer;

    private float lineWidth = 0.01f;

    private LineRenderer lineRenderer;

    private bool havePalette = false;
    [SerializeField] GameObject paletteObjPrefab;
    private GameObject paletteObj;
    [SerializeField] Transform centerCamera;
    [SerializeField] float during = 5.0f;

    private GameObject lineParent;
    private bool isAnimation = false;
    [SerializeField] Transform returnPos;

    private bool canDraw = true;
    public bool CanDraw
    {
        set { canDraw = value; }
    }

    private void Start()
    {
        lineColor = new Color(0.0f, 0.0f, 0.0f, 20.0f);
        touchDistanceThreshold *= touchDistanceThreshold;
    }

    private void Update()
    {
        // ハンドトラッキングをしていない、またはハンドトラッキングの信用度が低ければ誤作動を防ぐために無効にする
        if (!rightOVRHand.IsTracked || rightOVRHand.HandConfidence.Equals(OVRHand.TrackingConfidence.Low)) return;

        if (!canDraw) return;

        var indexTipPosR = rightSkeleton.Bones[(int)OVRSkeleton.BoneId.XRHand_IndexTip].Transform.position;
        var thumbTipPosR = rightSkeleton.Bones[(int)OVRSkeleton.BoneId.XRHand_ThumbTip].Transform.position;

        // 2つの指の位置の差を計算
        float distanceR = Vector3.SqrMagnitude(indexTipPosR - thumbTipPosR);

        Debug.Log(distanceR);
        // 指先がくっついているかどうかを判定
        if (distanceR < touchDistanceThreshold)
        {
            Debug.Log("書く！");
            if (currentLineObj == null)
            {
                if (lineParent == null)
                {
                    lineParent = new GameObject();
                    lineParent.name = "LineParent";
                }

                currentLineObj = Instantiate(lineObjPrefab, Vector3.zero, Quaternion.identity);
                lineRenderer = currentLineObj.GetComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;

                currentLineObj.transform.parent = lineParent.transform;
            }

            int nextPosIndex = lineRenderer.positionCount;
            lineRenderer.positionCount = nextPosIndex + 1;
            lineRenderer.SetPosition(nextPosIndex, indexTipPosR);
        }
        else
        {
            if(currentLineObj != null)
            {
                currentLineObj = null;
            }
        }
    }

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
            Destroy(paletteObj);
            paletteObj = null;
            havePalette = false;
        }
    }

    public void DestroyPalette()
    {
        if(paletteObj != null)
        {
            Destroy(paletteObj);
            paletteObj = null;
            havePalette = false;
        }
    }

    public void ColorChange(Color _color)
    {
        lineColor = _color;
        rightHandRenderer.GetComponent<Renderer>().material.color = lineColor;
    }

    public void ColorDetailChange(float r, float g, float b, float w)
    {
        lineColor = new Color(r, g, b);
        rightHandRenderer.GetComponent<Renderer>().material.color = lineColor;

        lineWidth = w / 500.0f;
    }

    public void DeleteLines()
    {
        if(isAnimation || lineParent == null) return;
        canDraw = false;
        isAnimation = true;

        // ローカルスケールのアニメーション
        lineParent.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InSine) // イージング関数を設定
            .OnComplete(FinAnimation);

        // ローカル座標のアニメーション
        lineParent.transform.DOMove(returnPos.transform.position, 0.5f)
            .SetEase(Ease.InSine); // イージング関数を設定
    }
    private void FinAnimation()
    {
        Destroy(lineParent);
        lineParent = null;

        isAnimation = false;
        canDraw = true;
    }
}
