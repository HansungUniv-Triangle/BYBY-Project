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

    protected override void Initiate()
    {
    }
}
