using System.Collections;

namespace TapKnockout.Room
{
    public interface IRoomStartIntro
    {
        bool IsIntroEnabled { get; }

        IEnumerator PlayIntro(RoomTemplateConfig roomConfig, RoomPrefabContract roomContract);
    }
}
