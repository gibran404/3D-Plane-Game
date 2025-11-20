using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneManagerSingleton : MonoBehaviour
{
    public void StartGame()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(next);
        }
        else
        {
            Debug.LogWarning("StartGame(): there is no next scene in Build Settings.");
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
