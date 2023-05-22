using System;
using DG.Tweening;
using Types;
using UnityEngine;

namespace Network
{
    public class NetworkSniperRifle : NetworkProjectileHolder
    {
        private bool _isSnipingMode = true;
        
        protected override void Attack()
        {
            if (CanAttack())
            {
                var projectile = SpawnProjectile(WeaponTransform);
                RemainBullet--;

                if (_isSnipingMode)
                {
                    projectile.GetComponent<NetworkProjectileBase>().IndividualDamage += GetWeaponStat(WeaponStat.Damage).Total;
                }
            }
        }

        public void SnipingMode(bool value)
        {
            _isSnipingMode = value;
        }
    }
}