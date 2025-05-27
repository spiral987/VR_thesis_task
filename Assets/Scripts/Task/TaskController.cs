using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;



public class TaskController : MonoBehaviour
{
    [SerializeField] private AudioSource correctSoundSource; // AudioSource���w��

    [SerializeField] private AudioClip successSound; // AudioSource���w��

    private bool isAudioPlayed = false;

    [SerializeField]
    private string _playId;
    [SerializeField]
    private List<GameObject> ghostJoints = new List<GameObject>(); // �S�[�X�g�֐߃��X�g���i�[

    [SerializeField] private List<GameObject> dollJoints=new List<GameObject>(); // ���삷��l�`�̊֐߃��X�g���i�[
    [SerializeField] private List<GameObject> dollJointBoxes=new List<GameObject>();//�F��ς���֐߃{�b�N�X�K�C�h

    [SerializeField] private GameObject GhostHip; // Ghost��hip
    [SerializeField] private GameObject GhostHip2; // Ghost2��hip
    [SerializeField] private GameObject GhostHip2Guide;//Ghost2��hip�ʒu��doll��hip�ʒu���Ȃ��ł����
    //[SerializeField] private GameObject Ghost2Parent;
    [SerializeField] private GameObject DollHip; // doll��hip

    [SerializeField] private Animator initposeanim;
    [SerializeField] private Animator grabposeanim;


    [SerializeField] private float positionTolerance = 0.1f; // �ʒu�̋��e�덷
    [SerializeField] private float rotationTolerance = 15.0f; // ��]�̋��e�덷 (�p�x)
    [SerializeField] private FindByTagExample Finder;
    [SerializeField] private ChangeGhostPose PoseChanger;
    [SerializeField] private ChangeGhostPose PoseChanger2;


    [SerializeField] private List<bool> skipPositionComparison; // �ʒu���r���Ȃ����ǂ����̃`�F�b�N�{�b�N�X
    [SerializeField] private List<bool> skipRotationComparison; // ��]���r���Ȃ����ǂ����̃`�F�b�N�{�b�N�X
    [SerializeField] private List<bool> skipSync; // Sync���Ȃ����ǂ����̃`�F�b�N�{�b�N�X

    [SerializeField] private GameObject startButton; // �J�n�{�^��
    [SerializeField] private GameObject nextPoseButton; // ���֐i�ރ{�^��

    private int currentPoseIndex = -1; // ���݂̃|�[�Y�̃C���f�b�N�X
    public int CurrentTasknum => currentPoseIndex;
    [SerializeField] private int taskNum = 2;
    private bool isMatching = false;
    private bool isPositionMatching = false;
    private bool isRotationMatching = false;

    private bool isTaskRunning = false;


    private List<Vector3> initialBoxPosition=new List<Vector3>();//doll��box�̏����ʒu
    private List<Quaternion> initialBoxRotation = new List<Quaternion>();//doll��box�̏����ʒu
    private bool isBoxSync=true;
    [SerializeField] private float tolerance = 0.01f; // box�ʒu��doll�̊֐߂ƕs��v�Ƃ݂Ȃ����e�덷

    private float overallMatchScore = 0.0f; // �S�̂̈�v�����i�[

    //Undo��Redo�̂��
    // ���O�̏�Ԃ�ۑ�����Undo�X�^�b�N
    private Stack<List<Vector3>> undoPositionStack = new Stack<List<Vector3>>();
    private Stack<List<Quaternion>> undoRotationStack = new Stack<List<Quaternion>>();

    // Undo�Ŗ߂�����Ԃ�ۑ�����Redo�X�^�b�N
    private Stack<List<Vector3>> redoPositionStack = new Stack<List<Vector3>>();
    private Stack<List<Quaternion>> redoRotationStack = new Stack<List<Quaternion>>();


    //���ԊǗ�
    [SerializeField]
    private TextMeshProUGUI startTaskTimerText; // StartTask �̑ҋ@���Ԃ�\������ TMPro
    [SerializeField]
    private TextMeshProUGUI currentPoseText; // currentposeindex

    private List<float> taskDurations = new List<float>(); // �e�^�X�N�̊������Ԃ��L�^
    private float currentTaskStartTime = -1f; // ���݂̃^�X�N�̊J�n����

    [SerializeField]
    private GrabCountManager countManager;

    [SerializeField]
    private PinchCounter _pinchCounter;

    private bool isPracticeMode = false; // ���K���[�h���ǂ���
    private int practicePoseIndex = 0; // ���݂̗��K�|�[�Y�C���f�b�N�X

    [SerializeField] private ChangeGhostPosePractice PoseChangerPractice;
    [SerializeField] private ChangeGhostPosePractice PoseChanger2Practice;
    [SerializeField] private int practicePoseNum=5;

    private void Start()
    {
        // ���֐i�ރ{�^����������ԂŔ�\����
        nextPoseButton.SetActive(false);
        //box�̏����ʒu���擾���Ă���
        initialBoxPosition =GetJointPositionsRelativeToHip();
        initialBoxRotation = GetJointRotationsRelativeToHip();



    }

    private void Update()
    {
        //Debug.Log(GhostHip2.transform.position);
        //Debug.Log(DollHip.transform.position);
        GhostHip2.transform.position = DollHip.transform.position;
        GhostHip2.transform.rotation = DollHip.transform.rotation; 
        //GhostHip2Guide.transform.position = GhostHip2.transform.position;
        //GhostHip2Guide.transform.rotation = GhostHip2.transform.rotation;

        //Ghost2Parent.transform.position = GhostHip2Guide.transform.position;
        //Ghost2Parent.transform.rotation = GhostHip2Guide.transform.rotation;




        //�|�[�Y���Z�b�g�ȊO�̂����݂񂮂ŁAbox�Ɗ֐߂̈ʒu���V���N������
        if (isBoxSync)
        {
            SyncBoxPosition();
        }
        /*
        if (Input.GetKeyDown("a"))
        {
            StartTask();
        }
        */

        if (isTaskRunning||isPracticeMode)
        {

            // ��v������s���A���ʂɉ��������������s
            isMatching = CompareTransforms();

            if (isMatching)
            {
                //�����o��
                if (!isAudioPlayed) {
                correctSoundSource.PlayOneShot(successSound);
                    isAudioPlayed = true;
                }

                // �S�Ă̊֐߂���v���Ă���ꍇ
                nextPoseButton.SetActive(true); // ���֐i�ރ{�^����\��
            }
            else
            {
                isAudioPlayed = false;
                nextPoseButton.SetActive(false); // �{�^�����\��
            }

           
        }
    }

    //public�i�{�^���ɂ������j

    //start�{�^�����������Ƃ�
    public void StartTask()
    {
        // 5�b�ҋ@���ă^�X�N���J�n����R���[�`�����Ăяo��
        StartCoroutine(StartTaskWithDelay(5.0f)); // 5�b�ҋ@
    }

    // �R���[�`���Œx���������s��
    private System.Collections.IEnumerator StartTaskWithDelay(float delaySeconds)
    {
        startButton.SetActive(false);

        if (!isPracticeMode)
        {
            isTaskRunning = true;
        }
        float timeRemaining = delaySeconds;

        // �ҋ@���Ԃ��J�E���g�_�E���`���ŕ\��
        while (timeRemaining > 0)
        {
            startTaskTimerText.text = $" {timeRemaining:F1} seconds...";
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        isBoxSync = false;
        ApplyPositionsToTarget(DollHip, dollJointBoxes, initialBoxPosition);
        ApplyJointRotationsRelativeToHip(DollHip, dollJointBoxes, initialBoxRotation);
        isBoxSync = true;

        // �^�C�}�[���\���ɂ��ă^�X�N���J�n
        startTaskTimerText.text = "Go!";
        if (isTaskRunning)
        {
            //���̃X�e�[�W�C���f�b�N�X���C���N�������g���A�v�����J�n
            currentPoseIndex++;
            currentPoseText.text = $"stage: {currentPoseIndex + 1} ";

            PoseChanger.ChangePose(currentPoseIndex);
            PoseChanger2.ChangePose(currentPoseIndex);
        }
        else if (isPracticeMode)
        {
            PoseChangerPractice.ChangePose(practicePoseIndex);
            PoseChanger2Practice.ChangePose(practicePoseIndex);
        }




        // �^�X�N�J�n�������L�^
        currentTaskStartTime = Time.time;
    }


    // �{�^�����������ۂɒx�����������s
    public void OnPressedButton()
    {
        // ���̏������J�n
        StartCoroutine(OnPressedButtonWithDelay(5.0f)); // 5�b�ҋ@
    }

    // �R���[�`���Œx���������s��
    private System.Collections.IEnumerator OnPressedButtonWithDelay(float delaySeconds)
    {

        float timeRemaining = delaySeconds;

        // �{�^�����\���ɖ߂�
        nextPoseButton.SetActive(false);

        if (currentTaskStartTime > 0)
        {
            // �^�X�N�̏I�����Ԃ��v�Z
            float taskDuration = Time.time - currentTaskStartTime;
            taskDurations.Add(taskDuration); // ���X�g�ɋL�^
            Debug.Log($"Task {currentPoseIndex + 1} completed in {taskDuration:F2} seconds.");
        }
        if (isTaskRunning)
        {
            // �^�X�N���Ԃ����O�ɕ\�����ACSV�ɃG�N�X�|�[�g
            LogTaskDurationsAndExport();
            //�͂񂾃f�[�^��export
            countManager.OnTaskFinished();
            //�s���`�f�[�^
            _pinchCounter.ExportPinchDataToCSV();

            // �^�X�N���s�񐔂ɒB������
            if (currentPoseIndex + 1 == taskNum)
            {
                startTaskTimerText.text = "finish";
                isTaskRunning = false;
                nextPoseButton.SetActive(false);

                yield break; // �R���[�`�����I��
            }
            currentPoseIndex++;
            currentPoseText.text = $"stage: {currentPoseIndex + 1} ";

        }

        int tmpIndex = currentPoseIndex;
        currentPoseIndex = -1;

        // �ҋ@���Ԃ��J�E���g�_�E���`���ŕ\��
        while (timeRemaining > 0)
        {
            startTaskTimerText.text = $"{timeRemaining:F1} seconds...";
            timeRemaining -= Time.deltaTime;
            yield return null;
        }
        currentPoseIndex=tmpIndex;
        startTaskTimerText.text = "Go!";

        if (isPracticeMode)
        {
            practicePoseIndex = (practicePoseIndex + 1) % practicePoseNum;
            currentPoseText.text = $"stage: {practicePoseIndex + 1} ";
        }

        if (isTaskRunning)
        {
            StartCoroutine(LoadNextPose()); // ���̃|�[�Y�����[�h
        }
        else if (isPracticeMode)
        {
            StartCoroutine(LoadPracticePose());
        }
        // ���̃^�X�N�̌v�����J�n
        currentTaskStartTime = Time.time;

        //���������̂���
        isAudioPlayed = false;
    }

    public void DollReset()
    {
        // ���݂�Doll�̊֐߂�������
        //grabposeanim.enabled =false;
        isBoxSync = false;
        ApplyPositionsToTarget(DollHip, dollJointBoxes, initialBoxPosition);
        ApplyJointRotationsRelativeToHip(DollHip, dollJointBoxes, initialBoxRotation);
        isBoxSync = true;
    }


    private void SyncBoxPosition()
    {
        int totalCount = dollJoints.Count;

        for (int i = 0; i < totalCount; i++)
        {
            GameObject doll = dollJoints[i];
            GameObject box = dollJointBoxes[i];
            if (!skipSync[i])
            {
                if (doll == null || box == null)
                {
                    Debug.LogWarning($"���X�g�̃C���f�b�N�X {i} �ɑΉ�����I�u�W�F�N�g���ݒ肳��Ă��܂���B");
                    continue;
                }

                // �I�u�W�F�N�g1�ƃI�u�W�F�N�g2�̈ʒu���s��v�̏ꍇ
                if (Vector3.Distance(doll.transform.position, box.transform.position) > tolerance)
                {
                    // �I�u�W�F�N�g2���I�u�W�F�N�g1�̈ʒu�Ɉړ�
                    box.transform.position = doll.transform.position;
                }
            }
        }

    }

    private bool CompareTransforms()
    {
        //if (ghostJoints.Count != dollJoints.Count || ghostJoints.Count != skipPositionComparison.Count || ghostJoints.Count != skipRotationComparison.Count)
        //{
        //    Debug.LogError("The sizes of the lists (ghost, doll, skipPositionComparison, skipRotationComparison) must match.");
        //    return false;
        //}

        int totalCount = ghostJoints.Count;
        bool allMatched = true; // ���ׂĂ̗v�f���}�b�`���O���Ă��邩�ǂ���
        float totalMatchScore = 0.0f; // �S�̂̈�v�����v

        //�֐߂��Ƃɔ�r
        for (int i = 0; i < totalCount; i++)
        {
            GameObject nowGhostJoint = ghostJoints[i];
            GameObject nowDollJoint = dollJoints[i];
            GameObject nowDollJointBox = dollJointBoxes[i];

            // nowGhostJoint, nowDollJoint �̍��W����I�u�W�F�N�g (referenceObject) ����̑��΍��W�ɕϊ�
            Vector3 relativePositionNowGhostJoint = GhostHip.transform.InverseTransformPoint(nowGhostJoint.transform.position);
            Vector3 relativePositionNowDollJoint = DollHip.transform.InverseTransformPoint(nowDollJoint.transform.position);

            // nowGhostJoint, nowDollJoint �̉�]����I�u�W�F�N�g (referenceObject) �̑��Ή�]�ɕϊ�
            Quaternion relativeRotationNowGhostJoint = Quaternion.Inverse(GhostHip.transform.rotation) * nowGhostJoint.transform.rotation;
            Quaternion relativeRotationNowDollJoint = Quaternion.Inverse(DollHip.transform.rotation) * nowDollJoint.transform.rotation;

            //Debug.Log($"Relative Ghost Position (Index {i}): " + relativePositionNowGhostJoint);
            //Debug.Log($"Relative Doll Position (Index {i}): " + relativePositionNowDollJoint);
            //Debug.Log($"Relative Ghost Rotation (Index {i}): " + relativeRotationNowGhostJoint.eulerAngles);
            //Debug.Log($"Relative Doll Rotation (Index {i}): " + relativeRotationNowDollJoint.eulerAngles);

            // ��v�����v�Z
            float matchScore = CalculateJointMatchScore(
                relativePositionNowGhostJoint,
                relativePositionNowDollJoint,
                relativeRotationNowGhostJoint,
                relativeRotationNowDollJoint,
                skipPositionComparison[i],
                skipRotationComparison[i]);

            totalMatchScore += matchScore;

            
            // ��r���\�b�h�ɃX�L�b�v�t���O��n��
            if (AreTransformsMatching(relativePositionNowGhostJoint, relativePositionNowDollJoint, relativeRotationNowGhostJoint, relativeRotationNowDollJoint, skipPositionComparison[i], skipRotationComparison[i]))
            {
                //Color darkGreen = new Color(0.0f, 0.39f, 0.0f); // #006400 �ɑΉ�����RGB�l
                ChangeChildrenColors(nowDollJointBox, Color.blue);
            }
            else
            {
                if (!isPositionMatching && isRotationMatching)
                {
                    ChangeChildrenColors(nowDollJointBox, Color.red);

                }
                else if (isPositionMatching && !isRotationMatching)
                {
                    ChangeChildrenColors(nowDollJointBox, Color.yellow);
                }
                else
                {
                    //Debug.Log($"Not matching at index {i}");
                    ChangeChildrenColors(nowDollJointBox, Color.red);
                }
                allMatched = false;
                
            }
            


        }

        // �S�̂̈�v�����v�Z
        overallMatchScore = totalMatchScore / totalCount;
        //Debug.Log($"Overall Match Score: {overallMatchScore * 100:F2}%");

        if (allMatched) 
        {
            startTaskTimerText.text = $"clear! ";
            return true;
        }
        else
        {
            startTaskTimerText.text = $"Not Matched...";
            return false;
        }

    }

    //doll��ghost�̂ЂƂ̊֐߈ʒu���r����@�ǂ����������Ă���true��Ԃ�
    private bool AreTransformsMatching(Vector3 pos1, Vector3 pos2, Quaternion rot1, Quaternion rot2, bool skipPosition, bool skipRotation)
    {

        // �ʒu�̔�r�i�X�L�b�v�t���O��false�̏ꍇ�̂݁j
        if (!skipPosition && Vector3.Distance(pos1, pos2) > positionTolerance)
        {
            isPositionMatching = false;
            //Debug.Log("Position mismatch");
        }
        else
        {
            isPositionMatching = true;
        }

        // ��]�̔�r�i�X�L�b�v�t���O��false�̏ꍇ�̂݁j
        if (!skipRotation && Quaternion.Angle(rot1, rot2) > rotationTolerance)
        {
            isRotationMatching = false;
            //Debug.Log("Rotation mismatch");
        }
        else
        {
            isRotationMatching = true;
        }

        return isPositionMatching && isRotationMatching;
  
    }


    /// <summary>
    /// �֐߂��Ƃ̈�v�����v�Z (Tolerance���Ȃ�100%)  
    /// </summary>
    private float CalculateJointMatchScore(Vector3 pos1, Vector3 pos2, Quaternion rot1, Quaternion rot2, bool skipPosition, bool skipRotation)
    {
        float positionScore = 1.0f; // �����l��100%�Ƃ���
        float rotationScore = 1.0f; // �����l��100%�Ƃ���

        // �ʒu�̈�v�����v�Z
        if (!skipPosition)
        {
            float distance = Vector3.Distance(pos1, pos2);
            if (distance <= positionTolerance)
            {
                positionScore = 1.0f; // Tolerance���Ȃ�100%
            }
            else
            {
                // Tolerance�𒴂�������������,2*Tolerance��0�p�[�Z���g
                positionScore = Mathf.Clamp01(1.0f - (distance - positionTolerance) / (positionTolerance * 2));
            }
        }

        // ��]�̈�v�����v�Z
        if (!skipRotation)
        {
            float angleDifference = Quaternion.Angle(rot1, rot2);
            if (angleDifference <= rotationTolerance)
            {
                rotationScore = 1.0f; // Tolerance���Ȃ�100%
            }
            else
            {
                // Tolerance�𒴂�������������
                rotationScore = Mathf.Clamp01(1.0f - (angleDifference - rotationTolerance) / (rotationTolerance * 2));
            }
        }

        // �ʒu�Ɖ�]�̈�v���̕��ς�Ԃ�
        return (positionScore + rotationScore) / 2.0f;
    }





    //���̃|�[�Y�����[�h���Adoll�̃|�[�Y��T�ɖ߂�
    private System.Collections.IEnumerator LoadNextPose()
    {

        // ���݂�Doll�̊֐߂�������
        //grabposeanim.enabled =false;
        isBoxSync = false;
        ApplyPositionsToTarget(DollHip, dollJointBoxes, initialBoxPosition);
        ApplyJointRotationsRelativeToHip(DollHip, dollJointBoxes, initialBoxRotation);
        isBoxSync = true;
        //initposeanim.enabled=true;
        //yield return new WaitForSeconds(1.0f); // �w��b���ҋ@
        //grabposeanim.enabled = true;
        //initposeanim.enabled=false;
        PoseChanger.ChangePose(currentPoseIndex);
        PoseChanger2.ChangePose(currentPoseIndex);


        Debug.Log($"init doll and load next pose");
        yield return null;
    }

    //���̃|�[�Y�����[�h���Adoll�̃|�[�Y��T�ɖ߂�
    private System.Collections.IEnumerator LoadPracticePose()
    {

        // ���݂�Doll�̊֐߂�������
        //grabposeanim.enabled =false;
        isBoxSync = false;
        ApplyPositionsToTarget(DollHip, dollJointBoxes, initialBoxPosition);
        ApplyJointRotationsRelativeToHip(DollHip, dollJointBoxes, initialBoxRotation);
        isBoxSync = true;
        //initposeanim.enabled=true;
        //yield return new WaitForSeconds(1.0f); // �w��b���ҋ@
        //grabposeanim.enabled = true;
        //initposeanim.enabled=false;
        PoseChangerPractice.ChangePose(practicePoseIndex);
        PoseChanger2Practice.ChangePose(practicePoseIndex);

        Debug.Log($"init doll and load next pose");
        yield return null;
    }


    //Doll�̏���Box�ʒu���擾
    List<Vector3> GetJointPositionsRelativeToHip()
    {
        List<Vector3> positions = new List<Vector3>();

        foreach (GameObject joint in dollJointBoxes)
        {
            if (joint == null) continue;

            // Hip����Ƃ������[�J�����W
            Vector3 relativePosition = DollHip.transform.InverseTransformPoint(joint.transform.position);
            positions.Add(relativePosition);
        }

        return positions;
    }

    //Doll�̏���Box1��]���擾
    List<Quaternion> GetJointRotationsRelativeToHip()
    {
        List<Quaternion> rotations = new List<Quaternion>();

        foreach (GameObject joint in dollJointBoxes)
        {
            if (joint == null) continue;

            // Hip����Ƃ������[�J����]
            Quaternion relativeRotation = Quaternion.Inverse(DollHip.transform.rotation) * joint.transform.rotation;
            rotations.Add(relativeRotation);
        }

        return rotations;
    }




    //����Box�ʒu��K�p
    void ApplyPositionsToTarget(GameObject targetHip, List<GameObject> targetJoints, List<Vector3> localPositions)
    {
        for (int i = 0; i < targetJoints.Count; i++)
        {
            if (targetJoints[i] == null) continue;

            // ���[�J�����W�����[���h���W�ɕϊ����ēK�p
            Vector3 worldPosition = targetHip.transform.TransformPoint(localPositions[i]);
            targetJoints[i].transform.position = worldPosition;
        }
    }

    //������]�ʒu��K�p
    void ApplyJointRotationsRelativeToHip(GameObject targetHip, List<GameObject> targetJointBoxes, List<Quaternion> relativeRotations)
    {
        if (targetJointBoxes == null || relativeRotations == null || targetJointBoxes.Count != relativeRotations.Count)
        {
            Debug.LogError("���X�g���������ݒ肳��Ă��Ȃ����A�v�f������v���Ă��܂���B");
            return;
        }

        for (int i = 0; i < targetJointBoxes.Count; i++)
        {
            GameObject joint = targetJointBoxes[i];

            if (joint == null) continue;

            // Hip����Ƃ������[�J����]�����[���h��]�ɕϊ����ēK�p
            joint.transform.rotation = targetHip.transform.rotation * relativeRotations[i];
        }
    }

    //�|�[�Y�̐���ɉ�����target��visual�̐F��ς���
    void ChangeChildrenColors(GameObject parent, Color color)
    {
        // �e�I�u�W�F�N�g��Transform���擾
        Transform parentTransform = parent.transform;

        // �S�Ă̎q�I�u�W�F�N�g�����[�v����
        foreach (Transform child in parentTransform)
        {
            // Renderer�R���|�[�l���g���擾
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                // �F��ύX
                renderer.material.color = color;
            }
        }
    }

    private void LogTaskDurations()
    {
        Debug.Log("Task durations:");
        for (int i = 0; i < taskDurations.Count; i++)
        {
            Debug.Log($"Task {i + 1}: {taskDurations[i]:F2} seconds");
        }
    }


    public void LogTaskDurationsAndExport()
    {
        // ���݂̃V�[�����擾
        Scene currentScene = SceneManager.GetActiveScene();

        // �V�[�������擾
        string sceneName = currentScene.name;

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // �R���\�[���Ƀ^�X�N���Ԃ����O�o��
        LogTaskDurations();

        // CSV���G�N�X�|�[�g
        string filePath = Path.Combine(Application.persistentDataPath, $"{timestamp}_{_playId}_{sceneName}_TaskDurations.csv");
        ExportTaskDurationsToCSV(filePath);
    }


    private void ExportTaskDurationsToCSV(string filePath)
    {
        // CSV�̃w�b�_�[
        string csvContent = "Task,Duration (seconds)\n";



        // �^�X�N���Ƃ̃f�[�^��ǉ�
        for (int i = 0; i < taskDurations.Count; i++)
        {
            csvContent += $"{i + 1},{taskDurations[i]:F2}\n";
        }

        try
        {
            // �t�@�C���ɏ����o��
            File.WriteAllText(filePath, csvContent);
            Debug.Log($"Task durations successfully exported to: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to write CSV: {e.Message}");
        }
    }

    // �^�X�N�ԍ�����J�n����
    public void StartTaskFromIndex(int taskIndex)
    {
        if (taskIndex < 0 || taskIndex >= taskNum)
        {
            Debug.LogError($"Invalid task index: {taskIndex}. Must be between 0 and {taskNum - 1}.");
            return;
        }

        // �^�X�N�ԍ���ݒ肵�ă^�X�N���J�n
        currentPoseIndex = taskIndex - 1; // StartTask��+1����邽��
        StartTask();
    }

    public void ResetScene()
    {
        // ���݂̃V�[���������[�h
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Debug.Log($"Scene {currentScene.name} has been reset.");
    }

    // ���K���[�h���J�n
    public void StartPracticeMode()
    {
        isPracticeMode = true;
        practicePoseIndex = 0;
        Debug.Log("Practice mode started.");
    }



}

