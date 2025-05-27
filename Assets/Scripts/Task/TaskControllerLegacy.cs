using System.Collections.Generic;
using UnityEngine;



public class TaskControllerLegacy : MonoBehaviour
{
    //�S�[�X�g�|�[�Y��10�i�[���邽��
    [SerializeField]
    private List<ChildList> ghostJoints = new List<ChildList>(); // �S�[�X�g�֐߃��X�g���i�[

    [System.Serializable]
    class ChildList
    {
        public List<GameObject> joint = new List<GameObject>();
    }

    [SerializeField] private List<GameObject> ghost;

    [SerializeField] private List<GameObject> dollJoints; // ���삷��l�`�̊֐߃��X�g���i�[

    [SerializeField] private List<GameObject> GhostHip = new List<GameObject>(); // Ghost��hip
    [SerializeField] private GameObject DollHip; // doll��hip

    [SerializeField] private float positionTolerance = 0.1f; // �ʒu�̋��e�덷
    [SerializeField] private float rotationTolerance = 15.0f; // ��]�̋��e�덷 (�p�x)
    [SerializeField] private ChangeColor Changer;
    [SerializeField] private FindByTagExample Finder;

    [SerializeField] private List<bool> skipPositionComparison; // �ʒu���r���Ȃ����ǂ����̃`�F�b�N�{�b�N�X
    [SerializeField] private List<bool> skipRotationComparison; // ��]���r���Ȃ����ǂ����̃`�F�b�N�{�b�N�X

    [SerializeField] private GameObject nextPoseButton; // ���֐i�ރ{�^��

    private int currentPoseIndex = 0; // ���݂̃|�[�Y�̃C���f�b�N�X
    [SerializeField] private int taskNum = 1;
    private bool isMatching = false;

    private bool isTaskRunning = false;

    public void StartTask()
    {
        isTaskRunning = true;
        // ���֐i�ރ{�^����������ԂŔ�\����
        nextPoseButton.SetActive(false);
        //�֐߂̖��̃��X�g�����A���ԂɌ������ă��X�g�Ɋi�[����
        List<string> jointNames = new List<string> { "LeftArm", "LeftForeArm" };

        //Ghost�̊֐߂�Ghost���X�g�Ɋi�[
        for (int i = 0; i < 2; i++)
        {
            foreach (var jointName in jointNames)
            {
                Finder.findTargetTags(ghost[i], ghostJoints[i].joint, jointName);
            }
        }

    }

    private void Update()
    {
        if (isTaskRunning)
        {

            // ��v������s���A���ʂɉ��������������s
            isMatching = CompareTransforms();

            if (isMatching)
            {
                // �S�Ă̊֐߂���v���Ă���ꍇ
                nextPoseButton.SetActive(true); // ���֐i�ރ{�^����\��
            }
            else
            {
                nextPoseButton.SetActive(false); // �{�^�����\��
            }
        }
    }

    public bool CompareTransforms()
    {
        //if (ghostJoints.Count != dollJoints.Count || ghostJoints.Count != skipPositionComparison.Count || ghostJoints.Count != skipRotationComparison.Count)
        //{
        //    Debug.LogError("The sizes of the lists (ghost, doll, skipPositionComparison, skipRotationComparison) must match.");
        //    return false;
        //}

        int totalCount = ghostJoints.Count;
        bool allMatched = true; // ���ׂĂ̗v�f���}�b�`���O���Ă��邩�ǂ���

        for (int i = 0; i < totalCount; i++)
        {
            GameObject nowGhostJoint = ghostJoints[currentPoseIndex].joint[i];
            GameObject nowDollJoint = dollJoints[i];

            // nowGhostJoint, nowDollJoint �̍��W����I�u�W�F�N�g (referenceObject) ����̑��΍��W�ɕϊ�
            Vector3 relativePositionNowGhostJoint = GhostHip[currentPoseIndex].transform.InverseTransformPoint(nowGhostJoint.transform.position);
            Vector3 relativePositionNowDollJoint = DollHip.transform.InverseTransformPoint(nowDollJoint.transform.position);

            // nowGhostJoint, nowDollJoint �̉�]����I�u�W�F�N�g (referenceObject) �̑��Ή�]�ɕϊ�
            Quaternion relativeRotationNowGhostJoint = Quaternion.Inverse(GhostHip[currentPoseIndex].transform.rotation) * nowGhostJoint.transform.rotation;
            Quaternion relativeRotationNowDollJoint = Quaternion.Inverse(DollHip.transform.rotation) * nowDollJoint.transform.rotation;

            Debug.Log($"Relative Ghost Position (Index {i}): " + relativePositionNowGhostJoint);
            Debug.Log($"Relative Doll Position (Index {i}): " + relativePositionNowDollJoint);
            Debug.Log($"Relative Ghost Rotation (Index {i}): " + relativeRotationNowGhostJoint.eulerAngles);
            Debug.Log($"Relative Doll Rotation (Index {i}): " + relativeRotationNowDollJoint.eulerAngles);

            // ��r���\�b�h�ɃX�L�b�v�t���O��n��
            if (AreTransformsMatching(relativePositionNowGhostJoint, relativePositionNowDollJoint, relativeRotationNowGhostJoint, relativeRotationNowDollJoint, skipPositionComparison[i], skipRotationComparison[i]))
            {
                Debug.Log($"Matching at index {i}");
                Changer.SetColorToGreen(nowDollJoint, Color.green);
            }
            else
            {
                Debug.Log($"Not matching at index {i}");
                Changer.SetColorToGreen(nowDollJoint, Color.red);
                allMatched = false;
            }


        }
        if (allMatched)
        {
            Debug.Log("All elements matched! Showing the Next button.");
            return true;
        }
        else
        {
            Debug.Log("Not all elements matched.");
            return false;
        }

    }

    private bool AreTransformsMatching(Vector3 pos1, Vector3 pos2, Quaternion rot1, Quaternion rot2, bool skipPosition, bool skipRotation)
    {
        // �ʒu�̔�r�i�X�L�b�v�t���O��false�̏ꍇ�̂݁j
        if (!skipPosition && Vector3.Distance(pos1, pos2) > positionTolerance)
        {
            Debug.Log("Position mismatch");
            return false;
        }

        // ��]�̔�r�i�X�L�b�v�t���O��false�̏ꍇ�̂݁j
        if (!skipRotation && Quaternion.Angle(rot1, rot2) > rotationTolerance)
        {
            Debug.Log("Rotation mismatch");
            return false;
        }

        return true;
    }

    public void OnPressedButton()
    {
        //�^�X�N���s�񐔂ɒB������
        if (currentPoseIndex + 1 > taskNum)
        {
            Debug.Log("task finished.");
            isTaskRunning = false;
            return;
        }
        ghost[currentPoseIndex].SetActive(false);
        ghost[currentPoseIndex + 1].SetActive(true);
        LoadNextPose(ghostJoints[currentPoseIndex + 1].joint);
    }
    public void LoadNextPose(List<GameObject> newGhostJoints)
    {

        // ���݂�Doll���������i�K�v�ɉ����Ď����j
        foreach (var joint in dollJoints)
        {
            Changer.SetColorToGreen(joint.gameObject, Color.white); // �F�����Z�b�g
        }

        // �{�^�����\���ɖ߂�
        nextPoseButton.SetActive(false);

        Debug.Log($"���̃|�[�Y {++currentPoseIndex} �����[�h���܂����B");
    }
}

