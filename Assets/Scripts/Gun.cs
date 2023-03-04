using System.Collections;
using UnityEngine;
using Type;

// public class Gun : MonoBehaviour
// {
//     private ObjectPoolManager _instance;
//
//     private void Start()
//     {
//         _instance = ObjectPoolManager.Instance;
//     }
//
//     public void Shoot(int amount)
//     { // todo: 샷건은 퍼지게, 라이플은 직선으로 나가게 수정
//         StartCoroutine(MultipleShoot(amount));
//     }
//
//     private IEnumerator MultipleShoot(int amount)
//     {
//         for (var i = 0; i < amount; i++)
//         {
//             //var bullet = _instance.DequeueObject(PoolObject.PlayerBullet);
//             bullet.transform.position = gameObject.transform.position;
//             bullet.transform.rotation = gameObject.transform.rotation;
//             bullet.SetActive(true);
//             yield return new WaitForSeconds(.05f);
//         }
//     }
// }