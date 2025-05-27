using UnityEngine;

public class MaintainDistance : MonoBehaviour
{
    [SerializeField] private Transform object1; // ��ƂȂ�I�u�W�F�N�g
    [SerializeField] private Transform object2; // ������ۂI�u�W�F�N�g
    [SerializeField] private float targetDistance = 5f; // �ۂ���������

    private void Update()
    {
        if (object1 == null || object2 == null)
        {
            Debug.LogWarning("Object1�܂���Object2���ݒ肳��Ă��܂���B");
            return;
        }

        // Object1����Object2�ւ̃x�N�g�����v�Z
        Vector3 direction = object2.position - object1.position;
        float currentDistance = direction.magnitude;

        // �������v�Z���A�ڕW�����Ƃ̍�����K�p
        if (!Mathf.Approximately(currentDistance, targetDistance))
        {
            Vector3 newPosition = object1.position + direction.normalized * targetDistance;
            object2.position = newPosition;
        }
    }
}
