using System.Collections.Generic;
using TapKnockout.Room;
using UnityEngine;

namespace TapKnockout.Level
{
    [CreateAssetMenu(fileName = "ChapterConfig", menuName = "Tap Knockout/Chapters/Chapter Config")]
    public sealed class ChapterConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string chapterId = "chapter_001";
        [SerializeField] private string displayName = "Chapter 1";
        [SerializeField, Min(1)] private int chapterIndex = 1;

        [Header("Rooms")]
        [SerializeField] private List<RoomTemplateConfig> rooms = new List<RoomTemplateConfig>();

        [Header("Progression")]
        [SerializeField, Min(0)] private int recommendedPower = 1;

        public string ChapterId => chapterId;
        public string DisplayName => displayName;
        public int ChapterIndex => chapterIndex;
        public IReadOnlyList<RoomTemplateConfig> Rooms => rooms;
        public int RecommendedPower => recommendedPower;
        public bool HasRooms => rooms != null && rooms.Count > 0;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(chapterId))
            {
                chapterId = "chapter_001";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "Chapter 1";
            }

            chapterIndex = Mathf.Max(1, chapterIndex);
            recommendedPower = Mathf.Max(0, recommendedPower);
            rooms ??= new List<RoomTemplateConfig>();
        }
    }
}
