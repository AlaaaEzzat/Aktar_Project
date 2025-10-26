using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class WinGate : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.transform.DORotate(new Vector3(0, 0, 360f), 1f, RotateMode.FastBeyond360);
            collision.gameObject.transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
            {
                UiManager.Instance.WinEndGame();
                LevelManager.Instance.IncreaseLevelOpen(SceneManager.GetActiveScene().buildIndex);
            });
            //LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
