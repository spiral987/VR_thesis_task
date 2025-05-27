using Oculus.Interaction;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderResetter : MonoBehaviour
{
    [SerializeField] private RectTransform handleRect; // つまみオブジェクトのRectTransform

    [SerializeField]
    private Grabbable _grabbable;

    //[SerializeField]
   // private FingerPinchValue _fingerPinchValue; // FingerPinchValueの参照

    private bool _isGrabbing = false; // 現在掴んでいるかどうかのフラグ

    //private bool _isDisplay = true;

    private void Start()
    {
        if (_grabbable == null)
        {
            Debug.LogError("Grabbable component is missing.");
            return;
        }

        // Grabbableのイベントリスナーに登録
        _grabbable.WhenPointerEventRaised += OnPointerEventRaised;


    }

    private void OnDestroy()
    {
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised -= OnPointerEventRaised;
        }
    }

    private void OnPointerEventRaised(PointerEvent evt)
    {
        //float pinchValue = _fingerPinchValue.Value(); // ピンチ強度を取得
        if (evt.Type == PointerEventType.Select)
        {
          //  Debug.Log("Grabbed.");
           // if (_isDisplay)
           // {
          //      Debug.Log($"value:{pinchValue}");
          //      _isDisplay = false;
          //  }
            _isGrabbing = true;
        }

        if (evt.Type == PointerEventType.Unselect)
        {
            // 掴む終了時の処理
            if (_isGrabbing)
            {
                _isGrabbing = false;
           //     _isDisplay = true;
                EndGrab();
            }

        }
    }

    public void EndGrab()
    {
        // 一時変数に保存してxの値を変更
        Vector2 newPosition = handleRect.anchoredPosition;
        newPosition.x = 0;

        // 再代入
        handleRect.anchoredPosition = newPosition;
    }

}
