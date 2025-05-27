using UnityEngine;

namespace Oculus.Interaction
{
    public class PersistentTubeRenderer : TubeRenderer
    {
        // 親クラスのHide()を上書きする
        public new void Hide()
        {
            // 何も実行しないようにする
            Debug.Log("PersistentTubeRenderer: Hide() is disabled.");
        }
    }
}
