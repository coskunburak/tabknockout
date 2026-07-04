using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Room.Tests
{
    public sealed class RuntimeRoomLoaderTests
    {
        [Test]
        public void LoadRoom_ReturnsNull_WhenPrefabMissing()
        {
            var loaderObject = new GameObject("Loader");
            var config = ScriptableObject.CreateInstance<RoomTemplateConfig>();

            try
            {
                var loader = loaderObject.AddComponent<RuntimeRoomLoader>();
                loader.SetUseExistingSceneRoomInstance(false);

                var instance = loader.LoadRoom(config);

                Assert.That(instance, Is.Null);
                Assert.That(loader.ActiveRoomInstance, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(loaderObject);
            }
        }

        [Test]
        public void LoadRoom_InstantiatesPrefabAndExposesActiveContract()
        {
            var loaderObject = new GameObject("Loader");
            var root = new GameObject("RoomInstanceRoot");
            var sourcePrefab = CreateRoomPrefabSource("SourceRoom");
            var config = CreateConfigWithPrefab(sourcePrefab);

            try
            {
                var loader = loaderObject.AddComponent<RuntimeRoomLoader>();
                loader.SetReferences(root.transform, null, null, null);
                loader.SetUseExistingSceneRoomInstance(false);

                var instance = loader.LoadRoom(config);

                Assert.That(instance, Is.Not.Null);
                Assert.That(loader.ActiveRoomInstance, Is.EqualTo(instance));
                Assert.That(loader.ActiveRoomContract, Is.Not.Null);
                Assert.That(instance.transform.parent, Is.EqualTo(root.transform));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(sourcePrefab);
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(loaderObject);
            }
        }

        [Test]
        public void LoadRoom_DestroysPreviousGeneratedInstanceOnly()
        {
            var loaderObject = new GameObject("Loader");
            var root = new GameObject("RoomInstanceRoot");
            var firstSource = CreateRoomPrefabSource("FirstRoom");
            var secondSource = CreateRoomPrefabSource("SecondRoom");
            var firstConfig = CreateConfigWithPrefab(firstSource);
            var secondConfig = CreateConfigWithPrefab(secondSource);

            try
            {
                var loader = loaderObject.AddComponent<RuntimeRoomLoader>();
                loader.SetReferences(root.transform, null, null, null);
                loader.SetUseExistingSceneRoomInstance(false);

                var firstInstance = loader.LoadRoom(firstConfig);
                var firstInstanceObject = firstInstance.gameObject;
                var secondInstance = loader.LoadRoom(secondConfig);

                Assert.That(firstInstanceObject == null, Is.True);
                Assert.That(secondInstance, Is.Not.Null);
                Assert.That(loader.ActiveRoomInstance, Is.EqualTo(secondInstance));
                Assert.That(firstSource, Is.Not.Null);
                Assert.That(secondSource, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(firstConfig);
                Object.DestroyImmediate(secondConfig);
                Object.DestroyImmediate(firstSource);
                Object.DestroyImmediate(secondSource);
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(loaderObject);
            }
        }

        [Test]
        public void LoadRoom_UsesExistingSceneRoomInstance_WhenMatchingPrefabIsAlreadyInScene()
        {
            var loaderObject = new GameObject("Loader");
            var root = new GameObject("RoomInstanceRoot");
            var sourcePrefab = CreateRoomPrefabSource("SourceRoom");
            var sceneRoom = CreateRoomPrefabSource("SourceRoom");
            var config = CreateConfigWithPrefab(sourcePrefab);

            try
            {
                var loader = loaderObject.AddComponent<RuntimeRoomLoader>();
                loader.SetReferences(root.transform, null, null, null);
                var sceneRoomContract = sceneRoom.GetComponent<RoomPrefabContract>();
                var sceneRoomInstance = sceneRoom.AddComponent<RoomInstanceController>();
                sceneRoomInstance.Initialize(sceneRoomContract);
                loader.SetSceneRoomInstance(sceneRoomInstance);

                var instance = loader.LoadRoom(config);

                Assert.That(instance, Is.EqualTo(sceneRoomInstance));
                Assert.That(loader.ActiveRoomInstance, Is.EqualTo(sceneRoomInstance));
                Assert.That(loader.ActiveRoomContract, Is.EqualTo(sceneRoomContract));
                Assert.That(instance.transform.parent, Is.Not.EqualTo(root.transform));

                loader.UnloadActiveRoom();

                Assert.That(sceneRoom, Is.Not.Null);
                Assert.That(loader.ActiveRoomInstance, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(sourcePrefab);
                Object.DestroyImmediate(sceneRoom);
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(loaderObject);
            }
        }

        [Test]
        public void CalculatePlayerRootPositionForSpawn_RaisesCapsuleAboveSpawnPlane()
        {
            var player = new GameObject("Player");

            try
            {
                player.transform.position = new Vector3(0f, -0.74f, 0f);
                var capsule = player.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0.056f, 1.27f, 0.071f);
                capsule.height = 2.842f;
                capsule.radius = 0.36f;

                var spawn = new Vector3(0f, 0.05f, -5.2f);
                var adjusted = RuntimeRoomLoader.CalculatePlayerRootPositionForSpawn(spawn, player.transform, 0.03f);

                Assert.That(adjusted.x, Is.EqualTo(spawn.x));
                Assert.That(adjusted.z, Is.EqualTo(spawn.z));
                Assert.That(adjusted.y, Is.GreaterThan(spawn.y));
                Assert.That(adjusted.y, Is.EqualTo(0.231f).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        private static RoomTemplateConfig CreateConfigWithPrefab(GameObject prefab)
        {
            var config = ScriptableObject.CreateInstance<RoomTemplateConfig>();
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("roomPrefab").objectReferenceValue = prefab;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static GameObject CreateRoomPrefabSource(string name)
        {
            var root = new GameObject(name);
            var contract = root.AddComponent<RoomPrefabContract>();
            var arena = CreateChild(root.transform, RoomPrefabContract.ArenaRootName);
            var playerSpawn = CreateChild(root.transform, RoomPrefabContract.PlayerSpawnName);
            var enemyRoot = CreateChild(root.transform, RoomPrefabContract.EnemySpawnPointsRootName);
            CreateChild(enemyRoot, "SP_Enemy_01");
            var exitGate = CreateChild(root.transform, RoomPrefabContract.ExitGateName);
            var cameraBounds = CreateChild(root.transform, RoomPrefabContract.CameraBoundsName);
            contract.SetReferences(arena, playerSpawn, enemyRoot, new[] { enemyRoot.GetChild(0) }, exitGate, cameraBounds, null, null);
            return root;
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }
    }
}
