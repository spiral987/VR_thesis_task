using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GrabbableObject : MonoBehaviour
{
    private GlobalUndoRedoManager globalManager;

    private void Start()
    {
        // GlobalUndoRedoManagerを取得
        globalManager = FindObjectOfType<GlobalUndoRedoManager>();
    }

    public void OnGrab()
    {
        // オブジェクトを掴んだ際に現在の状態を保存
        globalManager.SaveAction(transform);
    }

    public void OnRelease()
    {
        // 必要ならリリース時の追加処理
    }
}
