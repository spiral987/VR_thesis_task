using UnityEngine;

public class SyncPosition : MonoBehaviour
{
    [SerializeField] private GameObject object1; // 基準となるオブジェクト
    [SerializeField] private GameObject object2; // 移動させるオブジェクト
    [SerializeField] private float tolerance = 0.01f; // 不一致とみなす許容誤差

    private void Update()
    {
        if (object1 == null || object2 == null)
        {
            Debug.LogWarning("Object1 または Object2 が設定されていません。");
            return;
        }

        // オブジェクト1とオブジェクト2の位置が不一致の場合
        if (Vector3.Distance(object1.transform.position, object2.transform.position) > tolerance)
        {
            // オブジェクト2をオブジェクト1の位置に移動
            object2.transform.position = object1.transform.position;
        }
    }
}
