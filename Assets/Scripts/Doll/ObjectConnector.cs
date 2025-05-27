using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ObjectConnector : MonoBehaviour
{
    [SerializeField]
    private Transform objectA; // オブジェクトA
    [SerializeField]
    private Transform objectB; // オブジェクトB

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // ラインの設定（必要に応じてカスタマイズ）
        lineRenderer.startWidth = 0.003f; // 線の開始時の太さ
        lineRenderer.endWidth = 0.003f;   // 線の終了時の太さ
        lineRenderer.positionCount = 2; // ラインの点の数
    }

    private void Update()
    {
        if (objectA != null && objectB != null)
        {
            // ラインの両端をオブジェクトの位置に設定
            lineRenderer.SetPosition(0, objectA.position);
            lineRenderer.SetPosition(1, objectB.position);
        }
    }
}
