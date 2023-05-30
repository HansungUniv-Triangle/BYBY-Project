using System;
using System.Numerics;
using DG.Tweening;
using Fusion;
using Types;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Network
{
    public class NetworkSniperRifle : NetworkProjectileHolder
    {
        private bool _isSnipingMode = false;
        public NetworkObject hitScan;
        
        protected override void Attack()
        {
            if (CanAttack())
            {
                SpawnProjectile(ShootPointTransform.position);
                UpdateBullet(-1);
            }
        }

        public void SnipingShot()
        {
            if (CanAttack())
            {
                var line = Runner.Spawn(hitScan, position:null, inputAuthority: Runner.LocalPlayer);
                line.GetComponent<HitScan>().SetPosition(ShootPointTransform.position, Target);

                var result = Physics.OverlapCapsule(ShootPointTransform.position, Target, 0.3f, (int)Layer.Enemy);

                if (result.Length > 0)
                {
                    var special = GetWeaponStat(WeaponStat.Special).Total;
                    var ratio = 2 + special * 0.01f;
                    var damage = GetWeaponStat(WeaponStat.Attack).Total * ratio;
                        
                    GameManager.Instance.NetworkManager.AddCharacterHitData(Object, (int)damage, true);
                }

                UpdateBullet(-1);
                SetDelayTimer();
            }
        }
    }
}