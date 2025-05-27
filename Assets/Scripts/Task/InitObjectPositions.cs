using UnityEngine;

public class InitObjectPositions : MonoBehaviour
{
    [Header("�v���C���[�I�u�W�F�N�g")]
    [SerializeField] private Transform playerTransform; // �v���C���[��Transform

    [Header("�ΏۃI�u�W�F�N�g")]
    [SerializeField] private Transform objectA; // �I�u�W�F�N�gA
    [SerializeField] private Transform objectB; // �I�u�W�F�N�gB
    [SerializeField] private Transform objectC; // �I�u�W�F�N�gC

    [Header("���Έʒu (�v���C���[�)")]
    [SerializeField] private Vector3 relativePositionA = new Vector3(2f, 0f, 1f); // �E2m�A�O1m
    [SerializeField] private Vector3 relativePositionB = new Vector3(-1f, 0f, -0.5f); // ��1m�A��0.5m
    [SerializeField] private Vector3 relativePositionC = new Vector3(0f, 1f, 0f); // ��1m

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform���ݒ肳��Ă��܂���B");
            return;
        }


    }

    public void Setting()
    {
        // �v���C���[�̈ʒu�Ɖ�]����ɑ��Έʒu���v�Z���Ĕz�u
        SetRelativePosition(objectA, relativePositionA);
        SetRelativePosition(objectB, relativePositionB);
        SetRelativePosition(objectC, relativePositionC);
    }

    private void SetRelativePosition(Transform obj, Vector3 relativePosition)
    {
        if (obj == null)
        {
            Debug.LogWarning("�I�u�W�F�N�g�����ݒ�ł��B");
            return;
        }

        // �v���C���[����̂̑��Έʒu���v�Z
        Vector3 worldPosition = playerTransform.position + relativePosition;
        obj.position = worldPosition;
    }
}
