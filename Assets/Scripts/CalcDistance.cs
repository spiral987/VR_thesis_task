using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcDistance : MonoBehaviour
{
    public Transform object1;  // 1�ڂ̃I�u�W�F�N�g
    public Transform object2;  // 2�ڂ̃I�u�W�F�N�g

    void Update()
    {
        // �I�u�W�F�N�g�Ԃ̋������v�Z
        float distance = Vector3.Distance(object1.position, object2.position);

        // ���������O�ɕ\��
        Debug.Log("Distance between the objects: " + distance);
    }
}
