using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] Image loadingBar;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadAsyncOperation());
    }

    IEnumerator LoadAsyncOperation()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("TestScene");

        while (!async.isDone)
        {
            loadingBar.fillAmount = async.progress;
            yield return new WaitForEndOfFrame();
        }
    }
}
