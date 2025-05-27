using Oculus.Interaction;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderResetter : MonoBehaviour
{
    [SerializeField] private RectTransform handleRect; // �܂݃I�u�W�F�N�g��RectTransform

    [SerializeField]
    private Grabbable _grabbable;

    //[SerializeField]
   // private FingerPinchValue _fingerPinchValue; // FingerPinchValue�̎Q��

    private bool _isGrabbing = false; // ���ݒ͂�ł��邩�ǂ����̃t���O

    //private bool _isDisplay = true;

    private void Start()
    {
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
        //float pinchValue = _fingerPinchValue.Value(); // �s���`���x���擾
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
            // �͂ޏI�����̏���
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
        // �ꎞ�ϐ��ɕۑ�����x�̒l��ύX
        Vector2 newPosition = handleRect.anchoredPosition;
        newPosition.x = 0;

        // �đ��
        handleRect.anchoredPosition = newPosition;
    }

}
