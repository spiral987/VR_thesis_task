using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private Transform doll; // Doll��Transform

    [SerializeField] private RectTransform handleRect; // �܂݃I�u�W�F�N�g��RectTransform
    [SerializeField] private Slider slider;           // �Ή�����Slider

    [SerializeField] private float maxSpeed = 20f; // �ő��]���x�i�x/�b�j

    private float positionXMin = -80f;                // RectTransform��X���W�̍ŏ��l
    private float positionXMax = 80f;                 // RectTransform��X���W�̍ő�l

    private void Update()
    {
        // �܂݂̌��݂�X���W���擾
        float currentX = handleRect.anchoredPosition.x;

        // RectTransform��X���W��Slider�l�Ƀ}�b�s���O
        float sliderValue = Mathf.Lerp(slider.minValue, slider.maxValue, Mathf.InverseLerp(positionXMin, positionXMax, currentX));

        // Slider�̒l���X�V
        slider.value = sliderValue;

        // ��]���x���v�Z�i|sliderValue| ���傫���قǑ��x���傫���j
        float rotationSpeed = Mathf.Lerp(0, maxSpeed, Mathf.Abs(sliderValue) / 180f);

        // ��]�����̌��� (sliderValue�����̏ꍇ�͎��v���A���̏ꍇ�͔����v���)
        float rotationDirection = -Mathf.Sign(sliderValue); // -1: ���v���, 1: �����v���

        // ��]�ʂ��v�Z (���Ԃŕ␳���Ĉ�葬�x�ɂ���)
        float deltaRotation = rotationDirection * rotationSpeed * Time.deltaTime;

        // Doll��Y�����S�ɉ�]������
        doll.Rotate(0, deltaRotation, 0);
    }

    /// <summary>
    /// Doll�̉�]�� x:0, y:180, z:0 �Ƀ��Z�b�g���郁�\�b�h
    /// </summary>
    public void ResetDollRotation()
    {
        if (doll != null)
        {
            doll.rotation = Quaternion.Euler(0, 180, 0);
            Debug.Log("Doll's rotation has been reset to x:0, y:180, z:0");
        }
        else
        {
            Debug.LogWarning("Doll Transform is not assigned in the inspector!");
        }
    }
}
