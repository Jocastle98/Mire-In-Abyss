using Events.Abyss;
using UIHUDEnums;
using UnityEngine;

namespace Events.HUD
{
    public static class TrackableEventHelper
    {
        /* ── 스폰 ────────────────────────────────── */
        public static void PublishSpawned(Object entity)
        {
            var id = entity.GetInstanceID();
            if (entity is IMapTrackable map)
            {
                R3EventBus.Instance.Publish(new EntitySpawned<IMapTrackable>(id, map));
            }

            if (entity is IHpTrackable hp)
            {
                R3EventBus.Instance.Publish(new EntitySpawned<IHpTrackable>(id, hp));
            }
        }

        /* ── 파괴 ────────────────────────────────── */
        public static void PublishDestroyed(Object entity)
        {
            var id = entity.GetInstanceID();
            if (entity is IMapTrackable map)
            {
                R3EventBus.Instance.Publish(new EntityDestroyed<IMapTrackable>(id, map));
            }

            if (entity is IHpTrackable hp)
            {
                R3EventBus.Instance.Publish(new EntityDestroyed<IHpTrackable>(id, hp));
            }
        }
    }

    public interface IMapTrackable   // ─ 미니맵 아이콘용
    {
        Transform MapAnchor { get; }
        MiniMapIconType Icon { get; }
    }

    public interface IHpTrackable    // ─ HP 바 필요할 때만
    {
        Transform HpAnchor { get; }
    }
}
