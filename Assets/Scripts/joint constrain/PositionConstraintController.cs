using Oculus.Interaction;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PositionConstraintController : MonoBehaviour
{
    [SerializeField]
    private List<MyGrabFree> myGrabFreeList=new List<MyGrabFree>(); // MyGrabFree �R���|�[�l���g�̎Q��

    private bool isConstrain = false;

    public bool IsJointConstrained {  get { return isConstrain; } }�@//�O������Q��

    void Start()
    {
        int totalCount = myGrabFreeList.Count;
        for (int i = 0; i < totalCount; i++)
        {
            // �����ݒ�: X���̂݌Œ�
            if (myGrabFreeList[i] != null)
            {
                myGrabFreeList[i].ConstrainXAxis = false;
                myGrabFreeList[i].ConstrainYAxis = false;
                myGrabFreeList[i].ConstrainZAxis = false;
            }
        }
    }

    void Update()
    {
        /* �e�X�g�p: �L�[�{�[�h���͂Ŏ��̌Œ��؂�ւ�
        if (Input.GetKeyDown(KeyCode.X))
        {
            myGrabFree.ConstrainXAxis = !myGrabFree.ConstrainXAxis;
            Debug.Log($"ConstrainXAxis: {myGrabFree.ConstrainXAxis}");
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            myGrabFree.ConstrainYAxis = !myGrabFree.ConstrainYAxis;
            Debug.Log($"ConstrainYAxis: {myGrabFree.ConstrainYAxis}");
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            myGrabFree.ConstrainZAxis = !myGrabFree.ConstrainZAxis;
            Debug.Log($"ConstrainZAxis: {myGrabFree.ConstrainZAxis}");
        }
        */
    }

    public void ToggleConstrain()
    {
        int totalCount=myGrabFreeList.Count;
        for (int i = 0; i < totalCount; i++)
        {   

            myGrabFreeList[i].ConstrainXAxis = !myGrabFreeList[i].ConstrainXAxis;
            myGrabFreeList[i].ConstrainYAxis = !myGrabFreeList[i].ConstrainYAxis;
            myGrabFreeList[i].ConstrainZAxis = !myGrabFreeList[i].ConstrainZAxis;
            isConstrain=myGrabFreeList[i].ConstrainXAxis;
        }
    }

    public void FalseConstrain()
    {
        int totalCount = myGrabFreeList.Count;
        for (int i = 0; i < totalCount; i++)
        {

            myGrabFreeList[i].ConstrainXAxis = false;
            myGrabFreeList[i].ConstrainYAxis = false;
            myGrabFreeList[i].ConstrainZAxis = false;
            isConstrain = myGrabFreeList[i].ConstrainXAxis;
        }
    }

}
