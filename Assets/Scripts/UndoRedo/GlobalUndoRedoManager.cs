using System.Collections.Generic;
using UnityEngine;

public class GlobalUndoRedoManager : MonoBehaviour
{
    private Stack<ObjectAction> undoStack = new Stack<ObjectAction>();
    private Stack<ObjectAction> redoStack = new Stack<ObjectAction>();

    // ó‘Ô‚ð•Û‘¶
    public void SaveAction(Transform transform)
    {
        undoStack.Push(new ObjectAction(transform));
        redoStack.Clear(); // V‚µ‚¢‘€ì‚ª“ü‚é‚ÆRedo‚Í–³Œø‰»
    }

    // Undoˆ—
    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            ObjectAction action = undoStack.Pop();
            redoStack.Push(new ObjectAction(action.targetTransform));
            action.state.ApplyState(action.targetTransform);
        }
    }

    // Redoˆ—
    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            ObjectAction action = redoStack.Pop();
            undoStack.Push(new ObjectAction(action.targetTransform));
            action.state.ApplyState(action.targetTransform);
        }
    }
}




[System.Serializable]
public class ObjectAction
{
    public Transform targetTransform;
    public ObjectState state;

    public ObjectAction(Transform transform)
    {
        targetTransform = transform;
        state = new ObjectState(transform);
    }
}

