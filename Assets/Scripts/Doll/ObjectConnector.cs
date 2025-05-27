using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ObjectConnector : MonoBehaviour
{
    [SerializeField]
    private Transform objectA; // �I�u�W�F�N�gA
    [SerializeField]
    private Transform objectB; // �I�u�W�F�N�gB

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // ���C���̐ݒ�i�K�v�ɉ����ăJ�X�^�}�C�Y�j
        lineRenderer.startWidth = 0.003f; // ���̊J�n���̑���
        lineRenderer.endWidth = 0.003f;   // ���̏I�����̑���
        lineRenderer.positionCount = 2; // ���C���̓_�̐�
    }

    private void Update()
    {
        if (objectA != null && objectB != null)
        {
            // ���C���̗��[���I�u�W�F�N�g�̈ʒu�ɐݒ�
            lineRenderer.SetPosition(0, objectA.position);
            lineRenderer.SetPosition(1, objectB.position);
        }
    }
}
