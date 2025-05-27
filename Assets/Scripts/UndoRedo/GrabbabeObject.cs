using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GrabbableObject : MonoBehaviour
{
    private GlobalUndoRedoManager globalManager;

    private void Start()
    {
        // GlobalUndoRedoManager���擾
        globalManager = FindObjectOfType<GlobalUndoRedoManager>();
    }

    public void OnGrab()
    {
        // �I�u�W�F�N�g��͂񂾍ۂɌ��݂̏�Ԃ�ۑ�
        globalManager.SaveAction(transform);
    }

    public void OnRelease()
    {
        // �K�v�Ȃ烊���[�X���̒ǉ�����
    }
}
