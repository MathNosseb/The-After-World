using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public void LoadGameScene(string gameScene)
    {
        SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }
}
