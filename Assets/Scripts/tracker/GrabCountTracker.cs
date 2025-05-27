using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;
using Unity.VisualScripting;
using Oculus.Interaction.Input;


public class GrabCountTracker : MonoBehaviour
{

    [SerializeField]
    private GrabCountManager _manager;

    private Grabbable _grabbable;

    private bool _isGrabbing = false; // 現在掴んでいるかどうかのフラグ

    [SerializeField]
    private int JointIndex; // このトラッカーが担当するジョイントのインデックス

    [SerializeField]
    private Transform jointPosition;

    private int grabCount = 0; // このジョイントが掴まれた回数


    private void Start()
    {
        _manager = FindObjectOfType<GrabCountManager>();
        if (_manager == null)
        {
            Debug.LogError("GrabCountManager not found in the scene.");
        }
        // Grabbableを取得
        _grabbable = GetComponent<Grabbable>();
        if (_grabbable == null)
        {
            Debug.LogError("Grabbableがこのオブジェクトに見つかりません。");
            return;
        }
        // Grabbableのイベントリスナーに登録
        _grabbable.WhenPointerEventRaised += OnPointerEventRaised;
    }

    void Update()
    {
        if (_isGrabbing)
        {
            _manager.Calculatedistance(jointPosition);
        }
    }
    private void OnDestroy()
    {
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised -= OnPointerEventRaised;
        }
    }


    private void OnPointerEventRaised(PointerEvent evt)
    {

        if (evt.Type == PointerEventType.Select)
        {
            _isGrabbing = true;

            // 掴むイベントが発生した場合にカウントを増加
            StartGrab();

        }

        else if (evt.Type == PointerEventType.Unselect)
        {
            // 掴む終了時の処理
            if (_isGrabbing)
            {
                _isGrabbing = false;
                EndGrab();
                
            }

        }
    }

    public void StartGrab()
    {
        if (_manager != null)
        {
            _manager.StartGrab(JointIndex, jointPosition.position, jointPosition.rotation, jointPosition);
            //_manager.LogQueueContents();
        }
    }

    public void EndGrab()
    {
        if (_manager != null)
        {
            _manager.EndGrab(JointIndex, jointPosition.position, jointPosition.rotation, jointPosition);
            //_manager.LogQueueContents();
            _manager.LogAllGrabData();
        }
    }

}

