using DG.Tweening;
using Fusion;
using UnityEngine;

namespace Network
{
    public class RotateShield : NetworkProjectileBase
    {
        [Networked(OnChanged = nameof(ChangeChildState)), Capacity(4)]
        public NetworkArray<NetworkBool> ChildShieldActive { get; } 
            = MakeInitializer(new NetworkBool[] { true, true, true, true });
        
        public GameObject[] childShieldObject = new GameObject[4];
        public int childCount = 4;

        private static void ChangeChildState(Changed<RotateShield> changed)
        {
            changed.Behaviour.ChangeChildState();
        }
        
        private void ChangeChildState()
        {
            for (var i = 0; i < childShieldObject.Length; i++)
            {
                childShieldObject[i].SetActive(ChildShieldActive[i]);
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            gameObject.transform.eulerAngles = Vector3.zero;
            if (_projectileHolder)
            {
                _projectileHolder.ChangeIsDone(false);
            }
        }

        protected override bool IsExpirationProjectile()
        {
            if (childCount == 0 || Distance > (MaxRange * 10))
            {
                if (_projectileHolder)
                {
                    _projectileHolder.ChangeIsDone(true);
                }
                return true;
            }

            return false;
        }

        protected override void UpdateProjectile()
        {
            gameObject.transform.Rotate(Vector3.up, TotalVelocity / 10);
            gameObject.transform.position = GameManager.Instance.NetworkManager.PlayerCharacter.transform.position;
        }

        public void TouchChildShield(GameObject shield, NetworkObject bullet)
        {
            var childNum = shield.transform.GetSiblingIndex();
            if (HasStateAuthority && !bullet.HasStateAuthority) // 쉴드는 내꺼, 총알은 상대꺼
            {
                Debug.Log("적 총알과 충돌");
                ChildShieldActive.Set(childNum, false);
                childCount--;
            }
            else if (!HasStateAuthority && bullet.HasStateAuthority) // 쉴드는 상대꺼, 총알은 내꺼
            {
                Debug.Log("적 쉴드와 충돌");
                bullet.GetComponent<NetworkProjectileBase>().DestroyProjectile();
            }
        }
    }
}