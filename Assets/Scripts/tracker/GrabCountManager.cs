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
//using UnityEditor.PackageManager.Requests; // 非同期処理に必要


public class GrabCountManager : MonoBehaviour
{
    [SerializeField]
    private string _playId;

    //[SerializeField]
    //private List<GrabCountTracker> _trackers = new List<GrabCountTracker>();

    [SerializeField]
    private TaskController _taskController;

    //左手と右手の位置
    [SerializeField]
    private Transform _leftHandTransform;
    [SerializeField]
    private Transform _rightHandTransform;

    //記録したい値・左手と右手
    private int[,] _grabCounts; // 二次元配列: [タスクステージ, 関節]
    private int[,] _leftGrabCounts; // 左手で掴んだ回数: [タスクステージ, 関節]
    private int[,] _rightGrabCounts; // 右手で掴んだ回数: [タスクステージ, 関節]

    private float[,] _grabTimes; // 掴んでいる時間: [タスクステージ, 関節]
    private float[,] _leftGrabTimes; // 左手で掴んでいる合計時間: [タスクステージ, 関節]
    private float[,] _rightGrabTimes; // 右手で掴んでいる合計時間: [タスクステージ, 関節]

    private float[,] _movedDistances; // 動かした距離: [タスクステージ, 関節]
    private float[,] _leftMovedDistances; // 動かした距離: [タスクステージ, 関節]
    private float[,] _rightMovedDistances; // 動かした距離: [タスクステージ, 関節]

    // 回転角度の累計を記録
    private float[,] _totalRotations; // 累計回転角度: [タスクステージ, 関節]
    private float[,] _leftTotalRotations; // 左手での累計回転角度: [タスクステージ, 関節]
    private float[,] _rightTotalRotations; // 右手での累計回転角度: [タスクステージ, 関節]

    private float[,] _totalGrabDistancesLeft; // 累積の手とオブジェクトの距離: [タスクステージ, 関節]
    private float[,] _averageGrabDistancesLeft; // 平均の手とオブジェクトの距離: [タスクステージ, 関節]
    private float[,] _totalGrabDistancesRight; // 累積の手とオブジェクトの距離: [タスクステージ, 関節]
    private float[,] _averageGrabDistancesRight; // 平均の手とオブジェクトの距離: [タスクステージ, 関節]


    //Grab開始時に記録する変数
    private float[] _grabStartTimes; // 各関節の掴み開始時間を記録
    private float[] _leftGrabStartTimes; // L各関節の掴み開始時間を記録
    private float[] _rightGrabStartTimes; // R各関節の掴み開始時間を記録

    private Vector3[] _grabStartPositions; // 各関節の掴み開始位置を記録
    private Vector3[] _leftGrabStartPositions; // 各関節の掴み開始位置を記録
    private Vector3[] _rightGrabStartPositions; // 各関節の掴み開始位置を記録

    private Quaternion[] _grabStartRotations; // 各関節の掴み開始時の回転
    private Quaternion[] _leftGrabStartRotations; // 左手の掴み開始時の回転
    private Quaternion[] _rightGrabStartRotations; // 右手の掴み開始時の回転

    [SerializeField]
    private int _taskCount = 10;//タスクの個数
    [SerializeField]
    private int _jointCount = 15; // ジョイントの数

    private bool? isLeftHand = null;

    // Grab リクエストのクラス
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
        public string RequestId { get; set; } // リクエストを一意に識別するID
    }
    // 両手用の別々のキューを用意
    private Queue<GrabRequest> _leftHandGrabRequests = new Queue<GrabRequest>();
    private Queue<GrabRequest> _rightHandGrabRequests = new Queue<GrabRequest>();

    // 現在処理中のリクエストを追跡
    private Dictionary<int, GrabRequest> _activeGrabRequests = new Dictionary<int, GrabRequest>();

    // Transform と GrabRequest を関連付けるための Dictionary を追加
    private Dictionary<Transform, GrabRequest> _jointToRequestMap = new Dictionary<Transform, GrabRequest>();

    // GrabStateクラスを修正
    private class GrabState
    {
        public bool IsGrabbing;
        public Vector3 LastPosition = Vector3.zero;
        public Quaternion LastRotation = Quaternion.identity;
        public bool IsInitialized = false;
    }

    private Dictionary<int, GrabState> _grabStates; // 各ジョイントの状態


    public void IsLeftHand(bool isLeftHand)
    {
        // 最新のアクティブなリクエストを探して手の情報を設定
        var activeRequest = _activeGrabRequests.Values.OrderByDescending(r => r.StartTime).FirstOrDefault();

        if (activeRequest != null && activeRequest.IsLeftHand == null)
        {
            activeRequest.IsLeftHand = isLeftHand;

            // 適切なキューに振り分け
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
    /// GrabCountManagerの初期化
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
            _grabStartTimes[i] = -1f; // 掴み開始時間を無効な値で初期化
            _grabStartPositions[i] = Vector3.zero; // 掴み開始位置を初期化
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
        // 引数の joint に対応する Request が存在するか確認
        if (!_jointToRequestMap.TryGetValue(joint, out var request))
        {
            return;  // 対応する Request が無ければ処理をスキップ
        }

        int jointIndex = request.JointIndex;
        int taskIndex = request.TaskIndex;
        var state = _grabStates[jointIndex];

        if (!state.IsGrabbing)
        {
            return;
        }

        // 最初の位置と回転を記録
        if (state.LastPosition == Vector3.zero)
        {
            state.LastPosition = joint.position;
            state.LastRotation = joint.rotation;
            return;
        }

        // 現在の位置と回転を取得
        Vector3 currentPosition = joint.position;
        Quaternion currentRotation = joint.rotation;

        // 移動距離を計算（前フレームからの差分）
        float distance = Vector3.Distance(state.LastPosition, currentPosition);
        if (distance > 0.0001f)
        {
            _movedDistances[taskIndex, jointIndex] += distance;
            Debug.Log($"Joint {jointIndex} moved: {distance:F4} units. Total: {_movedDistances[taskIndex, jointIndex]:F4}");
        }

        // 回転角度を計算（前フレームからの差分）
        float rotationAngle = Quaternion.Angle(state.LastRotation, currentRotation);
        if (rotationAngle > 0.01f)
        {
            _totalRotations[taskIndex, jointIndex] += rotationAngle;
            Debug.Log($"Joint {jointIndex} rotated: {rotationAngle:F4} degrees. Total: {_totalRotations[taskIndex, jointIndex]:F4}");
        }

        // 左手/右手の判定に基づいて距離と回転を記録
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

        // 状態を更新
        state.LastPosition = currentPosition;
        state.LastRotation = currentRotation;
    }


    // StartGrabメソッドも更新
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
        state.LastPosition = jointPosition;  // 初期位置を設定
        state.LastRotation = jointRotation;  // 初期回転を設定
        state.IsInitialized = true;

        string requestId = System.Guid.NewGuid().ToString();

        var grabRequest = new GrabRequest
        {
            TaskIndex = taskIndex,
            JointIndex = jointIndex,
            JointPosition = jointPosition,
            JointRotation = jointRotation,
            StartPosition = jointPosition,  // StartPositionも正しく設定
            StartRotation = jointRotation,  // StartRotationも正しく設定
            StartTime = Time.time,
            RequestId = requestId
        };

    _activeGrabRequests[jointIndex] = grabRequest;
    _jointToRequestMap[joint] = grabRequest;  // Transform と Request を関連付け
    StartCoroutine(WaitForHandAssignment(grabRequest));

        // デバッグ用
        //Debug.Log($"Started grab for joint {jointIndex} at position {jointPosition}");
    }



    /// <summary>
    /// 掴む回数を記録
    /// </summary>
    private System.Collections.IEnumerator WaitForHandAssignment(GrabRequest grabRequest)
    {
        // isLeftHand が判明するまで待機
        while (grabRequest.IsLeftHand == null)
        {
            yield return null; // 毎フレーム待機
        }

        //カウント
        _grabCounts[grabRequest.TaskIndex, grabRequest.JointIndex]++;

        //時間
        //_grabStartTimes[grabRequest.JointIndex] = time; // 掴み開始時間を記録

        //距離
        //_grabStartPositions[grabRequest.JointIndex] = grabRequest.JointPosition; // 掴み開始位置を記録
        grabRequest.StartPosition = grabRequest.JointPosition;

        // 回転の記録開始
        //_grabStartRotations[grabRequest.JointIndex] = grabRequest.JointRotation;
        grabRequest.StartRotation = grabRequest.JointRotation;

        if (grabRequest.IsLeftHand == true)
        {
            _leftGrabCounts[grabRequest.TaskIndex, grabRequest.JointIndex]++;
            _leftGrabStartPositions[grabRequest.JointIndex] = grabRequest.JointPosition;
            _leftGrabStartRotations[grabRequest.JointIndex] = grabRequest.JointRotation;

            float leftHandDistance = Vector3.Distance(_leftHandTransform.position, grabRequest.JointPosition);
            _totalGrabDistancesLeft[grabRequest.TaskIndex, grabRequest.JointIndex] += leftHandDistance;

            // 平均の計算を修正
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

            // 平均の計算を修正
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
        int _currentStage = _taskController.CurrentTasknum; // 現在のタスクステージ

        if (jointIndex < 0 || jointIndex >= _jointCount || _currentStage < 0)
        {
            Debug.LogError("Invalid joint and task index.");
            return;
        }

        //カウント
        _grabCounts[_currentStage, jointIndex]++;

        //時間
        float time = Time.time;
        _grabStartTimes[jointIndex] = time; // 掴み開始時間を記録

        //距離
        _grabStartPositions[jointIndex] = jointPosition; // 掴み開始位置を記録

        // 回転の記録開始
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

            // 時間の計算
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

            // マッピングとアクティブリクエストから削除
            _jointToRequestMap.Remove(joint);
            _activeGrabRequests.Remove(jointIndex);

            _grabStartTimes[jointIndex] = -1f;
            _grabStartPositions[jointIndex] = Vector3.zero;
        }
    }


    /// <summary>
    /// すべてのタスクステージとジョイントの情報をログに表示
    /// </summary>
    public void LogAllGrabData()
    {
        int stage = _taskController.CurrentTasknum; // 現在のタスクステージ
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

                            $"Total Rotation: {_totalRotations[stage, joint]:F2}°, " +
                            $"Left Rotation: {_leftTotalRotations[stage, joint]:F2}°, " +
                            $"Right Rotation: {_rightTotalRotations[stage, joint]:F2}° | " +

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
        // 現在のシーンを取得
        Scene currentScene = SceneManager.GetActiveScene();

        // シーン名を取得
        string sceneName = currentScene.name;

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // ファイルの保存先を指定
        string filePath = Path.Combine(Application.persistentDataPath, $"{timestamp}_{_playId}_{sceneName}_GrabData.csv");

        // CSVエクスポートを実行
        ExportGrabDataToCSV(filePath);

        Debug.Log($"CSV exported to: {filePath}");
    }


    /// <summary>
    /// GrabCounts, GrabTimes, MovedDistances をCSVファイルにエクスポート
    /// </summary>
    public void ExportGrabDataToCSV(string filePath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // ヘッダーを書き込む
                writer.WriteLine("Task Stage,Joint,Grab Count,Left Grab Count,Right Grab Count,Grab Time (s),Left Grab Time (s),Right Grab Time (s),Moved Distance (m),Left Moved Distance (m),Right Moved Distance (m),Total Rotation ,Left Rotation ,Right Rotation, Ave Left Grab Distance, Ave Right Grab Distance");

                // データを書き込む
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
