using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    public void PlayEffect(ParticleSystem particle, Vector3 pos, Vector3 normal)
    {
        var effect = Instantiate(particle, pos, Quaternion.LookRotation(normal));
        effect.Play();
    }

    public void PlayShootEffect(Vector3 pos, Vector3 normal)
    {
        PlayEffect(BulletShoot, pos, -normal);
    }

    public void PlayHitEffect(Vector3 pos, Vector3 normal, HitEffectType effectType)
    {
        ParticleSystem particleSystem;

        switch (effectType) {
            case HitEffectType.Player:
                particleSystem = BulletHitToPlayer;
                break;
            default:
                particleSystem = BulletHit;
                break;
        }

        PlayEffect(particleSystem, pos, normal);
    }

    protected override void Initiate()
    {
    }
}
