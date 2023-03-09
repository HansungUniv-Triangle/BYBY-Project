using System;
using System.Collections.Generic;
using UnityEngine;
using Type;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    // private readonly Dictionary<PoolObject, Queue<GameObject>> _objectDictionary = new();
    // private GameManager _gameManager;
    //
    // // 투사체 프리팹
    // public GameObject projectilePrefab;
    //
    // protected override void Initiate()
    // {
    //     CreateKey(PoolObject.PlayerBullet);
    //     _playerBullet = Instantiate(bulletPrefab, transform);
    //     
    //     CreateKey(PoolObject.EnemyBullet);
    //     _enemyBullet = Instantiate(bulletPrefab, transform);
    // }
    //
    // private void Start()
    // {
    //     _gameManager = GameManager.Instance;
    //     _playerBullet.GetComponent<Bullet>().SetBaseBullet(_gameManager.PlayerBulletData);
    //     _enemyBullet.GetComponent<Bullet>().SetBaseBullet(_gameManager.EnemyBulletData);
    //
    //     for (var i = 0; i < 100; i++)
    //     {
    //         var pBullet = Instantiate(_playerBullet);
    //         EnqueueObject(PoolObject.PlayerBullet, pBullet);
    //         
    //         var eBullet = Instantiate(_enemyBullet);
    //         EnqueueObject(PoolObject.EnemyBullet, eBullet);
    //     } 
    // }
    //
    // private void CreateKey(PoolObject key)
    // {
    //     if (!_objectDictionary.ContainsKey(key))
    //     {
    //         var newQueue = new Queue<GameObject>(); 
    //         _objectDictionary.Add(key, newQueue); 
    //     }
    // }
    //
    // public void EnqueueObject(PoolObject key, GameObject obj)
    // {
    //     if (!_objectDictionary.ContainsKey(key))
    //     {
    //         CreateKey(key);
    //     }
    //     
    //     obj.SetActive(false);
    //     obj.transform.SetParent(gameObject.transform);
    //     _objectDictionary[key].Enqueue(obj);
    // }
    //
    // public GameObject DequeueObject(PoolObject key)
    // {
    //     if (!_objectDictionary.ContainsKey(key))
    //     {
    //         CreateKey(key);
    //     } 
    //     
    //     return _objectDictionary[key].Count > 0 ? 
    //         _objectDictionary[key].Dequeue() : 
    //         Instantiate(KeyToPoolObj(key), transform);
    // }
    //
    // private GameObject KeyToPoolObj(PoolObject key)
    // {
    //     return key switch
    //     {
    //         PoolObject.PlayerBullet => _playerBullet,
    //         PoolObject.EnemyBullet => _enemyBullet,
    //         _ => throw new ArgumentOutOfRangeException()
    //     };
    // }
    protected override void Initiate()
    {
        throw new NotImplementedException();
    }
}
