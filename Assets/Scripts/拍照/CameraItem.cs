using UnityEngine;
using UnityEngine.EventSystems; 
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Camera", menuName = "Inventory/CameraItem")]
public class CameraItem : Item
{
    public CameraSimulation cameraSimulationPrefab;

    

    public override void UseItem()
    {
        //if (cameraSimulationPrefab == null)
        //{
        //    Debug.LogError("CameraItem 上的 " + name + " 沒有設定 'cameraSimulationPrefab'！");
        //    return;
        //}

        //Canvas mainCanvas = FindObjectOfType<Canvas>();
        //CameraSimulation uiInstance = Instantiate(cameraSimulationPrefab, mainCanvas.transform);

        
    }
    public void UseItemAtPosition(Vector2 screenPosition)
    {
        //防呆機制
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            //一：如果步驟根本不對，直接攔截不給用
            if (TutorialManager.Instance.CurrentStepIndex != TutorialManager.Instance.dragStepIndex)
            {
                Debug.LogWarning("目前不是拖曳相機的時機！");
                return;
            }

            //二：如果是拖曳相機步驟，檢查有沒有精確拖到「指定的教學格子」上
            if (!IsPointerOverTutorialSlot(screenPosition))
            {
                Debug.LogWarning("放錯地方了！請把相機拖曳到閃爍的指定格子內。");
                return; // 位置不對，直接跳出，不會開啟相機視窗
            }

            //成功過關：拖對地方了，教學自動進入下一步
            Debug.Log("相機成功拖曳至正確位置，教學前進至下一階段。");
            TutorialManager.Instance.NextStep();
        }

        // 通過防呆（或是教學沒開啟時），才允許生成相機視窗
        SpawnCameraWindow(screenPosition);
    }

    private void SpawnCameraWindow(Vector2 pos)
    {
        if (cameraSimulationPrefab == null)
        {
            Debug.LogError("CameraItem 上的 " + name + " 沒有設定 'cameraSimulationPrefab'！");
            return;
        }

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        CameraSimulation uiInstance = Instantiate(cameraSimulationPrefab, mainCanvas.transform);

        // 設定視窗位置到滑鼠放開的地方
        uiInstance.transform.position = pos;
    }
    //偵測滑鼠放開的位置是否落在正確的 UI 格子上
    private bool IsPointerOverTutorialSlot(Vector2 screenPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (var result in results)
        {
            // 這裡的 "TutorialSlot" 請改成妳在 Hierarchy 裡「目標教學格子」的物件名稱！
            // 或者妳可以給那個格子加上特定的 Tag 判定：result.gameObject.CompareTag("TutorialGrid")
            if (result.gameObject.name == "InventorySlot(Clone)")
            {
                return true;
            }
        }
        return false;
    }
}