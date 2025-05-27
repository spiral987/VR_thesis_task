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

    [SerializeField] private Hand _leftHand; // �����Hand�N���X
    [SerializeField] private Hand _rightHand; // �E���Hand�N���X
    [SerializeField] private HandFinger fingerToTrack = HandFinger.Index; // �ǐՂ���w

    [SerializeField]
    private int _taskCount = 10;//�^�X�N�̌�

    private int[,] pinchCounts; // [taskStage, 0=LeftHand, 1=RightHand]
    private bool _wasLeftPinching = false;
    private bool _wasRightPinching = false;


    private void Start()
    {
        pinchCounts = new int[_taskCount, 2]; // �^�X�N���Ǝ�̎�ނɑΉ�����2D�z���������
    }

    private void Update()
    {
        int currentStage = _taskController.CurrentTasknum; // ���݂̃^�X�N�X�e�[�W

        // ����̃s���`���
        bool isLeftPinching = _leftHand.GetFingerIsPinching(fingerToTrack);
        if (_wasLeftPinching && !isLeftPinching) // �s���`�I����
        {
            pinchCounts[currentStage, 0]++;
            Debug.Log($"Task {currentStage + 1}: Left Pinch Count = {pinchCounts[currentStage, 0]}");
        }
        _wasLeftPinching = isLeftPinching;

        // �E��̃s���`���
        bool isRightPinching = _rightHand.GetFingerIsPinching(fingerToTrack);
        if (_wasRightPinching && !isRightPinching) // �s���`�I����
        {
            pinchCounts[currentStage, 1]++;
            Debug.Log($"Task {currentStage + 1}: Right Pinch Count = {pinchCounts[currentStage, 1]}");
        }
        _wasRightPinching = isRightPinching;
    }


    // �s���`�f�[�^��CSV�ɕۑ�
    public void ExportPinchDataToCSV()
    {
        // ���݂̃V�[�����擾
        Scene currentScene = SceneManager.GetActiveScene();

        // �V�[�������擾
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
