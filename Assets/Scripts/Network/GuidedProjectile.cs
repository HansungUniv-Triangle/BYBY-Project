using System;
using UnityEngine;

namespace Network
{
    public class GuidedProjectile : NetworkProjectileBase
    {
        public float speed = 10.0f;
        public float maxAngle = 45.0f;
        
        protected override bool IsExpirationProjectile()
        {
            if (IsHit) return true;
            return Distance > MaxRange;
        }

        protected override void UpdateProjectile()
        {
            Vector3 direction = _projectileHolder.GetTarget() - transform.position;
            float angle = GetAngle(transform.forward, direction); 
            float angleThreshold = 45f;
            float rotationSpeed = 10f;

            if (angle > -angleThreshold && angle < angleThreshold)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Runner.DeltaTime);
            }
            
            base.UpdateProjectile();
            DamageSave /= 2;
        }
        
        public static float GetAngle(Vector3 vStart, Vector3 vEnd)
        {
            Vector3 v = vEnd - vStart;
            return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
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
                GameManager.Instance.NetworkManager.AddCharacterHitData(Object, (int)DamageSave, _projectileHolder.WeaponData.isMainWeapon);
                IsHit = true;
            }
        }
    }
}