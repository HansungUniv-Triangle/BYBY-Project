/* BasicBullet
 * 기본 총알
 */

using System;
using UnityEngine;
using Util;

namespace Weapon
{
    public class BasicBullet : ProjectileBase<BasicBullet>
    {
        protected override void MoveProjectile()
        {
            gameObject.transform.Translate(Direction * TotalVelocity);
        }
        
        protected override bool CheckDestroy()
        {
            return Distance > MaxRange;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("World"))
            {
                var hit = collision.contacts[0];
                var point = hit.point - hit.normal * 0.1f;
                WorldManager.Instance.GetWorld().HitBlock(point, 1);
                DestroyProjectile();
            }
            
            // if (collision.gameObject.CompareTag("Block"))
            // {
            //     if (collision.gameObject.TryGetComponent<DamagedBlock>(out var damagedBlockScript))
            //     {
            //         damagedBlockScript.DecreaseHP(TotalDamage);
            //     }
            //     else
            //     {
            //         throw new Exception(Message.CantFindBlockTagInDamagedBlock);
            //     }
            // }
        }
    }
}