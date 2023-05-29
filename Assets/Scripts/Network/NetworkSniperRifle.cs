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
                var projectile = SpawnProjectile(ShootPointTransform.position);
                RemainBullet--;

                if (_isSnipingMode)
                {
                    var special = GetWeaponStat(WeaponStat.Special).Total;
                    var ratio = 1 + special * 0.01f;
                    projectile.GetComponent<NetworkProjectileBase>().IndividualDamage += GetWeaponStat(WeaponStat.Damage).Total * ratio;
                }
            }
        }

        public void SnipingMode(bool value)
        {
            _isSnipingMode = value;
        }
    }
}