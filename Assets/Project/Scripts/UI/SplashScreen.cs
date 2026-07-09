using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>Shows the splash briefly, then routes to the main menu (§4.4.1 lifecycle).</summary>
public class SplashScreen : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private float displaySeconds = 2.5f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(displaySeconds);
        SceneManager.LoadScene(nextSceneName);
    }
}
