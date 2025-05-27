using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FindByTagExample : MonoBehaviour
{
    private GameObject targetObject;//�������I�u�W�F�N�g���i�[
    public void findTargetTags(GameObject Body, List<GameObject> GhostJoints, string tag)
    {
        // �^�O�� "TargetTag" �̂��ׂẴI�u�W�F�N�g���擾
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in taggedObjects)
        {
            // Grabbable Bot �̎q�I�u�W�F�N�g���ǂ������m�F
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

