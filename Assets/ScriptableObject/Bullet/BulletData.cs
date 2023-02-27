using Type;
using UnityEngine;

[ CreateAssetMenu(fileName = "BulletData", menuName = "SO/BulletData" )]
public class BulletData : ScriptableObject
{
    public PoolObject poolObject = PoolObject.PlayerBullet;
    public float maxRange = 1; // 최대 거리
    public float velocity = 0.1f; // 탄속
    public float size = 1; // 탄환 크기
    public float damage = 1; // 데미지
    public float shield = 1; // 쉴드 데미지
    public Color32 color = new Color32(0, 0, 0, 255);
}