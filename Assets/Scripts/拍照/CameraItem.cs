// CameraItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Camera", menuName = "Inventory/CameraItem")]
public class CameraItem : Item
{
    public CameraSimulation cameraSimulationPrefab;

    

    public override void UseItem()
    {
        if (cameraSimulationPrefab == null)
        {
            Debug.LogError("CameraItem 上的 " + name + " 沒有設定 'cameraSimulationPrefab'！");
            return;
        }

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        CameraSimulation uiInstance = Instantiate(cameraSimulationPrefab, mainCanvas.transform);

        
    }
}