using System.Collections;
using System.Collections.Generic;
using UnityChan;
using UnityEngine;


public class ChangeGhostPosePractice : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private List<string> animationName = new List<string>(); // �A�j���[�V������




    //�{�^�����������ƃ|�[�Y���Z�b�g�����
    public void ChangePose(int tasknum)
    {
        if (_animator == null)
        {
            Debug.LogError("Animator is not assigned.");
            return;
        }

        _animator.Play(animationName[tasknum]);
    }


}
