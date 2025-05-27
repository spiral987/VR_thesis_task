using UnityEngine;

public class MaintainDistance : MonoBehaviour
{
    [SerializeField] private Transform object1; // 基準となるオブジェクト
    [SerializeField] private Transform object2; // 距離を保つオブジェクト
    [SerializeField] private float targetDistance = 5f; // 保ちたい距離

    private void Update()
    {
        if (object1 == null || object2 == null)
        {
            Debug.LogWarning("Object1またはObject2が設定されていません。");
            return;
        }

        // Object1からObject2へのベクトルを計算
        Vector3 direction = object2.position - object1.position;
        float currentDistance = direction.magnitude;

        // 距離を計算し、目標距離との差分を適用
        if (!Mathf.Approximately(currentDistance, targetDistance))
        {
            Vector3 newPosition = object1.position + direction.normalized * targetDistance;
            object2.position = newPosition;
        }
    }
}
