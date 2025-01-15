using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Transform centerCamera;
    [SerializeField] Transform centerCameraForward;
    [SerializeField] Transform leftPalm;
    [SerializeField] float rotationOffset;
    [SerializeField] GameObject flatUnityCanvas;
    [SerializeField] GameObject[] UIs;
    [SerializeField] RectTransform[] uiRectTransforms;

    private bool isAnimation = false;
    private bool uiActive = false;

    private int nowActiveNum;

    [SerializeField] Drawing drawing;

    private void Start()
    {
        DOTween.Init();

        flatUnityCanvas.SetActive(false);
        for(int i = 0; i < UIs.Length; i++)
        {
            UIs[i].SetActive(false);
        }
    }

    public void SetUIVisibility(int num)
    {
        if (isAnimation) return;

        if (uiActive)
        {
            if(nowActiveNum == num)
            {
                uiActive = false;
            }
            else
            {
                uiActive = true;
                SwitchUI(nowActiveNum);
            }
            nowActiveNum = num;
        }
        else
        {
            nowActiveNum = num;

            flatUnityCanvas.SetActive(true);

            flatUnityCanvas.transform.position = centerCameraForward.position;

            flatUnityCanvas.transform.LookAt(centerCamera.position);

            flatUnityCanvas.transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
            uiActive = true;
        }
        AnimationUI(uiActive);
    }


    private void AnimationUI(bool toActive)
    {
        isAnimation = true;
        if (toActive)
        {
            uiRectTransforms[nowActiveNum].localScale = Vector3.zero;
            uiRectTransforms[nowActiveNum].position = leftPalm.transform.position;
            Transform parent = uiRectTransforms[nowActiveNum].parent;
            uiRectTransforms[nowActiveNum].SetParent(parent);
            UIs[nowActiveNum].SetActive(true);

            // ローカルスケールのアニメーション
            uiRectTransforms[nowActiveNum].DOScale(Vector3.one, 1.0f)
                .SetEase(Ease.OutSine) // イージング関数を設定
                .OnComplete(FinAnimation);

            // ローカル座標のアニメーション
            Vector3 targetPosition = new Vector3(1f, 1f, 1f);
            uiRectTransforms[nowActiveNum].DOLocalMove(targetPosition, 1.0f)
                .SetEase(Ease.OutSine); // イージング関数を設定
        }
        else
        {
            // ローカルスケールのアニメーション
            uiRectTransforms[nowActiveNum].DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InSine) // イージング関数を設定
                .OnComplete(FinAnimation);

            // ローカル座標のアニメーション
            uiRectTransforms[nowActiveNum].DOMove(leftPalm.transform.position, 0.5f)
                .SetEase(Ease.InSine); // イージング関数を設定

            Invoke("DeleteUI", 0.5f);
        }
    }

    private void FinAnimation()
    {
        isAnimation = false;
    }

    private void DeleteUI()
    {
        UIs[nowActiveNum].SetActive(false);
        flatUnityCanvas.SetActive(false);
    }

    private void SwitchUI(int num)
    {
        // ローカルスケールのアニメーション
        uiRectTransforms[num].DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InSine) // イージング関数を設定
            .OnComplete(() => SetUIInaactive(num));

        // ローカル座標のアニメーション
        uiRectTransforms[num].DOMove(leftPalm.transform.position, 0.5f)
            .SetEase(Ease.InSine); // イージング関数を設定

    }

    private void SetUIInaactive(int num)
    {
        UIs[num].SetActive(false);
    }
}
