using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreScene : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float beginningDelay = 1.0f;
    [SerializeField] float sceneDuration = 5.0f;
    [SerializeField] float spawnSpeed = 0.2f;

    [Header("References")]
    [SerializeField] GameObject dollarPrefab = null;
    [SerializeField] TextMeshProUGUI topBanner = null;
    [SerializeField] TextMeshProUGUI bottomBanner = null;
    [SerializeField] ScoreSpawner objectSpawner = null;
    [SerializeField] Animator stageAnimator = null;

    GameInfo gameInfo = null;
    MapManager mapManager = null;

    List<GameObject> sceneObjects = new List<GameObject>();

    private void Start()
    {
        Time.timeScale = 1.0f;

        gameInfo = GameInfo.GetInstance();
        mapManager = MapManager.GetInstance();

        StartCoroutine(DoCutscene());
    }

    IEnumerator DoCutscene()
    {
        stageAnimator.SetBool("StageUp", true);
        yield return new WaitForSeconds(beginningDelay);

        topBanner.text = "Products sold";
        bottomBanner.text = "";

        for (int prod = 0; prod < gameInfo.GetProductsSold().Length; prod++)
        {
            sceneObjects.Add(
                objectSpawner.SpawnObject(
                    mapManager.GetStockTypePrefab(gameInfo.GetProductsSold()[prod]
                    )
                    )
                );

            bottomBanner.text = (prod + 1).ToString() + " products";

            yield return new WaitForSeconds(spawnSpeed);
        }

        bottomBanner.text = gameInfo.GetProductsSold().Length.ToString() + " products";

        yield return new WaitForSeconds(sceneDuration);

        stageAnimator.SetBool("StageUp", false);
        yield return new WaitForSeconds(3.0f);

        

        ClearSceneObjects();

        topBanner.text = "Profits";
        bottomBanner.text = "";

        stageAnimator.SetBool("StageUp", true);
        yield return new WaitForSeconds(1.0f);

        for (int cash = 0; cash < PlayerData.GetCurrentDataInfo().profit; cash++)
        {
            sceneObjects.Add(
                objectSpawner.SpawnObject(dollarPrefab)
                );

            bottomBanner.text = "$" + (cash + 1).ToString() + " made";

            yield return new WaitForSeconds(spawnSpeed);
        }

        bottomBanner.text = "$" + PlayerData.GetCurrentDataInfo().profit.ToString() + " made";
        yield return new WaitForSeconds(sceneDuration);

        stageAnimator.SetBool("StageUp", false);
        yield return new WaitForSeconds(3.0f);

        

        ClearSceneObjects();

        SceneManager.LoadSceneAsync("MainMenu");

    }

    void ClearSceneObjects()
    {
        for(int i = 0; i < sceneObjects.Count; i++)
        {
            Destroy(sceneObjects[i]);
        }

        sceneObjects.Clear();
    }
}
