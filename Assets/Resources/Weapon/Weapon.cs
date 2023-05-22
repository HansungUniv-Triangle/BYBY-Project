using Fusion;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Weapon", fileName = "Weapon", order = 0)]
public class Weapon : ScriptableObject
{
    public Sprite sprite;
    public string weaponName;
    [TextArea]
    public string weaponExplain;
    public NetworkPrefabRef weaponPrefabRef;
    public NetworkPrefabRef bulletPrefabRef;
    public ParticleSystem bulletShoot;
    public ParticleSystem bulletHit;
    public ParticleSystem bulletHitToPlayer;
    public string shootSoundPath;
    public string hitSoundPath;
    public bool isMainWeapon;
}
