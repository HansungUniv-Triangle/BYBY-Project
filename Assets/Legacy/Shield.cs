// using UnityEngine;
//
// namespace Weapon
// {
//     public class Shield : ProjectileBase<Shield>
//     {
//         private bool _isTouched = false;
//         
//         protected override bool CheckDestroy()
//         {
//             return Distance > MaxRange || _isTouched;
//         }
//
//         private void OnCollisionEnter(Collision collision)
//         {
//             if (collision.gameObject.layer == LayerMask.NameToLayer("World"))
//             {
//                 var hit = collision.contacts[0];
//                 var point = hit.point - hit.normal * 0.1f;
//                 WorldManager.Instance.GetWorld().HitBlock(point, 1);
//             }
//         }
//
//         protected override void MoveProjectile()
//         {
//             transform.Rotate(Vector3.up, TotalVelocity);
//             AddBulletSize(0.01f);
//         }
//     }
// }