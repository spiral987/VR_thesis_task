using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class buttontext : MonoBehaviour
{
    [SerializeField]
    private TMP_Text tmpro;

    [SerializeField]
    private PositionConstraintController constraintcontroller;

    void Update()
    {
        tmpro.text = "jointLock:"+constraintcontroller.IsJointConstrained;
    }
}
