using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using Unity.VisualScripting;
using Oculus.Interaction.Input;


public class GrabCountTracker : MonoBehaviour
{

    [SerializeField]
    private GrabCountManager _manager;

    private Grabbable _grabbable;

    private bool _isGrabbing = false; // ���ݒ͂�ł��邩�ǂ����̃t���O

    [SerializeField]
    private int JointIndex; // ���̃g���b�J�[���S������W���C���g�̃C���f�b�N�X

    [SerializeField]
    private Transform jointPosition;

    private int grabCount = 0; // ���̃W���C���g���͂܂ꂽ��


    private void Start()
    {
        _manager = FindObjectOfType<GrabCountManager>();
        if (_manager == null)
        {
            Debug.LogError("GrabCountManager not found in the scene.");
        }
        // Grabbable���擾
        _grabbable = GetComponent<Grabbable>();
        if (_grabbable == null)
        {
            Debug.LogError("Grabbable�����̃I�u�W�F�N�g�Ɍ�����܂���B");
            return;
        }
        // Grabbable�̃C�x���g���X�i�[�ɓo�^
        _grabbable.WhenPointerEventRaised += OnPointerEventRaised;
    }

    void Update()
    {
        if (_isGrabbing)
        {
            _manager.Calculatedistance(jointPosition);
        }
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
            _isGrabbing = true;

            // �͂ރC�x���g�����������ꍇ�ɃJ�E���g�𑝉�
            StartGrab();

        }

        else if (evt.Type == PointerEventType.Unselect)
        {
            // �͂ޏI�����̏���
            if (_isGrabbing)
            {
                _isGrabbing = false;
                EndGrab();
                
            }

        }
    }

    public void StartGrab()
    {
        if (_manager != null)
        {
            _manager.StartGrab(JointIndex, jointPosition.position, jointPosition.rotation, jointPosition);
            //_manager.LogQueueContents();
        }
    }

    public void EndGrab()
    {
        if (_manager != null)
        {
            _manager.EndGrab(JointIndex, jointPosition.position, jointPosition.rotation, jointPosition);
            //_manager.LogQueueContents();
            _manager.LogAllGrabData();
        }
    }

}

