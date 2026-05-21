using UnityEngine;
using UnityEngine.SceneManagement;

public class MapLocationIndicator : MonoBehaviour
{
    [Header("地圖上的定位圖示")]
    public GameObject pinTutorial;  // 新手區
    public GameObject pinPlaza;     // 圖像廣場
    public GameObject pinPhantom;   // 幻影巷
    public GameObject pinLake;      // 湖中怪屋
    public GameObject pinBase;    // 作品集偷偷的基地

    [Header("場景精確名稱")]
    public string sceneTutorial = "(2D)新手區 1";
    public string scenePlaza = "(2D)圖像廣場 1";
    public string scenePhantom = "(2D)幻影巷 1";
    public string sceneLake = "(2D)湖中怪屋 1";
    public string sceneBase = "(2D)作品集偷偷的基地 1";

    // 使用 OnEnable 而不是 Start，這樣玩家每次「打開」地圖時都會重新偵測一次
    void OnEnable()
    {
        // 1. 先把所有標記都強制隱藏，避免殘影
        if (pinTutorial) pinTutorial.SetActive(false);
        if (pinPlaza) pinPlaza.SetActive(false);
        if (pinPhantom) pinPhantom.SetActive(false);
        if (pinLake) pinLake.SetActive(false);
        if (pinBase) pinBase.SetActive(false);

        // 2. 取得玩家現在所在的場景名稱
        string currentScene = SceneManager.GetActiveScene().name;

        // 3. 比對場景名稱，打開對應的定位圖示
        if (currentScene == sceneTutorial && pinTutorial)
            pinTutorial.SetActive(true);
        else if (currentScene == scenePlaza && pinPlaza)
            pinPlaza.SetActive(true);
        else if (currentScene == scenePhantom && pinPhantom)
            pinPhantom.SetActive(true);
        else if (currentScene == sceneLake && pinLake)
            pinLake.SetActive(true);
        else if (currentScene == sceneBase && pinBase)
            pinBase.SetActive(true);
    }
}