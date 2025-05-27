using System.Collections.Generic;
using UnityEngine;



public class TaskControllerLegacy : MonoBehaviour
{
    //ゴーストポーズを10個格納するため
    [SerializeField]
    private List<ChildList> ghostJoints = new List<ChildList>(); // ゴースト関節リストを格納

    [System.Serializable]
    class ChildList
    {
        public List<GameObject> joint = new List<GameObject>();
    }

    [SerializeField] private List<GameObject> ghost;

    [SerializeField] private List<GameObject> dollJoints; // 操作する人形の関節リストを格納

    [SerializeField] private List<GameObject> GhostHip = new List<GameObject>(); // Ghostのhip
    [SerializeField] private GameObject DollHip; // dollのhip

    [SerializeField] private float positionTolerance = 0.1f; // 位置の許容誤差
    [SerializeField] private float rotationTolerance = 15.0f; // 回転の許容誤差 (角度)
    [SerializeField] private ChangeColor Changer;
    [SerializeField] private FindByTagExample Finder;

    [SerializeField] private List<bool> skipPositionComparison; // 位置を比較しないかどうかのチェックボックス
    [SerializeField] private List<bool> skipRotationComparison; // 回転を比較しないかどうかのチェックボックス

    [SerializeField] private GameObject nextPoseButton; // 次へ進むボタン

    private int currentPoseIndex = 0; // 現在のポーズのインデックス
    [SerializeField] private int taskNum = 1;
    private bool isMatching = false;

    private bool isTaskRunning = false;

    public void StartTask()
    {
        isTaskRunning = true;
        // 次へ進むボタンを初期状態で非表示に
        nextPoseButton.SetActive(false);
        //関節の名称リストを作り、順番に検索してリストに格納する
        List<string> jointNames = new List<string> { "LeftArm", "LeftForeArm" };

        //Ghostの関節をGhostリストに格納
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

            // 一致判定を行い、結果に応じた処理を実行
            isMatching = CompareTransforms();

            if (isMatching)
            {
                // 全ての関節が一致している場合
                nextPoseButton.SetActive(true); // 次へ進むボタンを表示
            }
            else
            {
                nextPoseButton.SetActive(false); // ボタンを非表示
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
        bool allMatched = true; // すべての要素がマッチングしているかどうか

        for (int i = 0; i < totalCount; i++)
        {
            GameObject nowGhostJoint = ghostJoints[currentPoseIndex].joint[i];
            GameObject nowDollJoint = dollJoints[i];

            // nowGhostJoint, nowDollJoint の座標を基準オブジェクト (referenceObject) からの相対座標に変換
            Vector3 relativePositionNowGhostJoint = GhostHip[currentPoseIndex].transform.InverseTransformPoint(nowGhostJoint.transform.position);
            Vector3 relativePositionNowDollJoint = DollHip.transform.InverseTransformPoint(nowDollJoint.transform.position);

            // nowGhostJoint, nowDollJoint の回転を基準オブジェクト (referenceObject) の相対回転に変換
            Quaternion relativeRotationNowGhostJoint = Quaternion.Inverse(GhostHip[currentPoseIndex].transform.rotation) * nowGhostJoint.transform.rotation;
            Quaternion relativeRotationNowDollJoint = Quaternion.Inverse(DollHip.transform.rotation) * nowDollJoint.transform.rotation;

            Debug.Log($"Relative Ghost Position (Index {i}): " + relativePositionNowGhostJoint);
            Debug.Log($"Relative Doll Position (Index {i}): " + relativePositionNowDollJoint);
            Debug.Log($"Relative Ghost Rotation (Index {i}): " + relativeRotationNowGhostJoint.eulerAngles);
            Debug.Log($"Relative Doll Rotation (Index {i}): " + relativeRotationNowDollJoint.eulerAngles);

            // 比較メソッドにスキップフラグを渡す
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
        // 位置の比較（スキップフラグがfalseの場合のみ）
        if (!skipPosition && Vector3.Distance(pos1, pos2) > positionTolerance)
        {
            Debug.Log("Position mismatch");
            return false;
        }

        // 回転の比較（スキップフラグがfalseの場合のみ）
        if (!skipRotation && Quaternion.Angle(rot1, rot2) > rotationTolerance)
        {
            Debug.Log("Rotation mismatch");
            return false;
        }

        return true;
    }

    public void OnPressedButton()
    {
        //タスク試行回数に達したら
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

        // 現在のDollを初期化（必要に応じて実装）
        foreach (var joint in dollJoints)
        {
            Changer.SetColorToGreen(joint.gameObject, Color.white); // 色をリセット
        }

        // ボタンを非表示に戻す
        nextPoseButton.SetActive(false);

        Debug.Log($"次のポーズ {++currentPoseIndex} をロードしました。");
    }
}

