using UnityEngine;

namespace Oculus.Interaction
{
    public class PersistentTubeRenderer : TubeRenderer
    {
        // �e�N���X��Hide()���㏑������
        public new void Hide()
        {
            // �������s���Ȃ��悤�ɂ���
            Debug.Log("PersistentTubeRenderer: Hide() is disabled.");
        }
    }
}
