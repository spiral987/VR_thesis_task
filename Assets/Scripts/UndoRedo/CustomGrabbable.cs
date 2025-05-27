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
        // Grabbable�R���|�[�l���g���擾
        _grabbable = GetComponent<Grabbable>();
        if (_grabbable == null)
        {
            Debug.LogError("Grabbable component is missing.");
            return;
        }

        // Grabbable�̃C�x���g���X�i�[�ɓo�^
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
            // �I�u�W�F�N�g��͂񂾂Ƃ��̏�Ԃ�ۑ�
            globalManager.SaveAction(transform);
        }
    }

}
