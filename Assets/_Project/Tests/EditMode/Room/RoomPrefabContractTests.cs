using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Room.Tests
{
    public sealed class RoomPrefabContractTests
    {
        [Test]
        public void TryValidate_ReturnsTrue_WhenRequiredContractChildrenExist()
        {
            var root = new GameObject(RoomPrefabContract.RoomRootName);

            try
            {
                var contract = root.AddComponent<RoomPrefabContract>();
                CreateChild(root.transform, RoomPrefabContract.ArenaRootName);
                CreateChild(root.transform, RoomPrefabContract.PlayerSpawnName);
                var enemyRoot = CreateChild(root.transform, RoomPrefabContract.EnemySpawnPointsRootName);
                CreateChild(enemyRoot, "SP_Enemy_01");
                CreateChild(root.transform, RoomPrefabContract.ExitGateName);
                CreateChild(root.transform, RoomPrefabContract.CameraBoundsName);

                var isValid = contract.TryValidate(out var message);

                Assert.That(isValid, Is.True);
                Assert.That(message, Is.EqualTo("Room prefab contract is valid."));
                Assert.That(contract.HasRequiredReferences, Is.True);
                Assert.That(contract.EnemySpawnPoints.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TryValidate_ReturnsFalse_WhenEnemySpawnPointsAreMissing()
        {
            var root = new GameObject(RoomPrefabContract.RoomRootName);

            try
            {
                var contract = root.AddComponent<RoomPrefabContract>();
                CreateChild(root.transform, RoomPrefabContract.ArenaRootName);
                CreateChild(root.transform, RoomPrefabContract.PlayerSpawnName);
                CreateChild(root.transform, RoomPrefabContract.EnemySpawnPointsRootName);
                CreateChild(root.transform, RoomPrefabContract.ExitGateName);
                CreateChild(root.transform, RoomPrefabContract.CameraBoundsName);

                var isValid = contract.TryValidate(out var message);

                Assert.That(isValid, Is.False);
                Assert.That(message, Does.Contain(RoomPrefabContract.EnemySpawnPointsRootName));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TryValidate_RebuildsEnemySpawnPointsFromConfiguredRoot()
        {
            var root = new GameObject(RoomPrefabContract.RoomRootName);

            try
            {
                var contract = root.AddComponent<RoomPrefabContract>();
                var arena = CreateChild(root.transform, RoomPrefabContract.ArenaRootName);
                var playerSpawn = CreateChild(root.transform, RoomPrefabContract.PlayerSpawnName);
                var enemyRoot = CreateChild(root.transform, RoomPrefabContract.EnemySpawnPointsRootName);
                var staleSpawn = CreateChild(root.transform, "SP_Enemy_Stale");
                var exitGate = CreateChild(root.transform, RoomPrefabContract.ExitGateName);
                var cameraBounds = CreateChild(root.transform, RoomPrefabContract.CameraBoundsName);

                contract.SetReferences(
                    arena,
                    playerSpawn,
                    enemyRoot,
                    new[] { staleSpawn },
                    exitGate,
                    cameraBounds,
                    null,
                    null);

                var isValid = contract.TryValidate(out var message);

                Assert.That(isValid, Is.False);
                Assert.That(message, Does.Contain(RoomPrefabContract.EnemySpawnPointsRootName));
                Assert.That(contract.EnemySpawnPoints.Count, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TryValidate_ResolvesWrongNamedReferencesToContractChildren()
        {
            var root = new GameObject(RoomPrefabContract.RoomRootName);

            try
            {
                var contract = root.AddComponent<RoomPrefabContract>();
                var arena = CreateChild(root.transform, RoomPrefabContract.ArenaRootName);
                var playerSpawn = CreateChild(root.transform, RoomPrefabContract.PlayerSpawnName);
                var wrongEnemyRoot = CreateChild(root.transform, "WrongEnemySpawnRoot");
                var correctEnemyRoot = CreateChild(root.transform, RoomPrefabContract.EnemySpawnPointsRootName);
                var correctSpawn = CreateChild(correctEnemyRoot, "SP_Enemy_01");
                var staleSpawn = CreateChild(wrongEnemyRoot, "SP_Enemy_Stale");
                var exitGate = CreateChild(root.transform, RoomPrefabContract.ExitGateName);
                var cameraBounds = CreateChild(root.transform, RoomPrefabContract.CameraBoundsName);

                contract.SetReferences(
                    arena,
                    playerSpawn,
                    wrongEnemyRoot,
                    new[] { staleSpawn },
                    exitGate,
                    cameraBounds,
                    null,
                    null);

                var isValid = contract.TryValidate(out var message);

                Assert.That(isValid, Is.True);
                Assert.That(message, Is.EqualTo("Room prefab contract is valid."));
                Assert.That(contract.EnemySpawnPointsRoot, Is.EqualTo(correctEnemyRoot));
                Assert.That(contract.EnemySpawnPoints, Is.EquivalentTo(new[] { correctSpawn }));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }
    }
}
