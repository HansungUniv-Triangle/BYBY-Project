using System;
using System.Collections.Generic;
using UnityEngine;

/* ProjectileHolder
 * 투사체 (ProjectileBase)를 품는 클래스
 * GetProjectile으로 투사체를 생성하고, List에 저장합니다. (쓸 곳은 모름)
 */

// Todo: WeaponStat의 Bullet이랑 List 크기랑 연동시켜야 함.

namespace Weapon
{
    public class ProjectileHolder<T> where T : ProjectileBase<T>
    {
        private ProjectileData _projectileData;
        private readonly List<ProjectileBase<T>> _projectileList;
        private readonly T _projectileScript;

        public ProjectileHolder(string prefabName)
        {
            _projectileList = new List<ProjectileBase<T>>();
            var projectilePrefab = Resources.Load<GameObject>(prefabName);

            if (projectilePrefab.TryGetComponent<T>(out var script))
            {
                _projectileScript = script;
            }
            else
            {
                throw new Exception("잘못된 prefab 연결");
            }
        }

        public ProjectileBase<T> GetProjectile(Transform position)
        {
            var obj = UnityEngine.Object.Instantiate(_projectileScript, position);
            obj.transform.SetParent(null);
            obj.Initialized(this);
            _projectileList.Add(obj);
            return obj;
        }

        public void RemoveProjectile(ProjectileBase<T> projectile)
        {
            _projectileList.Remove(projectile);
            //UnityEngine.Object.Destroy(projectile.GameObject());
        }
    }
}
