using System;
using DG.Tweening;
using Types;
using UnityEngine;

namespace Network
{
    public class NetworkSniperRifle : NetworkProjectileHolder
    {
        [SerializeField]
        private bool isSnipingMode = true;
        
        protected override void Attack()
        {
            if (CanAttack())
            {
                var projectile = SpawnProjectile(ShootPointTransform);
                RemainBullet--;
                
                GameManager.Instance.shootCount += 1;
                
                if (isSnipingMode)
                {
                    projectile.GetComponent<NetworkProjectileBase>().IndividualDamage += GetWeaponStat(WeaponStat.Damage).Total;
                }
            }
        }
    }
}