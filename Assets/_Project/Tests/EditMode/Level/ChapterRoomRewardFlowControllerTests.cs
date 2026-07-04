using NUnit.Framework;
using TapKnockout.Ability;
using TapKnockout.Combat;
using TapKnockout.Level;
using TapKnockout.Player;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Level.Tests
{
    public sealed class ChapterRoomRewardFlowControllerTests
    {
        [Test]
        public void CombatRoomComplete_StartsNextRoomWhenNoReward()
        {
            var fixture = CreateFixture(RoomRewardType.None, RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();

                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(1));
                Assert.That(fixture.Runner.CurrentRoomConfig, Is.EqualTo(fixture.Room2));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.CombatRunning));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void AbilityRewardRoomComplete_WaitsForAbilitySelection()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();

                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.FlowController.IsWaitingForAbilitySelection, Is.True);
                Assert.That(fixture.Runner.RunState.IsAbilitySelectionPending, Is.True);
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(0));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.AbilitySelectionPending));
                Assert.That(Time.timeScale, Is.EqualTo(0f));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }
        }

        [Test]
        public void AbilitySelected_AutoContinuesToNextRoom()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);

            try
            {
                SetAutoContinueAfterAbilitySelection(fixture.FlowController, true);
                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.SelectionController.SelectOffer(0), Is.True);

                Assert.That(fixture.FlowController.IsWaitingForAbilitySelection, Is.False);
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(1));
                Assert.That(fixture.Runner.RunState.IsRewardPending, Is.False);
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.CombatRunning));
                Assert.That(Time.timeScale, Is.EqualTo(previousTimeScale));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }
        }

        [Test]
        public void AbilitySelected_DefaultManualMode_WaitsForContinue()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.SelectionController.SelectOffer(0), Is.True);

                Assert.That(fixture.FlowController.IsWaitingForAbilitySelection, Is.False);
                Assert.That(fixture.FlowController.CanContinueAfterReward, Is.True);
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(0));
                Assert.That(fixture.Runner.RunState.IsWaitingForContinue, Is.True);
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.WaitingForContinue));
                Assert.That(Time.timeScale, Is.EqualTo(previousTimeScale));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }
        }

        [Test]
        public void ContinueAfterReward_WhenWaitingForContinue_StartsNextRoom()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();
                fixture.SelectionController.SelectOffer(0);

                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(1));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.CombatRunning));
                Assert.That(fixture.Runner.RunState.IsRewardPending, Is.False);
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }
        }

        [Test]
        public void TryContinueAfterReward_WhenRewardPendingWithoutOffer_RepairsStateAndStartsNextRoom()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();
                fixture.SelectionController.SelectOffer(0);

                fixture.Runner.RunState.ClearRewardState();
                fixture.Runner.RunState.MarkRewardPending();
                fixture.Runner.SetFlowState(ChapterFlowState.RewardPending);

                Assert.That(fixture.FlowController.CanContinueAfterReward, Is.True);
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.WaitingForContinue));

                Assert.That(fixture.FlowController.TryContinueAfterReward(), Is.True);
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(1));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.CombatRunning));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }
        }

        [Test]
        public void ContinueAfterReward_BeforeRewardResolved_DoesNotAdvance()
        {
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();

                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(0));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.CombatRunning));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void ContinueAfterReward_WhileAbilitySelectionPending_DoesNotAdvance()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();

                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(0));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.AbilitySelectionPending));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }
        }

        [Test]
        public void ContinueAfterReward_AfterChapterFailed_DoesNotAdvance()
        {
            var fixture = CreateFixture(RoomRewardType.None, RoomRewardType.None, includePlayerHealth: true);

            try
            {
                fixture.Runner.StartChapter();
                fixture.PlayerHealth.ResetHealth();
                fixture.PlayerHealth.ReceiveHit(new HitContext(null, fixture.PlayerHealth.gameObject, 999f));

                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.IsChapterFailed, Is.True);
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(0));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void ContinueAfterReward_AfterChapterCompleted_DoesNotChangeState()
        {
            var fixture = CreateFixture(RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();

                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.IsChapterCompleted, Is.True);
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.ChapterCompleted));
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(0));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void AbilitySelected_ManualMode_RaisesRoomExitUnlocked()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);
            var eventCount = 0;

            try
            {
                ChapterProgressionEvents.OnRoomExitUnlocked += HandleRoomExitUnlocked;
                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();

                fixture.SelectionController.SelectOffer(0);

                Assert.That(eventCount, Is.EqualTo(1));
            }
            finally
            {
                ChapterProgressionEvents.OnRoomExitUnlocked -= HandleRoomExitUnlocked;
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }

            void HandleRoomExitUnlocked(ChapterRoomProgressionEventArgs eventArgs)
            {
                eventCount++;
                Assert.That(eventArgs.RoomIndex, Is.EqualTo(0));
                Assert.That(eventArgs.NextRoomIndex, Is.EqualTo(1));
                Assert.That(eventArgs.TotalRoomCount, Is.EqualTo(2));
                Assert.That(eventArgs.RoomId, Is.EqualTo("room_01"));
            }
        }

        [Test]
        public void ContinueAfterReward_RaisesTransitionEventsOnce()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(RoomRewardType.Ability, RoomRewardType.None);
            var requestedCount = 0;
            var transitionStartedCount = 0;
            var transitionCompletedCount = 0;

            try
            {
                ChapterProgressionEvents.OnNextRoomRequested += HandleNextRoomRequested;
                ChapterProgressionEvents.OnRoomTransitionStarted += HandleTransitionStarted;
                ChapterProgressionEvents.OnRoomTransitionCompleted += HandleTransitionCompleted;

                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();
                fixture.SelectionController.SelectOffer(0);

                fixture.FlowController.ContinueAfterReward();
                fixture.FlowController.ContinueAfterReward();

                Assert.That(requestedCount, Is.EqualTo(1));
                Assert.That(transitionStartedCount, Is.EqualTo(1));
                Assert.That(transitionCompletedCount, Is.EqualTo(1));
            }
            finally
            {
                ChapterProgressionEvents.OnNextRoomRequested -= HandleNextRoomRequested;
                ChapterProgressionEvents.OnRoomTransitionStarted -= HandleTransitionStarted;
                ChapterProgressionEvents.OnRoomTransitionCompleted -= HandleTransitionCompleted;
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }

            void HandleNextRoomRequested(ChapterRoomProgressionEventArgs eventArgs)
            {
                requestedCount++;
            }

            void HandleTransitionStarted(ChapterRoomTransitionEventArgs eventArgs)
            {
                transitionStartedCount++;
                Assert.That(eventArgs.FromRoomIndex, Is.EqualTo(0));
                Assert.That(eventArgs.ToRoomIndex, Is.EqualTo(1));
            }

            void HandleTransitionCompleted(ChapterRoomTransitionEventArgs eventArgs)
            {
                transitionCompletedCount++;
            }
        }

        [Test]
        public void AbilityRoomThenManualNoRewardRoom_AllowsContinueToThirdRoom()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(new[] { RoomRewardType.Ability, RoomRewardType.None, RoomRewardType.None }, false);

            try
            {
                SetAutoAdvanceAfterClear(fixture.Room2, false);

                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();
                fixture.SelectionController.SelectOffer(0);
                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(1));
                Assert.That(fixture.Runner.CurrentRoomConfig, Is.EqualTo(fixture.Room2));

                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.FlowController.CanContinueAfterReward, Is.True);
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(1));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.WaitingForContinue));

                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(2));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.CombatRunning));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }
        }

        [Test]
        public void AbilityCombatAbilitySequence_AllowsContinueToFourthRoom()
        {
            var previousTimeScale = Time.timeScale;
            var fixture = CreateFixture(new[] { RoomRewardType.Ability, RoomRewardType.None, RoomRewardType.Ability, RoomRewardType.None }, false);

            try
            {
                fixture.Runner.StartChapter();
                fixture.RoomManager.ForceCompleteRoom();
                fixture.SelectionController.SelectOffer(0);
                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(1));

                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(2));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.CombatRunning));

                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.FlowController.IsWaitingForAbilitySelection, Is.True);
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.AbilitySelectionPending));

                fixture.SelectionController.SelectOffer(0);

                Assert.That(fixture.FlowController.CanContinueAfterReward, Is.True);
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(2));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.WaitingForContinue));

                fixture.FlowController.ContinueAfterReward();

                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(3));
                Assert.That(fixture.Runner.CurrentRoomConfig, Is.EqualTo(fixture.Rooms[3]));
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.CombatRunning));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Destroy();
            }
        }

        [Test]
        public void LastRoomComplete_CompletesChapter()
        {
            var fixture = CreateFixture(RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();

                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.Runner.IsChapterCompleted, Is.True);
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.ChapterCompleted));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void BossClearReward_CompletesChapter()
        {
            var fixture = CreateFixture(RoomRewardType.BossClear, RoomRewardType.None);

            try
            {
                fixture.Runner.StartChapter();

                fixture.RoomManager.ForceCompleteRoom();

                Assert.That(fixture.Runner.IsChapterCompleted, Is.True);
                Assert.That(fixture.Runner.CurrentRoomIndex, Is.EqualTo(0));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void PlayerDeath_FailsChapterAndBlocksNextRoom()
        {
            var fixture = CreateFixture(RoomRewardType.None, RoomRewardType.None, includePlayerHealth: true);

            try
            {
                fixture.Runner.StartChapter();
                fixture.PlayerHealth.ResetHealth();

                fixture.PlayerHealth.ReceiveHit(new HitContext(null, fixture.PlayerHealth.gameObject, 999f));

                Assert.That(fixture.Runner.IsChapterFailed, Is.True);
                Assert.That(fixture.Runner.FlowState, Is.EqualTo(ChapterFlowState.Failed));
                Assert.That(fixture.Runner.StartNextRoom(), Is.False);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void EmptyChapter_CompletesSafely()
        {
            var runnerObject = new GameObject("Runner");
            var config = ScriptableObject.CreateInstance<ChapterConfig>();

            try
            {
                var runner = runnerObject.AddComponent<ChapterRunner>();
                runner.SetReferences(config, null);

                runner.StartChapter();

                Assert.That(runner.IsChapterCompleted, Is.True);
                Assert.That(runner.FlowState, Is.EqualTo(ChapterFlowState.ChapterCompleted));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(runnerObject);
            }
        }

        [Test]
        public void RequestAbilityOffer_WithMissingReferences_ReturnsFalse()
        {
            var controllerObject = new GameObject("Flow");

            try
            {
                var controller = controllerObject.AddComponent<ChapterRoomRewardFlowController>();

                Assert.That(controller.RequestAbilityOffer(), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }

        private static FlowFixture CreateFixture(params RoomRewardType[] rewards)
        {
            return CreateFixture(rewards, false);
        }

        private static FlowFixture CreateFixture(RoomRewardType reward1, RoomRewardType reward2, bool includePlayerHealth = false)
        {
            return CreateFixture(new[] { reward1, reward2 }, includePlayerHealth);
        }

        private static FlowFixture CreateFixture(RoomRewardType[] rewards, bool includePlayerHealth)
        {
            var fixture = new FlowFixture
            {
                RunnerObject = new GameObject("Runner"),
                RoomObject = new GameObject("Room"),
                SelectionObject = new GameObject("Selection"),
                FlowObject = new GameObject("Flow"),
                PlayerObject = includePlayerHealth ? new GameObject("Player") : null,
                Chapter = ScriptableObject.CreateInstance<ChapterConfig>(),
                Ability = CreateAbility("attack_damage_up")
            };

            fixture.Runner = fixture.RunnerObject.AddComponent<ChapterRunner>();
            fixture.RoomManager = fixture.RoomObject.AddComponent<RoomManager>();
            fixture.SelectionController = fixture.SelectionObject.AddComponent<AbilitySelectionController>();
            fixture.FlowController = fixture.FlowObject.AddComponent<ChapterRoomRewardFlowController>();
            fixture.PlayerHealth = fixture.PlayerObject != null ? fixture.PlayerObject.AddComponent<PlayerHealth>() : null;

            fixture.SelectionController.SetAbilityPool(new[] { fixture.Ability });

            var rooms = new RoomTemplateConfig[rewards.Length];
            for (var i = 0; i < rewards.Length; i++)
            {
                rooms[i] = CreateRoom($"room_{i + 1:00}", rewards[i]);
            }

            fixture.Room1 = rooms.Length > 0 ? rooms[0] : null;
            fixture.Room2 = rooms.Length > 1 ? rooms[1] : null;
            fixture.Rooms = rooms;
            SetChapterRooms(fixture.Chapter, rooms);

            fixture.Runner.SetReferences(fixture.Chapter, fixture.RoomManager);
            fixture.FlowController.SetReferences(
                fixture.Runner,
                fixture.RoomManager,
                fixture.SelectionController,
                null,
                fixture.PlayerHealth);

            return fixture;
        }

        private static RoomTemplateConfig CreateRoom(string roomId, RoomRewardType rewardType)
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateConfig>();
            var serializedObject = new SerializedObject(room);
            serializedObject.FindProperty("roomId").stringValue = roomId;
            serializedObject.FindProperty("rewardType").enumValueIndex = (int)rewardType;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return room;
        }

        private static AbilityDefinition CreateAbility(string abilityId)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var serializedObject = new SerializedObject(ability);
            serializedObject.FindProperty("abilityId").stringValue = abilityId;
            serializedObject.FindProperty("displayName").stringValue = abilityId;
            serializedObject.FindProperty("maxStacks").intValue = 5;
            serializedObject.FindProperty("weight").floatValue = 100f;
            serializedObject.FindProperty("isEnabled").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return ability;
        }

        private static void SetAutoContinueAfterAbilitySelection(ChapterRoomRewardFlowController controller, bool value)
        {
            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("autoContinueAfterAbilitySelection").boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetAutoAdvanceAfterClear(RoomTemplateConfig room, bool value)
        {
            var serializedObject = new SerializedObject(room);
            serializedObject.FindProperty("autoAdvanceAfterClear").boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetChapterRooms(ChapterConfig chapter, RoomTemplateConfig[] rooms)
        {
            var serializedObject = new SerializedObject(chapter);
            var roomsProperty = serializedObject.FindProperty("rooms");
            roomsProperty.arraySize = rooms.Length;
            for (var i = 0; i < rooms.Length; i++)
            {
                roomsProperty.GetArrayElementAtIndex(i).objectReferenceValue = rooms[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class FlowFixture
        {
            public GameObject RunnerObject;
            public GameObject RoomObject;
            public GameObject SelectionObject;
            public GameObject FlowObject;
            public GameObject PlayerObject;
            public ChapterConfig Chapter;
            public RoomTemplateConfig Room1;
            public RoomTemplateConfig Room2;
            public RoomTemplateConfig[] Rooms;
            public AbilityDefinition Ability;
            public ChapterRunner Runner;
            public RoomManager RoomManager;
            public AbilitySelectionController SelectionController;
            public ChapterRoomRewardFlowController FlowController;
            public PlayerHealth PlayerHealth;

            public void Destroy()
            {
                if (Rooms != null)
                {
                    for (var i = 0; i < Rooms.Length; i++)
                    {
                        Object.DestroyImmediate(Rooms[i]);
                    }
                }

                Object.DestroyImmediate(Ability);
                Object.DestroyImmediate(Chapter);
                Object.DestroyImmediate(PlayerObject);
                Object.DestroyImmediate(FlowObject);
                Object.DestroyImmediate(SelectionObject);
                Object.DestroyImmediate(RoomObject);
                Object.DestroyImmediate(RunnerObject);
            }
        }
    }
}
