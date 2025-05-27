using Oculus.Interaction;
using UnityEngine;
using TMPro;

public class CustomGrabbable : MonoBehaviour
{

    private Grabbable _grabbable;

    [SerializeField]
    private GlobalUndoRedoManager globalManager;

    private void Start()
    {
        // Grabbableコンポーネントを取得
        _grabbable = GetComponent<Grabbable>();
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
        if (evt.Type == PointerEventType.Select)
        {
            // オブジェクトを掴んだときの状態を保存
            globalManager.SaveAction(transform);
        }
    }

}
