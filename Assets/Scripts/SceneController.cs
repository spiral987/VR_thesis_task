using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // �w�肵���V�[���ɑJ�ڂ��郁�\�b�h
    public void LoadScene(int sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ���݂̃V�[�����ēǂݍ��݂��郁�\�b�h
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // �A�v���P�[�V�������I�����郁�\�b�h�i�r���h���̂ݓ���j
    public void QuitApplication()
    {
        Application.Quit();
        Debug.Log("Application Quit");
    }
}
