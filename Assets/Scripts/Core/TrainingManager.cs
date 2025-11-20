using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class TrainingManager : MonoBehaviour
    {
        public static TrainingManager Instance { get; private set; }

        [Header("敵人設定")]
        public GameObject[] databugPrefabs;   // 練習用數據蟲 prefabs（多種顏色）
        public Transform[] spawnPoints;       // 空地上的出生點
        public Transform enemiesRoot;         // 生成出來放在哪個父物件底下（例如 EnemiesRoot，可為 null）

        [Header("流程設定")]
        public DialogueController dialogue;   // DialogueSystemRoot 上的 DialogueController
        public string trainingFinishKnot = "training_finish";
        public bool killOneIsEnough = true;   // true: 殺一隻就完成，false: 全部清掉才完成

        readonly List<TrainingBug> aliveBugs = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // 給 Ink: ~ spawn_wave()
        public void StartTraining()
        {
            Debug.Log("[Training] StartTraining: 生成練習用數據蟲");
            ClearOldBugs();

            if (databugPrefabs == null || databugPrefabs.Length == 0 ||
                spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("TrainingManager: databugPrefabs 或 spawnPoints 沒設定");
                return;
            }

            int count = Mathf.Min(3, spawnPoints.Length);

            for (int i = 0; i < count; i++)
            {
                // 隨機從多個顏色裡挑一個
                GameObject prefab = databugPrefabs[Random.Range(0, databugPrefabs.Length)];

                Transform parent = enemiesRoot ? enemiesRoot : null;
                var go = Instantiate(prefab, spawnPoints[i].position, Quaternion.identity, parent);

                var bug = go.GetComponent<TrainingBug>();
                if (!bug) bug = go.AddComponent<TrainingBug>();

                bug.manager = this;
                aliveBugs.Add(bug);
            }
        }

        void ClearOldBugs()
        {
            foreach (var b in aliveBugs)
                if (b) Destroy(b.gameObject);

            aliveBugs.Clear();
        }

        // 被 TrainingBug 呼叫：這隻蟲被淨化了
        public void OnBugPurified(TrainingBug bug)
        {
            aliveBugs.Remove(bug);
            Debug.Log($"[Training] OnBugPurified，剩下 {aliveBugs.Count} 隻");

            if (killOneIsEnough || aliveBugs.Count == 0)
            {
                FinishTraining();
            }
        }

        void FinishTraining()
        {
            ClearOldBugs();

            Debug.Log("[Training] FinishTraining: 播對話三");

            if (dialogue && !string.IsNullOrEmpty(trainingFinishKnot))
            {
                dialogue.StartInkDialogue(trainingFinishKnot);
            }
        }

        // 先給 show_objective / give_camera 用的空實作
        public void ShowObjective(string target, string hint)
        {
            Debug.Log($"[Training] Objective: {target} / 提示: {hint}");
            // TODO：接任務 UI
        }

        public void OnGiveCamera()
        {
            Debug.Log("[Training] 玩家取得相機（UI 由 Inventory / GameFlow 處理）");
            // TODO：如果要記錄 hasCamera，可以在這裡設 bool
        }
    }
}
