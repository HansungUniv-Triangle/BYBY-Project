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

    public void PlayEffect(ParticleSystem particle, Vector3 pos, Vector3 normal, Transform parent)
    {
        var effect = Instantiate(particle, pos, Quaternion.LookRotation(normal), parent);
        effect.Play();
    }

    protected override void Initiate()
    {
    }
}
