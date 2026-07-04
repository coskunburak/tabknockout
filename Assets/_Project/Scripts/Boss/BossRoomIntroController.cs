using System.Collections;
using TapKnockout.Room;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossRoomIntroController : MonoBehaviour, IRoomStartIntro
    {
        [SerializeField] private BossRoomIntroConfig config;
        [SerializeField, Min(0f)] private float fallbackIntroDelay = 0.5f;
        [SerializeField] private bool playOnlyForBossRooms = true;
        [SerializeField] private bool logDebug;

        public bool IsIntroEnabled => enabled && (config != null || fallbackIntroDelay > 0f);

        public IEnumerator PlayIntro(RoomTemplateConfig roomConfig, RoomPrefabContract roomContract)
        {
            if (!IsIntroEnabled)
            {
                yield break;
            }

            if (playOnlyForBossRooms && (roomConfig == null || !roomConfig.IsBossRoom))
            {
                yield break;
            }

            var warning = SpawnWarningVisual(roomContract);
            var delay = config != null ? config.IntroDelay : fallbackIntroDelay;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (warning != null)
            {
                Destroy(warning);
            }

            if (logDebug)
            {
                Debug.Log($"{nameof(BossRoomIntroController)} completed boss intro for {roomConfig?.RoomId ?? "<null>"}.", this);
            }
        }

        private GameObject SpawnWarningVisual(RoomPrefabContract roomContract)
        {
            if (config == null || config.WarningVisualPrefab == null)
            {
                return null;
            }

            var spawnPoint = roomContract != null
                ? roomContract.GetBossSpawnPoint() ?? roomContract.GetRewardSpawnPoint() ?? roomContract.GetPlayerEntryPoint()
                : transform;
            var warning = Instantiate(config.WarningVisualPrefab, spawnPoint.position, spawnPoint.rotation);
            if (config.WarningVisualLifetime > 0f)
            {
                Destroy(warning, config.WarningVisualLifetime);
            }

            return warning;
        }
    }
}
