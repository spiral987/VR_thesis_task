using UnityEngine;

public class InitObjectPositions : MonoBehaviour
{
    [Header("プレイヤーオブジェクト")]
    [SerializeField] private Transform playerTransform; // プレイヤーのTransform

    [Header("対象オブジェクト")]
    [SerializeField] private Transform objectA; // オブジェクトA
    [SerializeField] private Transform objectB; // オブジェクトB
    [SerializeField] private Transform objectC; // オブジェクトC

    [Header("相対位置 (プレイヤー基準)")]
    [SerializeField] private Vector3 relativePositionA = new Vector3(2f, 0f, 1f); // 右2m、前1m
    [SerializeField] private Vector3 relativePositionB = new Vector3(-1f, 0f, -0.5f); // 左1m、後0.5m
    [SerializeField] private Vector3 relativePositionC = new Vector3(0f, 1f, 0f); // 上1m

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transformが設定されていません。");
            return;
        }


    }

    public void Setting()
    {
        // プレイヤーの位置と回転を基準に相対位置を計算して配置
        SetRelativePosition(objectA, relativePositionA);
        SetRelativePosition(objectB, relativePositionB);
        SetRelativePosition(objectC, relativePositionC);
    }

    private void SetRelativePosition(Transform obj, Vector3 relativePosition)
    {
        if (obj == null)
        {
            Debug.LogWarning("オブジェクトが未設定です。");
            return;
        }

        // プレイヤーからのの相対位置を計算
        Vector3 worldPosition = playerTransform.position + relativePosition;
        obj.position = worldPosition;
    }
}
