using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using TMPro;
using Oculus.Interaction.HandGrab;

public class ChangeLayerOnToggle : MonoBehaviour
{
    [SerializeField] private GameObject _worldDoll; // ���C���[��ύX����Ώۂ̃I�u�W�F�N�g1
    [SerializeField] private GameObject _Hips; // ���C���[��ύX����Ώۂ̃I�u�W�F�N�g2

    [SerializeField] private string newLayerName1 = "None"; // �ύX��̃��C���[��1
    [SerializeField] private string newLayerName2 = "Both"; // �ύX��̃��C���[��2

    [SerializeField]
    private TMP_Text targetText; // �F��ύX����Ώۂ�TextMeshPro

    [SerializeField]
    private DistanceHandGrabInteractable _dhandGrab;

    [SerializeField]
    private HandGrabInteractable _handGrab;


    [SerializeField]
    private Color onColor = Color.green; // �g�O�����I���̂Ƃ��̐F
    [SerializeField]
    private Color offColor = Color.red;  // �g�O�����I�t�̂Ƃ��̐F


    private bool toggle=false;

    private void Start()
    {
        // �g�O���̏�����Ԃɉ����ĐF��ݒ�
        targetText.color = offColor;

    }

    void Update()
    {
        if (Input.GetKeyDown("a")){
            ChangeLayer();
        }
    }

    // �{�^�����������Ƃ��ɌĂяo����郁�\�b�h
    public void ChangeLayer()
    {
        toggle = !toggle;
        if (toggle)
        {
            //�{�^���̐F��ύX
            targetText.color = onColor;

            //Distancegrab��L����
            _handGrab.enabled=true;
            //Distancegrab��L����
            _dhandGrab.enabled = true;

            // ���C���[�������C���[�ԍ��ɕϊ�
            int newLayer1 = LayerMask.NameToLayer(newLayerName1);
            int newLayer2 = LayerMask.NameToLayer(newLayerName2);

            // �I�u�W�F�N�g�Ƃ��̎q�I�u�W�F�N�g�̃��C���[��ύX
            SetLayerRecursively(_worldDoll, newLayer2);
            SetLayerRecursively(_Hips, newLayer1);
            Debug.Log("toggle:true");
        }
        else
        {
            //�{�^���̐F��ύX
            targetText.color = offColor;

            //Distancegrab�𖳌���
            _handGrab.enabled = false;

            //Distancegrab�𖳌���
            _dhandGrab.enabled = false;

            // ���C���[�������C���[�ԍ��ɕϊ�
            int newLayer1 = LayerMask.NameToLayer(newLayerName1);
            int newLayer2 = LayerMask.NameToLayer(newLayerName2);

            // �I�u�W�F�N�g�Ƃ��̎q�I�u�W�F�N�g�̃��C���[��ύX
            SetLayerRecursively(_worldDoll, newLayer1);
            SetLayerRecursively(_Hips, newLayer2);
            Debug.Log("toggle:false");

        }
        
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        // �q�I�u�W�F�N�g���ċA�I�ɕύX
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
