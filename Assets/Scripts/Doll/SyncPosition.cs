using UnityEngine;

public class SyncPosition : MonoBehaviour
{
    [SerializeField] private GameObject object1; // ��ƂȂ�I�u�W�F�N�g
    [SerializeField] private GameObject object2; // �ړ�������I�u�W�F�N�g
    [SerializeField] private float tolerance = 0.01f; // �s��v�Ƃ݂Ȃ����e�덷

    private void Update()
    {
        if (object1 == null || object2 == null)
        {
            Debug.LogWarning("Object1 �܂��� Object2 ���ݒ肳��Ă��܂���B");
            return;
        }

        // �I�u�W�F�N�g1�ƃI�u�W�F�N�g2�̈ʒu���s��v�̏ꍇ
        if (Vector3.Distance(object1.transform.position, object2.transform.position) > tolerance)
        {
            // �I�u�W�F�N�g2���I�u�W�F�N�g1�̈ʒu�Ɉړ�
            object2.transform.position = object1.transform.position;
        }
    }
}
