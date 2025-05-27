using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    private Stack<ObjectState> undoStack = new Stack<ObjectState>();
    private Stack<ObjectState> redoStack = new Stack<ObjectState>();

    public void SaveState(Transform transform)
    {
        // 状態を保存し、Redoスタックをクリア
        undoStack.Push(new ObjectState(transform));
        redoStack.Clear();
    }

    public void Undo(Transform transform)
    {
        if (undoStack.Count > 0)
        {
            redoStack.Push(new ObjectState(transform));
            ObjectState state = undoStack.Pop();
            state.ApplyState(transform);
        }
    }

    public void Redo(Transform transform)
    {
        if (redoStack.Count > 0)
        {
            undoStack.Push(new ObjectState(transform));
            ObjectState state = redoStack.Pop();
            state.ApplyState(transform);
        }
    }
}

[System.Serializable]
public class ObjectState
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public ObjectState(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }

    public void ApplyState(Transform transform)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }
}
