using TapKnockout.Enemy;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor
{
    public static class EnemyAttackTestHarnessSpawner
    {
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string EnemyConfigRoot = "Assets/_Project/ScriptableObjects/Enemies/CuteMonsters";
        private const string EnemyPrefabRoot = "Assets/_Project/Prefabs/Enemies/CuteMonsters";
        private const string HarnessName = "EnemyAttackTestHarness";

        private static readonly HarnessEnemy[] Enemies =
        {
            new HarnessEnemy("Bat", "EnemyConfig_Bat", "PF_Enemy_Bat", 4.5f),
            new HarnessEnemy("Bee", "EnemyConfig_Bee", "PF_Enemy_Bee", 4f),
            new HarnessEnemy("GreenDemon", "EnemyConfig_GreenDemon", "PF_Enemy_BasicMelee_GreenDemon_Generated", 1.25f),
            new HarnessEnemy("YellowDragon", "EnemyConfig_YellowDragon_Boss", "PF_Boss_YellowDragon", 5f),
            new HarnessEnemy("Cactus", "EnemyConfig_Cactus", "PF_Enemy_Cactus", 3.5f),
            new HarnessEnemy("Cthulhu", "EnemyConfig_Cthulhu", "PF_Enemy_Cthulhu", 5f),
            new HarnessEnemy("Cyclops", "EnemyConfig_Cyclops", "PF_Enemy_Cyclops", 5.5f),
            new HarnessEnemy("Demon", "EnemyConfig_Demon", "PF_Enemy_Demon", 3.5f),
            new HarnessEnemy("Ghost", "EnemyConfig_Ghost", "PF_Enemy_Ghost", 5f),
            new HarnessEnemy("Mushroom", "EnemyConfig_Mushroom", "PF_Enemy_Mushroom", 4.5f),
            new HarnessEnemy("Yeti", "EnemyConfig_Yeti", "PF_Enemy_Yeti", 2.4f)
        };

        [MenuItem("Tap Knockout/Combat/Spawn Enemy Attack Test Harness", priority = 202)]
        public static void SpawnHarness()
        {
            EnemyAttackMechanicsBuilder.BuildAll();

            var existing = GameObject.Find(HarnessName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            var root = new GameObject(HarnessName);
            Undo.RegisterCreatedObjectUndo(root, "Create Enemy Attack Test Harness");

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError($"[EnemyAttackTestHarness] Player prefab missing at {PlayerPrefabPath}.");
                return;
            }

            var player = PrefabUtility.InstantiatePrefab(playerPrefab, root.transform) as GameObject;
            if (player == null)
            {
                player = Object.Instantiate(playerPrefab, root.transform);
            }

            player.name = "Harness_PlayerTarget";
            player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            Undo.RegisterCreatedObjectUndo(player, "Create Harness Player");

            for (var i = 0; i < Enemies.Length; i++)
            {
                SpawnEnemy(root.transform, player.transform, Enemies[i], i, Enemies.Length);
            }

            Selection.activeGameObject = root;
            Debug.Log("[EnemyAttackTestHarness] Spawned player target and all cute monster enemy attack prefabs. Press Play to verify telegraphs, VFX, projectiles, zones, and PlayerHealth damage.");
        }

        private static void SpawnEnemy(Transform root, Transform player, HarnessEnemy enemy, int index, int count)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EnemyPrefabRoot}/{enemy.PrefabName}.prefab");
            var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>($"{EnemyConfigRoot}/{enemy.ConfigName}.asset");
            if (prefab == null || config == null)
            {
                Debug.LogError($"[EnemyAttackTestHarness] Missing prefab/config for {enemy.Name}.");
                return;
            }

            var angle = count > 0 ? (Mathf.PI * 2f / count) * index : 0f;
            var position = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * enemy.Distance;
            var instance = PrefabUtility.InstantiatePrefab(prefab, root) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, root);
            }

            instance.name = $"Harness_{enemy.Name}";
            instance.transform.SetPositionAndRotation(position, Quaternion.LookRotation((player.position - position).normalized, Vector3.up));
            Undo.RegisterCreatedObjectUndo(instance, $"Create Harness {enemy.Name}");

            if (instance.TryGetComponent<EnemyController>(out var controller))
            {
                controller.Initialize(config, player);
            }

            var distinct = instance.GetComponent<EnemyDistinctAttackController>();
            if (distinct != null)
            {
                distinct.SetTarget(player);
            }
        }

        private readonly struct HarnessEnemy
        {
            public HarnessEnemy(string name, string configName, string prefabName, float distance)
            {
                Name = name;
                ConfigName = configName;
                PrefabName = prefabName;
                Distance = distance;
            }

            public string Name { get; }
            public string ConfigName { get; }
            public string PrefabName { get; }
            public float Distance { get; }
        }
    }
}
