using UnityEngine;
using Oculus.Interaction.Input;
using System.IO;
using UnityEngine.SceneManagement;

public class PinchCounter : MonoBehaviour
{
    [SerializeField]
    private string _playId;

    [SerializeField]
    private TaskController _taskController;

    [SerializeField] private Hand _leftHand; // 左手のHandクラス
    [SerializeField] private Hand _rightHand; // 右手のHandクラス
    [SerializeField] private HandFinger fingerToTrack = HandFinger.Index; // 追跡する指

    [SerializeField]
    private int _taskCount = 10;//タスクの個数

    private int[,] pinchCounts; // [taskStage, 0=LeftHand, 1=RightHand]
    private bool _wasLeftPinching = false;
    private bool _wasRightPinching = false;


    private void Start()
    {
        pinchCounts = new int[_taskCount, 2]; // タスク数と手の種類に対応する2D配列を初期化
    }

    private void Update()
    {
        int currentStage = _taskController.CurrentTasknum; // 現在のタスクステージ

        // 左手のピンチ状態
        bool isLeftPinching = _leftHand.GetFingerIsPinching(fingerToTrack);
        if (_wasLeftPinching && !isLeftPinching) // ピンチ終了時
        {
            pinchCounts[currentStage, 0]++;
            Debug.Log($"Task {currentStage + 1}: Left Pinch Count = {pinchCounts[currentStage, 0]}");
        }
        _wasLeftPinching = isLeftPinching;

        // 右手のピンチ状態
        bool isRightPinching = _rightHand.GetFingerIsPinching(fingerToTrack);
        if (_wasRightPinching && !isRightPinching) // ピンチ終了時
        {
            pinchCounts[currentStage, 1]++;
            Debug.Log($"Task {currentStage + 1}: Right Pinch Count = {pinchCounts[currentStage, 1]}");
        }
        _wasRightPinching = isRightPinching;
    }


    // ピンチデータをCSVに保存
    public void ExportPinchDataToCSV()
    {
        // 現在のシーンを取得
        Scene currentScene = SceneManager.GetActiveScene();

        // シーン名を取得
        string sceneName = currentScene.name;

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        string filePath = Path.Combine(Application.persistentDataPath, $"{timestamp}_{_playId}_{sceneName}_PinchCounts.csv");
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Task Stage,Left Hand Pinch Count,Right Hand Pinch Count");
                for (int i = 0; i < _taskCount; i++)
                {
                    writer.WriteLine($"{i + 1},{pinchCounts[i, 0]},{pinchCounts[i, 1]}");
                }
            }
            Debug.Log($"Pinch data successfully exported to: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to export CSV: {e.Message}");
        }
    }
}
