using System;
using DG.Tweening;
using Types;
using UnityEngine;

namespace Network
{
    public class SlowProjectile : NetworkProjectileBase, ICollisionCharacterEvent
    {
        protected override bool IsExpirationProjectile()
        {
            if (IsHit) return true;
            return Distance > MaxRange;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(IsHit || !HasStateAuthority) return;
            var objectLayer = collision.collider.gameObject.layer;

            if (objectLayer.Equals(LayerMask.NameToLayer("World")))
            {
                var hit = collision.contacts[0];
                var point = hit.point - hit.normal * 0.01f;

                point.x = (float)Math.Round(point.x, 3);
                point.y = (float)Math.Round(point.y, 3);
                point.z = (float)Math.Round(point.z, 3);

                WorldManager.Instance.GetWorld().HitBlock(point, (int)DamageSave);
                GameManager.Instance.NetworkManager.AddBlockHitData(point, (int)DamageSave);
                IsHit = true;
            }
            else if (objectLayer.Equals(LayerMask.NameToLayer("Enemy")))
            {
                if (Object.HasStateAuthority)
                {
                    GameManager.Instance.NetworkManager.AddCharacterHitData(Object, (int)DamageSave, _projectileHolder.WeaponData.isMainWeapon);
                    IsHit = true;
                }
            }
        }

        public void CollisionCharacterEvent(NetworkPlayer character)
        {
            var speed = character.GetCharStat(CharStat.Speed);
            var speedValue = speed.Total / 3;
            
            DOTween.Sequence()
                .AppendCallback(() => speed.AddAddition(-speedValue))
                .AppendInterval(3f)
                .AppendCallback(() => speed.AddAddition(speedValue));
        }
    }
}