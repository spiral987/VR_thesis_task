using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using Unity.VisualScripting;
using Oculus.Interaction.Input;
using System.IO;
using System.Threading.Tasks;
using static GrabCountManager;
using UnityEngine.SceneManagement;
using System.Linq;
//using UnityEditor.PackageManager.Requests; // �񓯊������ɕK�v


public class GrabCountManager : MonoBehaviour
{
    [SerializeField]
    private string _playId;

    //[SerializeField]
    //private List<GrabCountTracker> _trackers = new List<GrabCountTracker>();

    [SerializeField]
    private TaskController _taskController;

    //����ƉE��̈ʒu
    [SerializeField]
    private Transform _leftHandTransform;
    [SerializeField]
    private Transform _rightHandTransform;

    //�L�^�������l�E����ƉE��
    private int[,] _grabCounts; // �񎟌��z��: [�^�X�N�X�e�[�W, �֐�]
    private int[,] _leftGrabCounts; // ����Œ͂񂾉�: [�^�X�N�X�e�[�W, �֐�]
    private int[,] _rightGrabCounts; // �E��Œ͂񂾉�: [�^�X�N�X�e�[�W, �֐�]

    private float[,] _grabTimes; // �͂�ł��鎞��: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _leftGrabTimes; // ����Œ͂�ł��鍇�v����: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _rightGrabTimes; // �E��Œ͂�ł��鍇�v����: [�^�X�N�X�e�[�W, �֐�]

    private float[,] _movedDistances; // ������������: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _leftMovedDistances; // ������������: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _rightMovedDistances; // ������������: [�^�X�N�X�e�[�W, �֐�]

    // ��]�p�x�̗݌v���L�^
    private float[,] _totalRotations; // �݌v��]�p�x: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _leftTotalRotations; // ����ł̗݌v��]�p�x: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _rightTotalRotations; // �E��ł̗݌v��]�p�x: [�^�X�N�X�e�[�W, �֐�]

    private float[,] _totalGrabDistancesLeft; // �ݐς̎�ƃI�u�W�F�N�g�̋���: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _averageGrabDistancesLeft; // ���ς̎�ƃI�u�W�F�N�g�̋���: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _totalGrabDistancesRight; // �ݐς̎�ƃI�u�W�F�N�g�̋���: [�^�X�N�X�e�[�W, �֐�]
    private float[,] _averageGrabDistancesRight; // ���ς̎�ƃI�u�W�F�N�g�̋���: [�^�X�N�X�e�[�W, �֐�]


    //Grab�J�n���ɋL�^����ϐ�
    private float[] _grabStartTimes; // �e�֐߂̒͂݊J�n���Ԃ��L�^
    private float[] _leftGrabStartTimes; // L�e�֐߂̒͂݊J�n���Ԃ��L�^
    private float[] _rightGrabStartTimes; // R�e�֐߂̒͂݊J�n���Ԃ��L�^

    private Vector3[] _grabStartPositions; // �e�֐߂̒͂݊J�n�ʒu���L�^
    private Vector3[] _leftGrabStartPositions; // �e�֐߂̒͂݊J�n�ʒu���L�^
    private Vector3[] _rightGrabStartPositions; // �e�֐߂̒͂݊J�n�ʒu���L�^

    private Quaternion[] _grabStartRotations; // �e�֐߂̒͂݊J�n���̉�]
    private Quaternion[] _leftGrabStartRotations; // ����̒͂݊J�n���̉�]
    private Quaternion[] _rightGrabStartRotations; // �E��̒͂݊J�n���̉�]

    [SerializeField]
    private int _taskCount = 10;//�^�X�N�̌�
    [SerializeField]
    private int _jointCount = 15; // �W���C���g�̐�

    private bool? isLeftHand = null;

    // Grab ���N�G�X�g�̃N���X
    public class GrabRequest
    {
        public int TaskIndex { get; set; }
        public int JointIndex { get; set; }
        public bool? IsLeftHand { get; set; }
        public Vector3 JointPosition { get; set; }
        public Quaternion JointRotation { get; set; }
        public Vector3 StartPosition { get; set; }
        public Quaternion StartRotation { get; set; }
        public float StartTime { get; set; }
        public string RequestId { get; set; } // ���N�G�X�g����ӂɎ��ʂ���ID
    }
    // ����p�̕ʁX�̃L���[��p��
    private Queue<GrabRequest> _leftHandGrabRequests = new Queue<GrabRequest>();
    private Queue<GrabRequest> _rightHandGrabRequests = new Queue<GrabRequest>();

    // ���ݏ������̃��N�G�X�g��ǐ�
    private Dictionary<int, GrabRequest> _activeGrabRequests = new Dictionary<int, GrabRequest>();

    // Transform �� GrabRequest ���֘A�t���邽�߂� Dictionary ��ǉ�
    private Dictionary<Transform, GrabRequest> _jointToRequestMap = new Dictionary<Transform, GrabRequest>();

    // GrabState�N���X���C��
    private class GrabState
    {
        public bool IsGrabbing;
        public Vector3 LastPosition = Vector3.zero;
        public Quaternion LastRotation = Quaternion.identity;
        public bool IsInitialized = false;
    }

    private Dictionary<int, GrabState> _grabStates; // �e�W���C���g�̏��


    public void IsLeftHand(bool isLeftHand)
    {
        // �ŐV�̃A�N�e�B�u�ȃ��N�G�X�g��T���Ď�̏���ݒ�
        var activeRequest = _activeGrabRequests.Values.OrderByDescending(r => r.StartTime).FirstOrDefault();

        if (activeRequest != null && activeRequest.IsLeftHand == null)
        {
            activeRequest.IsLeftHand = isLeftHand;

            // �K�؂ȃL���[�ɐU�蕪��
            if (isLeftHand)
            {
                _leftHandGrabRequests.Enqueue(activeRequest);
            }
            else
            {
                _rightHandGrabRequests.Enqueue(activeRequest);
            }

            Debug.Log($"Hand assignment set for Joint {activeRequest.JointIndex}: {(isLeftHand ? "Left" : "Right")}");
        }
        else
        {
            Debug.LogWarning("No pending grab requests or hand already assigned.");
        }
    }



    /// <summary>
    /// GrabCountManager�̏�����
    /// </summary>
    void Start()
    {
        _grabStates = new Dictionary<int, GrabState>();
        for (int i = 0; i < _jointCount; i++)
        {
            _grabStates[i] = new GrabState { IsGrabbing = false, LastPosition = Vector3.zero, LastRotation = Quaternion.identity };
        }

        _grabCounts = new int[_taskCount, _jointCount];
        _leftGrabCounts = new int[_taskCount, _jointCount];
        _rightGrabCounts = new int[_taskCount, _jointCount];

        _grabTimes = new float[_taskCount, _jointCount];
        _leftGrabTimes = new float[_taskCount, _jointCount];
        _rightGrabTimes = new float[_taskCount, _jointCount];

        _movedDistances = new float[_taskCount, _jointCount];
        _leftMovedDistances = new float[_taskCount, _jointCount];
        _rightMovedDistances = new float[_taskCount, _jointCount];

        _totalRotations = new float[_taskCount, _jointCount];
        _leftTotalRotations = new float[_taskCount, _jointCount];
        _rightTotalRotations = new float[_taskCount, _jointCount];

        _totalGrabDistancesLeft = new float[_taskCount, _jointCount];
        _averageGrabDistancesLeft = new float[_taskCount, _jointCount];
        _totalGrabDistancesRight = new float[_taskCount, _jointCount];
        _averageGrabDistancesRight = new float[_taskCount, _jointCount];



        _grabStartTimes = new float[_jointCount];
        _leftGrabStartTimes = new float[_jointCount];
        _rightGrabStartTimes = new float[_jointCount];

        _grabStartPositions = new Vector3[_jointCount];
        _leftGrabStartPositions = new Vector3[_jointCount];
        _rightGrabStartPositions = new Vector3[_jointCount];

        _grabStartRotations = new Quaternion[_jointCount];
        _leftGrabStartRotations = new Quaternion[_jointCount];
        _rightGrabStartRotations = new Quaternion[_jointCount];



        for (int i = 0; i < _jointCount; i++)
        {
            _grabStartTimes[i] = -1f; // �͂݊J�n���Ԃ𖳌��Ȓl�ŏ�����
            _grabStartPositions[i] = Vector3.zero; // �͂݊J�n�ʒu��������
        }

        for (int i = 0; i < _jointCount; i++)
        {
            _grabStartRotations[i] = Quaternion.identity;
            _leftGrabStartRotations[i] = Quaternion.identity;
            _rightGrabStartRotations[i] = Quaternion.identity;
        }

    }


    public void Calculatedistance(Transform joint)
    {
        // ������ joint �ɑΉ����� Request �����݂��邩�m�F
        if (!_jointToRequestMap.TryGetValue(joint, out var request))
        {
            return;  // �Ή����� Request ��������Ώ������X�L�b�v
        }

        int jointIndex = request.JointIndex;
        int taskIndex = request.TaskIndex;
        var state = _grabStates[jointIndex];

        if (!state.IsGrabbing)
        {
            return;
        }

        // �ŏ��̈ʒu�Ɖ�]���L�^
        if (state.LastPosition == Vector3.zero)
        {
            state.LastPosition = joint.position;
            state.LastRotation = joint.rotation;
            return;
        }

        // ���݂̈ʒu�Ɖ�]���擾
        Vector3 currentPosition = joint.position;
        Quaternion currentRotation = joint.rotation;

        // �ړ��������v�Z�i�O�t���[������̍����j
        float distance = Vector3.Distance(state.LastPosition, currentPosition);
        if (distance > 0.0001f)
        {
            _movedDistances[taskIndex, jointIndex] += distance;
            Debug.Log($"Joint {jointIndex} moved: {distance:F4} units. Total: {_movedDistances[taskIndex, jointIndex]:F4}");
        }

        // ��]�p�x���v�Z�i�O�t���[������̍����j
        float rotationAngle = Quaternion.Angle(state.LastRotation, currentRotation);
        if (rotationAngle > 0.01f)
        {
            _totalRotations[taskIndex, jointIndex] += rotationAngle;
            Debug.Log($"Joint {jointIndex} rotated: {rotationAngle:F4} degrees. Total: {_totalRotations[taskIndex, jointIndex]:F4}");
        }

        // ����/�E��̔���Ɋ�Â��ċ����Ɖ�]���L�^
        if (request.IsLeftHand == true)
        {
            if (distance > 0.0001f)
            {
                _leftMovedDistances[taskIndex, jointIndex] += distance;
            }
            if (rotationAngle > 0.01f)
            {
                _leftTotalRotations[taskIndex, jointIndex] += rotationAngle;
            }

            float leftHandDistance = Vector3.Distance(_leftHandTransform.position, currentPosition);
            _totalGrabDistancesLeft[taskIndex, jointIndex] += leftHandDistance;
        }
        else if (request.IsLeftHand == false)
        {
            if (distance > 0.0001f)
            {
                _rightMovedDistances[taskIndex, jointIndex] += distance;
            }
            if (rotationAngle > 0.01f)
            {
                _rightTotalRotations[taskIndex, jointIndex] += rotationAngle;
            }

            float rightHandDistance = Vector3.Distance(_rightHandTransform.position, currentPosition);
            _totalGrabDistancesRight[taskIndex, jointIndex] += rightHandDistance;
        }

        // ��Ԃ��X�V
        state.LastPosition = currentPosition;
        state.LastRotation = currentRotation;
    }


    // StartGrab���\�b�h���X�V
    public void StartGrab(int jointIndex, Vector3 jointPosition, Quaternion jointRotation, Transform joint)
    {
        int taskIndex = _taskController.CurrentTasknum;
        if (taskIndex < 0 || taskIndex >= _taskCount || jointIndex < 0 || jointIndex >= _jointCount)
        {
            Debug.LogError("Invalid task or joint index.");
            return;
        }

        var state = _grabStates[jointIndex];
        state.IsGrabbing = true;
        state.LastPosition = jointPosition;  // �����ʒu��ݒ�
        state.LastRotation = jointRotation;  // ������]��ݒ�
        state.IsInitialized = true;

        string requestId = System.Guid.NewGuid().ToString();

        var grabRequest = new GrabRequest
        {
            TaskIndex = taskIndex,
            JointIndex = jointIndex,
            JointPosition = jointPosition,
            JointRotation = jointRotation,
            StartPosition = jointPosition,  // StartPosition���������ݒ�
            StartRotation = jointRotation,  // StartRotation���������ݒ�
            StartTime = Time.time,
            RequestId = requestId
        };

    _activeGrabRequests[jointIndex] = grabRequest;
    _jointToRequestMap[joint] = grabRequest;  // Transform �� Request ���֘A�t��
    StartCoroutine(WaitForHandAssignment(grabRequest));

        // �f�o�b�O�p
        //Debug.Log($"Started grab for joint {jointIndex} at position {jointPosition}");
    }



    /// <summary>
    /// �͂މ񐔂��L�^
    /// </summary>
    private System.Collections.IEnumerator WaitForHandAssignment(GrabRequest grabRequest)
    {
        // isLeftHand ����������܂őҋ@
        while (grabRequest.IsLeftHand == null)
        {
            yield return null; // ���t���[���ҋ@
        }

        //�J�E���g
        _grabCounts[grabRequest.TaskIndex, grabRequest.JointIndex]++;

        //����
        //_grabStartTimes[grabRequest.JointIndex] = time; // �͂݊J�n���Ԃ��L�^

        //����
        //_grabStartPositions[grabRequest.JointIndex] = grabRequest.JointPosition; // �͂݊J�n�ʒu���L�^
        grabRequest.StartPosition = grabRequest.JointPosition;

        // ��]�̋L�^�J�n
        //_grabStartRotations[grabRequest.JointIndex] = grabRequest.JointRotation;
        grabRequest.StartRotation = grabRequest.JointRotation;

        if (grabRequest.IsLeftHand == true)
        {
            _leftGrabCounts[grabRequest.TaskIndex, grabRequest.JointIndex]++;
            _leftGrabStartPositions[grabRequest.JointIndex] = grabRequest.JointPosition;
            _leftGrabStartRotations[grabRequest.JointIndex] = grabRequest.JointRotation;

            float leftHandDistance = Vector3.Distance(_leftHandTransform.position, grabRequest.JointPosition);
            _totalGrabDistancesLeft[grabRequest.TaskIndex, grabRequest.JointIndex] += leftHandDistance;

            // ���ς̌v�Z���C��
            if (_leftGrabCounts[grabRequest.TaskIndex, grabRequest.JointIndex] > 0)
            {
                _averageGrabDistancesLeft[grabRequest.TaskIndex, grabRequest.JointIndex] =
                    _totalGrabDistancesLeft[grabRequest.TaskIndex, grabRequest.JointIndex] /
                    _leftGrabCounts[grabRequest.TaskIndex, grabRequest.JointIndex];
            }
        }
        else if (grabRequest.IsLeftHand == false)
        {
            _rightGrabCounts[grabRequest.TaskIndex, grabRequest.JointIndex]++;
            _rightGrabStartPositions[grabRequest.JointIndex] = grabRequest.JointPosition;
            _rightGrabStartRotations[grabRequest.JointIndex] = grabRequest.JointRotation;

            float rightHandDistance = Vector3.Distance(_rightHandTransform.position, grabRequest.JointPosition);
            _totalGrabDistancesRight[grabRequest.TaskIndex, grabRequest.JointIndex] += rightHandDistance;

            // ���ς̌v�Z���C��
            if (_rightGrabCounts[grabRequest.TaskIndex, grabRequest.JointIndex] > 0)
            {
                _averageGrabDistancesRight[grabRequest.TaskIndex, grabRequest.JointIndex] =
                    _totalGrabDistancesRight[grabRequest.TaskIndex, grabRequest.JointIndex] /
                    _rightGrabCounts[grabRequest.TaskIndex, grabRequest.JointIndex];
            }
        }


    }

    /*private void StartRightGrab(int jointIndex, Vector3 jointPosition, Quaternion jointRotation)
    {
        int _currentStage = _taskController.CurrentTasknum; // ���݂̃^�X�N�X�e�[�W

        if (jointIndex < 0 || jointIndex >= _jointCount || _currentStage < 0)
        {
            Debug.LogError("Invalid joint and task index.");
            return;
        }

        //�J�E���g
        _grabCounts[_currentStage, jointIndex]++;

        //����
        float time = Time.time;
        _grabStartTimes[jointIndex] = time; // �͂݊J�n���Ԃ��L�^

        //����
        _grabStartPositions[jointIndex] = jointPosition; // �͂݊J�n�ʒu���L�^

        // ��]�̋L�^�J�n
        _grabStartRotations[jointIndex] = jointRotation;

        if (isLeftHand == true)
        {
            _leftGrabCounts[_currentStage, jointIndex]++;
            _leftGrabStartTimes[jointIndex] = time;
            _leftGrabStartPositions[jointIndex] = jointPosition;
            _leftGrabStartRotations[jointIndex] = jointRotation;

            float leftHandDistance = Vector3.Distance(_leftHandTransform.position, jointPosition);
            _totalGrabDistancesLeft[_currentStage, jointIndex] += leftHandDistance;
            _averageGrabDistancesLeft[_currentStage, jointIndex] = _totalGrabDistancesLeft[_currentStage, jointIndex] / _leftGrabCounts[_currentStage, jointIndex];

        }
        else
        {
            _rightGrabCounts[_currentStage, jointIndex]++;
            _rightGrabStartTimes[jointIndex] = time;
            _rightGrabStartPositions[jointIndex] = jointPosition;
            _rightGrabStartRotations[jointIndex] = jointRotation;

            float rightHandDistance = Vector3.Distance(_rightHandTransform.position, jointPosition);
            _totalGrabDistancesRight[_currentStage, jointIndex] += rightHandDistance;
            _averageGrabDistancesRight[_currentStage, jointIndex] = _totalGrabDistancesRight[_currentStage, jointIndex] / _rightGrabCounts[_currentStage, jointIndex];
        }


    }

    */

    public void EndGrab(int jointIndex, Vector3 endPosition, Quaternion endRotation, Transform joint)
    {
        if (jointIndex < 0 || jointIndex >= _jointCount)
        {
            Debug.LogError("Invalid joint index.");
            return;
        }
        if (_activeGrabRequests.TryGetValue(jointIndex, out var request))
        {
            var state = _grabStates[jointIndex];
            state.IsGrabbing = false;

            // ���Ԃ̌v�Z
            float grabDuration = Time.time - request.StartTime;
            _grabTimes[request.TaskIndex, jointIndex] += grabDuration;

            if (request.IsLeftHand == true)
            {
                _leftGrabTimes[request.TaskIndex, jointIndex] += grabDuration;
            }
            else if (request.IsLeftHand == false)
            {
                _rightGrabTimes[request.TaskIndex, jointIndex] += grabDuration;
            }

            // �}�b�s���O�ƃA�N�e�B�u���N�G�X�g����폜
            _jointToRequestMap.Remove(joint);
            _activeGrabRequests.Remove(jointIndex);

            _grabStartTimes[jointIndex] = -1f;
            _grabStartPositions[jointIndex] = Vector3.zero;
        }
    }


    /// <summary>
    /// ���ׂẴ^�X�N�X�e�[�W�ƃW���C���g�̏������O�ɕ\��
    /// </summary>
    public void LogAllGrabData()
    {
        int stage = _taskController.CurrentTasknum; // ���݂̃^�X�N�X�e�[�W
        Debug.Log($"--- Task Stage {stage + 1} ---");

        for (int joint = 0; joint < 2; joint++) {
        string log = $"Joint {joint} | " +
                            $"Grab Count: {_grabCounts[stage, joint]}, " +
                            $"Left Grab Count: {_leftGrabCounts[stage, joint]}, " +
                            $"Right Grab Count: {_rightGrabCounts[stage, joint]} | " +

                            $"Grab Time: {_grabTimes[stage, joint]:F2}s, " +
                            $"Left Grab Time: {_leftGrabTimes[stage, joint]:F2}s, " +
                            $"Right Grab Time: {_rightGrabTimes[stage, joint]:F2}s | " +

                            $"Moved Distance: {_movedDistances[stage, joint]:F2}m, " +
                            $"Left Moved Distance: {_leftMovedDistances[stage, joint]:F2}m, " +
                            $"Right Moved Distance: {_rightMovedDistances[stage, joint]:F2}m | " +

                            $"Total Rotation: {_totalRotations[stage, joint]:F2}��, " +
                            $"Left Rotation: {_leftTotalRotations[stage, joint]:F2}��, " +
                            $"Right Rotation: {_rightTotalRotations[stage, joint]:F2}�� | " +

                            $"Average Left Distance: {_averageGrabDistancesLeft[stage, joint]:F2}m, " +
                            $"Average Right Distance: {_averageGrabDistancesRight[stage, joint]:F2}m";
                            
            Debug.Log(log);
        }
        Debug.Log("--- End of Grab Data Log ---");
    }

    public void LogQueueContents()
    {
        Debug.Log("Left Hand Queue:");
        foreach (var request in _leftHandGrabRequests)
        {
            Debug.Log($"Joint: {request.JointIndex}, RequestId: {request.RequestId}");
        }

        Debug.Log("Right Hand Queue:");
        foreach (var request in _rightHandGrabRequests)
        {
            Debug.Log($"Joint: {request.JointIndex}, RequestId: {request.RequestId}");
        }

        Debug.Log("Active Requests:");
        foreach (var kvp in _activeGrabRequests)
        {
            Debug.Log($"Joint: {kvp.Key}, RequestId: {kvp.Value.RequestId}, IsLeftHand: {kvp.Value.IsLeftHand}");
        }
    }

    public void OnTaskFinished()
    {
        // ���݂̃V�[�����擾
        Scene currentScene = SceneManager.GetActiveScene();

        // �V�[�������擾
        string sceneName = currentScene.name;

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // �t�@�C���̕ۑ�����w��
        string filePath = Path.Combine(Application.persistentDataPath, $"{timestamp}_{_playId}_{sceneName}_GrabData.csv");

        // CSV�G�N�X�|�[�g�����s
        ExportGrabDataToCSV(filePath);

        Debug.Log($"CSV exported to: {filePath}");
    }


    /// <summary>
    /// GrabCounts, GrabTimes, MovedDistances ��CSV�t�@�C���ɃG�N�X�|�[�g
    /// </summary>
    public void ExportGrabDataToCSV(string filePath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // �w�b�_�[����������
                writer.WriteLine("Task Stage,Joint,Grab Count,Left Grab Count,Right Grab Count,Grab Time (s),Left Grab Time (s),Right Grab Time (s),Moved Distance (m),Left Moved Distance (m),Right Moved Distance (m),Total Rotation ,Left Rotation ,Right Rotation, Ave Left Grab Distance, Ave Right Grab Distance");

                // �f�[�^����������
                for (int stage = 0; stage < _grabCounts.GetLength(0); stage++)
                {
                    for (int joint = 0; joint < _jointCount; joint++)
                    {
                        string line = $"{stage},{joint}," +
                                      $"{_grabCounts[stage, joint]}," +
                                      $"{_leftGrabCounts[stage, joint]}," +
                                      $"{_rightGrabCounts[stage, joint]}," +
                                      $"{_grabTimes[stage, joint]:F2}," +
                                      $"{_leftGrabTimes[stage, joint]:F2}," +
                                      $"{_rightGrabTimes[stage, joint]:F2}," +
                                      $"{_movedDistances[stage, joint]:F2}," +
                                      $"{_leftMovedDistances[stage, joint]:F2}," +
                                      $"{_rightMovedDistances[stage, joint]:F2}," +
                                      $"{_totalRotations[stage, joint]:F2}," +
                                      $"{_leftTotalRotations[stage, joint]:F2}," +
                                      $"{_rightTotalRotations[stage, joint]:F2}," +
                                      $"{_averageGrabDistancesLeft[stage, joint]:F2}," +
                                      $"{_averageGrabDistancesRight[stage, joint]:F2}";

                        writer.WriteLine(line);
                    }
                }
            }

            Debug.Log($"Grab data successfully exported to: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to export CSV: {e.Message}");
        }
    }



}
