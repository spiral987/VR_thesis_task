using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FindByTagExample : MonoBehaviour
{
    private GameObject targetObject;//見つけたオブジェクトを格納
    public void findTargetTags(GameObject Body, List<GameObject> GhostJoints, string tag)
    {
        // タグが "TargetTag" のすべてのオブジェクトを取得
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in taggedObjects)
        {
            // Grabbable Bot の子オブジェクトかどうかを確認
            if (obj.transform.IsChildOf(Body.transform))
            {
                targetObject = obj;
                GhostJoints.Add(targetObject);
                Debug.Log("Found target with tag: " + targetObject.name);

                break;
            }
        }

        if (targetObject == null)
        {
            Debug.Log("No object with the specified tag found among children.");
        }
    }

}

