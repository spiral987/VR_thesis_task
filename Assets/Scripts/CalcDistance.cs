using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcDistance : MonoBehaviour
{
    public Transform object1;  // 1つ目のオブジェクト
    public Transform object2;  // 2つ目のオブジェクト

    void Update()
    {
        // オブジェクト間の距離を計算
        float distance = Vector3.Distance(object1.position, object2.position);

        // 距離をログに表示
        Debug.Log("Distance between the objects: " + distance);
    }
}
