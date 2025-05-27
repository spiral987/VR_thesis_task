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
    // �F��ύX����Ώۂ�GameObject���󂯎��
    public void SetColorToGreen(GameObject targetObject, Color color)
    {
        // Renderer�R���|�[�l���g���擾
        Renderer targetRenderer = targetObject.GetComponent<Renderer>();

        if (targetRenderer != null)
        {
            // �F��΂ɐݒ�
            targetRenderer.material.color = color;
        }
        else
        {
            Debug.LogWarning($"The GameObject '{targetObject.name}' does not have a Renderer component.");
        }
    }
}
