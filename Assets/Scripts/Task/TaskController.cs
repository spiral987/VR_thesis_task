using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;



public class TaskController : MonoBehaviour
{
    [SerializeField] private AudioSource correctSoundSource; // AudioSourceを指定

    [SerializeField] private AudioClip successSound; // AudioSourceを指定

    private bool isAudioPlayed = false;

    [SerializeField]
    private string _playId;
    [SerializeField]
    private List<GameObject> ghostJoints = new List<GameObject>(); // ゴースト関節リストを格納

    [SerializeField] private List<GameObject> dollJoints=new List<GameObject>(); // 操作する人形の関節リストを格納
    [SerializeField] private List<GameObject> dollJointBoxes=new List<GameObject>();//色を変える関節ボックスガイド

    [SerializeField] private GameObject GhostHip; // Ghostのhip
    [SerializeField] private GameObject GhostHip2; // Ghost2のhip
    [SerializeField] private GameObject GhostHip2Guide;//Ghost2のhip位置とdollのhip位置をつないでくれる
    //[SerializeField] private GameObject Ghost2Parent;
    [SerializeField] private GameObject DollHip; // dollのhip

    [SerializeField] private Animator initposeanim;
    [SerializeField] private Animator grabposeanim;


    [SerializeField] private float positionTolerance = 0.1f; // 位置の許容誤差
    [SerializeField] private float rotationTolerance = 15.0f; // 回転の許容誤差 (角度)
    [SerializeField] private FindByTagExample Finder;
    [SerializeField] private ChangeGhostPose PoseChanger;
    [SerializeField] private ChangeGhostPose PoseChanger2;


    [SerializeField] private List<bool> skipPositionComparison; // 位置を比較しないかどうかのチェックボックス
    [SerializeField] private List<bool> skipRotationComparison; // 回転を比較しないかどうかのチェックボックス
    [SerializeField] private List<bool> skipSync; // Syncしないかどうかのチェックボックス

    [SerializeField] private GameObject startButton; // 開始ボタン
    [SerializeField] private GameObject nextPoseButton; // 次へ進むボタン

    private int currentPoseIndex = -1; // 現在のポーズのインデックス
    public int CurrentTasknum => currentPoseIndex;
    [SerializeField] private int taskNum = 2;
    private bool isMatching = false;
    private bool isPositionMatching = false;
    private bool isRotationMatching = false;

    private bool isTaskRunning = false;


    private List<Vector3> initialBoxPosition=new List<Vector3>();//dollのboxの初期位置
    private List<Quaternion> initialBoxRotation = new List<Quaternion>();//dollのboxの初期位置
    private bool isBoxSync=true;
    [SerializeField] private float tolerance = 0.01f; // box位置がdollの関節と不一致とみなす許容誤差

    private float overallMatchScore = 0.0f; // 全体の一致率を格納

    //UndoとRedoのやつ
    // 直前の状態を保存するUndoスタック
    private Stack<List<Vector3>> undoPositionStack = new Stack<List<Vector3>>();
    private Stack<List<Quaternion>> undoRotationStack = new Stack<List<Quaternion>>();

    // Undoで戻した状態を保存するRedoスタック
    private Stack<List<Vector3>> redoPositionStack = new Stack<List<Vector3>>();
    private Stack<List<Quaternion>> redoRotationStack = new Stack<List<Quaternion>>();


    //時間管理
    [SerializeField]
    private TextMeshProUGUI startTaskTimerText; // StartTask の待機時間を表示する TMPro
    [SerializeField]
    private TextMeshProUGUI currentPoseText; // currentposeindex

    private List<float> taskDurations = new List<float>(); // 各タスクの完了時間を記録
    private float currentTaskStartTime = -1f; // 現在のタスクの開始時刻

    [SerializeField]
    private GrabCountManager countManager;

    [SerializeField]
    private PinchCounter _pinchCounter;

    private bool isPracticeMode = false; // 練習モードかどうか
    private int practicePoseIndex = 0; // 現在の練習ポーズインデックス

    [SerializeField] private ChangeGhostPosePractice PoseChangerPractice;
    [SerializeField] private ChangeGhostPosePractice PoseChanger2Practice;
    [SerializeField] private int practicePoseNum=5;

    private void Start()
    {
        // 次へ進むボタンを初期状態で非表示に
        nextPoseButton.SetActive(false);
        //boxの初期位置を取得しておく
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




        //ポーズリセット以外のたいみんぐで、boxと関節の位置をシンクさせる
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

            // 一致判定を行い、結果に応じた処理を実行
            isMatching = CompareTransforms();

            if (isMatching)
            {
                //音を出す
                if (!isAudioPlayed) {
                correctSoundSource.PlayOneShot(successSound);
                    isAudioPlayed = true;
                }

                // 全ての関節が一致している場合
                nextPoseButton.SetActive(true); // 次へ進むボタンを表示
            }
            else
            {
                isAudioPlayed = false;
                nextPoseButton.SetActive(false); // ボタンを非表示
            }

           
        }
    }

    //public（ボタンにかかわる）

    //startボタンを押したとき
    public void StartTask()
    {
        // 5秒待機してタスクを開始するコルーチンを呼び出し
        StartCoroutine(StartTaskWithDelay(5.0f)); // 5秒待機
    }

    // コルーチンで遅延処理を行う
    private System.Collections.IEnumerator StartTaskWithDelay(float delaySeconds)
    {
        startButton.SetActive(false);

        if (!isPracticeMode)
        {
            isTaskRunning = true;
        }
        float timeRemaining = delaySeconds;

        // 待機時間をカウントダウン形式で表示
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

        // タイマーを非表示にしてタスクを開始
        startTaskTimerText.text = "Go!";
        if (isTaskRunning)
        {
            //今のステージインデックスをインクリメントし、計測を開始
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




        // タスク開始時刻を記録
        currentTaskStartTime = Time.time;
    }


    // ボタンを押した際に遅延処理を実行
    public void OnPressedButton()
    {
        // 次の処理を開始
        StartCoroutine(OnPressedButtonWithDelay(5.0f)); // 5秒待機
    }

    // コルーチンで遅延処理を行う
    private System.Collections.IEnumerator OnPressedButtonWithDelay(float delaySeconds)
    {

        float timeRemaining = delaySeconds;

        // ボタンを非表示に戻す
        nextPoseButton.SetActive(false);

        if (currentTaskStartTime > 0)
        {
            // タスクの終了時間を計算
            float taskDuration = Time.time - currentTaskStartTime;
            taskDurations.Add(taskDuration); // リストに記録
            Debug.Log($"Task {currentPoseIndex + 1} completed in {taskDuration:F2} seconds.");
        }
        if (isTaskRunning)
        {
            // タスク時間をログに表示し、CSVにエクスポート
            LogTaskDurationsAndExport();
            //掴んだデータもexport
            countManager.OnTaskFinished();
            //ピンチデータ
            _pinchCounter.ExportPinchDataToCSV();

            // タスク試行回数に達したら
            if (currentPoseIndex + 1 == taskNum)
            {
                startTaskTimerText.text = "finish";
                isTaskRunning = false;
                nextPoseButton.SetActive(false);

                yield break; // コルーチンを終了
            }
            currentPoseIndex++;
            currentPoseText.text = $"stage: {currentPoseIndex + 1} ";

        }

        int tmpIndex = currentPoseIndex;
        currentPoseIndex = -1;

        // 待機時間をカウントダウン形式で表示
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
            StartCoroutine(LoadNextPose()); // 次のポーズをロード
        }
        else if (isPracticeMode)
        {
            StartCoroutine(LoadPracticePose());
        }
        // 次のタスクの計測を開始
        currentTaskStartTime = Time.time;

        //せいかいのおと
        isAudioPlayed = false;
    }

    public void DollReset()
    {
        // 現在のDollの関節を初期化
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
                    Debug.LogWarning($"リストのインデックス {i} に対応するオブジェクトが設定されていません。");
                    continue;
                }

                // オブジェクト1とオブジェクト2の位置が不一致の場合
                if (Vector3.Distance(doll.transform.position, box.transform.position) > tolerance)
                {
                    // オブジェクト2をオブジェクト1の位置に移動
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
        bool allMatched = true; // すべての要素がマッチングしているかどうか
        float totalMatchScore = 0.0f; // 全体の一致率合計

        //関節ごとに比較
        for (int i = 0; i < totalCount; i++)
        {
            GameObject nowGhostJoint = ghostJoints[i];
            GameObject nowDollJoint = dollJoints[i];
            GameObject nowDollJointBox = dollJointBoxes[i];

            // nowGhostJoint, nowDollJoint の座標を基準オブジェクト (referenceObject) からの相対座標に変換
            Vector3 relativePositionNowGhostJoint = GhostHip.transform.InverseTransformPoint(nowGhostJoint.transform.position);
            Vector3 relativePositionNowDollJoint = DollHip.transform.InverseTransformPoint(nowDollJoint.transform.position);

            // nowGhostJoint, nowDollJoint の回転を基準オブジェクト (referenceObject) の相対回転に変換
            Quaternion relativeRotationNowGhostJoint = Quaternion.Inverse(GhostHip.transform.rotation) * nowGhostJoint.transform.rotation;
            Quaternion relativeRotationNowDollJoint = Quaternion.Inverse(DollHip.transform.rotation) * nowDollJoint.transform.rotation;

            //Debug.Log($"Relative Ghost Position (Index {i}): " + relativePositionNowGhostJoint);
            //Debug.Log($"Relative Doll Position (Index {i}): " + relativePositionNowDollJoint);
            //Debug.Log($"Relative Ghost Rotation (Index {i}): " + relativeRotationNowGhostJoint.eulerAngles);
            //Debug.Log($"Relative Doll Rotation (Index {i}): " + relativeRotationNowDollJoint.eulerAngles);

            // 一致率を計算
            float matchScore = CalculateJointMatchScore(
                relativePositionNowGhostJoint,
                relativePositionNowDollJoint,
                relativeRotationNowGhostJoint,
                relativeRotationNowDollJoint,
                skipPositionComparison[i],
                skipRotationComparison[i]);

            totalMatchScore += matchScore;

            
            // 比較メソッドにスキップフラグを渡す
            if (AreTransformsMatching(relativePositionNowGhostJoint, relativePositionNowDollJoint, relativeRotationNowGhostJoint, relativeRotationNowDollJoint, skipPositionComparison[i], skipRotationComparison[i]))
            {
                //Color darkGreen = new Color(0.0f, 0.39f, 0.0f); // #006400 に対応するRGB値
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

        // 全体の一致率を計算
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

    //dollとghostのひとつの関節位置を比較する　どっちもあってたらtrueを返す
    private bool AreTransformsMatching(Vector3 pos1, Vector3 pos2, Quaternion rot1, Quaternion rot2, bool skipPosition, bool skipRotation)
    {

        // 位置の比較（スキップフラグがfalseの場合のみ）
        if (!skipPosition && Vector3.Distance(pos1, pos2) > positionTolerance)
        {
            isPositionMatching = false;
            //Debug.Log("Position mismatch");
        }
        else
        {
            isPositionMatching = true;
        }

        // 回転の比較（スキップフラグがfalseの場合のみ）
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
    /// 関節ごとの一致率を計算 (Tolerance内なら100%)  
    /// </summary>
    private float CalculateJointMatchScore(Vector3 pos1, Vector3 pos2, Quaternion rot1, Quaternion rot2, bool skipPosition, bool skipRotation)
    {
        float positionScore = 1.0f; // 初期値は100%とする
        float rotationScore = 1.0f; // 初期値は100%とする

        // 位置の一致率を計算
        if (!skipPosition)
        {
            float distance = Vector3.Distance(pos1, pos2);
            if (distance <= positionTolerance)
            {
                positionScore = 1.0f; // Tolerance内なら100%
            }
            else
            {
                // Toleranceを超えた分だけ減衰,2*Toleranceで0パーセント
                positionScore = Mathf.Clamp01(1.0f - (distance - positionTolerance) / (positionTolerance * 2));
            }
        }

        // 回転の一致率を計算
        if (!skipRotation)
        {
            float angleDifference = Quaternion.Angle(rot1, rot2);
            if (angleDifference <= rotationTolerance)
            {
                rotationScore = 1.0f; // Tolerance内なら100%
            }
            else
            {
                // Toleranceを超えた分だけ減衰
                rotationScore = Mathf.Clamp01(1.0f - (angleDifference - rotationTolerance) / (rotationTolerance * 2));
            }
        }

        // 位置と回転の一致率の平均を返す
        return (positionScore + rotationScore) / 2.0f;
    }





    //次のポーズをロードし、dollのポーズをTに戻す
    private System.Collections.IEnumerator LoadNextPose()
    {

        // 現在のDollの関節を初期化
        //grabposeanim.enabled =false;
        isBoxSync = false;
        ApplyPositionsToTarget(DollHip, dollJointBoxes, initialBoxPosition);
        ApplyJointRotationsRelativeToHip(DollHip, dollJointBoxes, initialBoxRotation);
        isBoxSync = true;
        //initposeanim.enabled=true;
        //yield return new WaitForSeconds(1.0f); // 指定秒数待機
        //grabposeanim.enabled = true;
        //initposeanim.enabled=false;
        PoseChanger.ChangePose(currentPoseIndex);
        PoseChanger2.ChangePose(currentPoseIndex);


        Debug.Log($"init doll and load next pose");
        yield return null;
    }

    //次のポーズをロードし、dollのポーズをTに戻す
    private System.Collections.IEnumerator LoadPracticePose()
    {

        // 現在のDollの関節を初期化
        //grabposeanim.enabled =false;
        isBoxSync = false;
        ApplyPositionsToTarget(DollHip, dollJointBoxes, initialBoxPosition);
        ApplyJointRotationsRelativeToHip(DollHip, dollJointBoxes, initialBoxRotation);
        isBoxSync = true;
        //initposeanim.enabled=true;
        //yield return new WaitForSeconds(1.0f); // 指定秒数待機
        //grabposeanim.enabled = true;
        //initposeanim.enabled=false;
        PoseChangerPractice.ChangePose(practicePoseIndex);
        PoseChanger2Practice.ChangePose(practicePoseIndex);

        Debug.Log($"init doll and load next pose");
        yield return null;
    }


    //Dollの初期Box位置を取得
    List<Vector3> GetJointPositionsRelativeToHip()
    {
        List<Vector3> positions = new List<Vector3>();

        foreach (GameObject joint in dollJointBoxes)
        {
            if (joint == null) continue;

            // Hipを基準としたローカル座標
            Vector3 relativePosition = DollHip.transform.InverseTransformPoint(joint.transform.position);
            positions.Add(relativePosition);
        }

        return positions;
    }

    //Dollの初期Box1回転を取得
    List<Quaternion> GetJointRotationsRelativeToHip()
    {
        List<Quaternion> rotations = new List<Quaternion>();

        foreach (GameObject joint in dollJointBoxes)
        {
            if (joint == null) continue;

            // Hipを基準としたローカル回転
            Quaternion relativeRotation = Quaternion.Inverse(DollHip.transform.rotation) * joint.transform.rotation;
            rotations.Add(relativeRotation);
        }

        return rotations;
    }




    //初期Box位置を適用
    void ApplyPositionsToTarget(GameObject targetHip, List<GameObject> targetJoints, List<Vector3> localPositions)
    {
        for (int i = 0; i < targetJoints.Count; i++)
        {
            if (targetJoints[i] == null) continue;

            // ローカル座標をワールド座標に変換して適用
            Vector3 worldPosition = targetHip.transform.TransformPoint(localPositions[i]);
            targetJoints[i].transform.position = worldPosition;
        }
    }

    //初期回転位置を適用
    void ApplyJointRotationsRelativeToHip(GameObject targetHip, List<GameObject> targetJointBoxes, List<Quaternion> relativeRotations)
    {
        if (targetJointBoxes == null || relativeRotations == null || targetJointBoxes.Count != relativeRotations.Count)
        {
            Debug.LogError("リストが正しく設定されていないか、要素数が一致していません。");
            return;
        }

        for (int i = 0; i < targetJointBoxes.Count; i++)
        {
            GameObject joint = targetJointBoxes[i];

            if (joint == null) continue;

            // Hipを基準としたローカル回転をワールド回転に変換して適用
            joint.transform.rotation = targetHip.transform.rotation * relativeRotations[i];
        }
    }

    //ポーズの正誤に応じてtargetのvisualの色を変える
    void ChangeChildrenColors(GameObject parent, Color color)
    {
        // 親オブジェクトのTransformを取得
        Transform parentTransform = parent.transform;

        // 全ての子オブジェクトをループ処理
        foreach (Transform child in parentTransform)
        {
            // Rendererコンポーネントを取得
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 色を変更
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
        // 現在のシーンを取得
        Scene currentScene = SceneManager.GetActiveScene();

        // シーン名を取得
        string sceneName = currentScene.name;

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // コンソールにタスク時間をログ出力
        LogTaskDurations();

        // CSVをエクスポート
        string filePath = Path.Combine(Application.persistentDataPath, $"{timestamp}_{_playId}_{sceneName}_TaskDurations.csv");
        ExportTaskDurationsToCSV(filePath);
    }


    private void ExportTaskDurationsToCSV(string filePath)
    {
        // CSVのヘッダー
        string csvContent = "Task,Duration (seconds)\n";



        // タスクごとのデータを追加
        for (int i = 0; i < taskDurations.Count; i++)
        {
            csvContent += $"{i + 1},{taskDurations[i]:F2}\n";
        }

        try
        {
            // ファイルに書き出し
            File.WriteAllText(filePath, csvContent);
            Debug.Log($"Task durations successfully exported to: {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to write CSV: {e.Message}");
        }
    }

    // タスク番号から開始する
    public void StartTaskFromIndex(int taskIndex)
    {
        if (taskIndex < 0 || taskIndex >= taskNum)
        {
            Debug.LogError($"Invalid task index: {taskIndex}. Must be between 0 and {taskNum - 1}.");
            return;
        }

        // タスク番号を設定してタスクを開始
        currentPoseIndex = taskIndex - 1; // StartTaskで+1されるため
        StartTask();
    }

    public void ResetScene()
    {
        // 現在のシーンをリロード
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Debug.Log($"Scene {currentScene.name} has been reset.");
    }

    // 練習モードを開始
    public void StartPracticeMode()
    {
        isPracticeMode = true;
        practicePoseIndex = 0;
        Debug.Log("Practice mode started.");
    }



}

