using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private Transform doll; // DollのTransform

    [SerializeField] private RectTransform handleRect; // つまみオブジェクトのRectTransform
    [SerializeField] private Slider slider;           // 対応するSlider

    [SerializeField] private float maxSpeed = 20f; // 最大回転速度（度/秒）

    private float positionXMin = -80f;                // RectTransformのX座標の最小値
    private float positionXMax = 80f;                 // RectTransformのX座標の最大値

    private void Update()
    {
        // つまみの現在のX座標を取得
        float currentX = handleRect.anchoredPosition.x;

        // RectTransformのX座標をSlider値にマッピング
        float sliderValue = Mathf.Lerp(slider.minValue, slider.maxValue, Mathf.InverseLerp(positionXMin, positionXMax, currentX));

        // Sliderの値を更新
        slider.value = sliderValue;

        // 回転速度を計算（|sliderValue| が大きいほど速度も大きい）
        float rotationSpeed = Mathf.Lerp(0, maxSpeed, Mathf.Abs(sliderValue) / 180f);

        // 回転方向の決定 (sliderValueが負の場合は時計回り、正の場合は反時計回り)
        float rotationDirection = -Mathf.Sign(sliderValue); // -1: 時計回り, 1: 反時計回り

        // 回転量を計算 (時間で補正して一定速度にする)
        float deltaRotation = rotationDirection * rotationSpeed * Time.deltaTime;

        // DollをY軸中心に回転させる
        doll.Rotate(0, deltaRotation, 0);
    }

    /// <summary>
    /// Dollの回転を x:0, y:180, z:0 にリセットするメソッド
    /// </summary>
    public void ResetDollRotation()
    {
        if (doll != null)
        {
            doll.rotation = Quaternion.Euler(0, 180, 0);
            Debug.Log("Doll's rotation has been reset to x:0, y:180, z:0");
        }
        else
        {
            Debug.LogWarning("Doll Transform is not assigned in the inspector!");
        }
    }
}
