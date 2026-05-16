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
        // 教學防呆
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            int currentStep = TutorialManager.Instance.CurrentStepIndex;

            // 如果比拖曳步驟還早，直接封鎖不給用
            if (currentStep < TutorialManager.Instance.dragStepIndex)
            {
                Debug.LogWarning("目前還不到使用相機的時機！");
                return;
            }

            // 正好在拖曳教學步驟（第7步），嚴格檢查格子並推進教學
            if (currentStep == TutorialManager.Instance.dragStepIndex)
            {
                if (!IsPointerOverTutorialSlot(screenPosition))
                {
                    Debug.LogWarning("放錯地方了！請把相機拖曳到正確的背包格子內。");
                    return;
                }

                Debug.Log("相機成功拖曳至正確位置，教學進入下一階段！");
                TutorialManager.Instance.NextStep(); 
            }

        }

        // 通過教學防呆（或正常遊戲沒開教學時），順利生成相機視窗
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
        uiInstance.transform.position = pos;
    }

    // 動態偵測滑鼠放開時是否在複製出來的格子上
    private bool IsPointerOverTutorialSlot(Vector2 screenPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (var result in results)
        {
            if (result.gameObject.name == "InventorySlot(Clone)")
            {
                return true;
            }
        }
        return false;
    }
}