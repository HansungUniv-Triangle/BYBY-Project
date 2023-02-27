using System.Collections.Generic;
using Observer;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private BulletData playerBulletData;
    [SerializeField]
    private BulletData enemyBulletData;

    public BulletData PlayerBulletData => playerBulletData;
    public BulletData EnemyBulletData => enemyBulletData;
}
