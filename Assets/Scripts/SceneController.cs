using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // 指定したシーンに遷移するメソッド
    public void LoadScene(int sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 現在のシーンを再読み込みするメソッド
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // アプリケーションを終了するメソッド（ビルド時のみ動作）
    public void QuitApplication()
    {
        Application.Quit();
        Debug.Log("Application Quit");
    }
}
