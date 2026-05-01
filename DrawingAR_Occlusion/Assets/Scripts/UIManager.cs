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
        // DOTweenの初期化
        DOTween.Init();

        // 初期状態ではキャンバスと各UIを非表示にする
        flatUnityCanvas.SetActive(false);
        for(int i = 0; i < UIs.Length; i++)
        {
            UIs[i].SetActive(false);
        }
    }

    public void SetUIVisibility(int num)
    {
        // アニメーション再生中は入力を受け付けない
        if (isAnimation) return;

        if (uiActive)
        {
            // 同じUIボタンが押された場合は閉じる
            if(nowActiveNum == num)
            {
                uiActive = false;
            }
            else
            {
                // 違うUIボタンが押された場合は、現在開いているものを閉じて新しいものを開く
                uiActive = true;
                SwitchUI(nowActiveNum);
            }
            nowActiveNum = num;
        }
        else
        {
            // UIを開く処理
            nowActiveNum = num;

            flatUnityCanvas.SetActive(true);

            // キャンバスの位置をカメラの前方に設定
            flatUnityCanvas.transform.position = centerCameraForward.position;

            // キャンバスをカメラの方向へ向かせる
            flatUnityCanvas.transform.LookAt(centerCamera.position);

            // LookAtで裏返ってしまう場合があるため、Y軸を180度回転させて正面を向ける
            flatUnityCanvas.transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
            uiActive = true;
        }
        
        // アニメーションの実行
        AnimationUI(uiActive);
    }


    private void AnimationUI(bool toActive)
    {
        isAnimation = true;
        if (toActive)
        {
            // UIを開くとき：左手の位置からスケール0で出現し、指定座標へ拡大しながら移動する
            uiRectTransforms[nowActiveNum].localScale = Vector3.zero;
            uiRectTransforms[nowActiveNum].position = leftPalm.transform.position;
            Transform parent = uiRectTransforms[nowActiveNum].parent;
            uiRectTransforms[nowActiveNum].SetParent(parent);
            UIs[nowActiveNum].SetActive(true);

            // スケールのアニメーション（0から1へ）
            uiRectTransforms[nowActiveNum].DOScale(Vector3.one, 1.0f)
                .SetEase(Ease.OutSine) // イージング関数の設定（滑らかに減速）
                .OnComplete(FinAnimation);

            // ローカル座標の移動アニメーション
            Vector3 targetPosition = new Vector3(1f, 1f, 1f);
            uiRectTransforms[nowActiveNum].DOLocalMove(targetPosition, 1.0f)
                .SetEase(Ease.OutSine); // イージング関数の設定（滑らかに減速）
        }
        else
        {
            // UIを閉じるとき：スケールを0にしながら、左手の位置へ戻る
            
            // スケールのアニメーション（1から0へ）
            uiRectTransforms[nowActiveNum].DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InSine) // イージング関数の設定（滑らかに加速）
                .OnComplete(FinAnimation);

            // ワールド座標の移動アニメーション（左手の位置へ）
            uiRectTransforms[nowActiveNum].DOMove(leftPalm.transform.position, 0.5f)
                .SetEase(Ease.InSine); // イージング関数の設定（滑らかに加速）

            // 0.5秒後（アニメーション完了後）にUIを非アクティブにする
            Invoke("DeleteUI", 0.5f);
        }
    }

    // アニメーション終了時のフラグ解除
    private void FinAnimation()
    {
        isAnimation = false;
    }

    // UIとキャンバスを非アクティブにする
    private void DeleteUI()
    {
        UIs[nowActiveNum].SetActive(false);
        flatUnityCanvas.SetActive(false);
    }

    // 別のUIに切り替える際、古いUIを閉じるアニメーション
    private void SwitchUI(int num)
    {
        // スケールのアニメーション（1から0へ縮小）
        uiRectTransforms[num].DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InSine) // イージング関数の設定
            .OnComplete(() => SetUIInactive(num));

        // ワールド座標の移動アニメーション（左手の位置へ戻る）
        uiRectTransforms[num].DOMove(leftPalm.transform.position, 0.5f)
            .SetEase(Ease.InSine); // イージング関数の設定
    }

    // 指定したUIを非アクティブにする
    private void SetUIInactive(int num)
    {
        UIs[num].SetActive(false);
    }
}