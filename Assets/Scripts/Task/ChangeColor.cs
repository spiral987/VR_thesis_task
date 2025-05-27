using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    private void Start()
    {
       
    }

    private void Update()
    {
        
    }
    // 色を変更する対象のGameObjectを受け取る
    public void SetColorToGreen(GameObject targetObject, Color color)
    {
        // Rendererコンポーネントを取得
        Renderer targetRenderer = targetObject.GetComponent<Renderer>();

        if (targetRenderer != null)
        {
            // 色を緑に設定
            targetRenderer.material.color = color;
        }
        else
        {
            Debug.LogWarning($"The GameObject '{targetObject.name}' does not have a Renderer component.");
        }
    }
}
