using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.Pool;

public sealed class DamageTextSpawner
{
    // readonly ObjectPool<DamageText> mPool;
    // readonly CompositeDisposable mCD = new();
    //
    // public DamageTextSpawner(IEventBus bus)
    // {
    //     bus.Receive<EnemyHit>()
    //         .Subscribe(spawn)
    //         .AddTo(mCD);
    // }
    // void spawn(EnemyHit hit)
    // {
    //     var text = mPool.Rent();
    //     text.Show(hit.Position, hit.Damage, hit.IsCritical);
    // }
}
