using UnityEngine;
using UnityEngine.Serialization;

public class Bullet : MonoBehaviour
{
    public BulletData _bulletData; // 중복되는 값은 SO를 통해 일괄적으로 관리
    
    private float _nowRange = 0;
    private float _addVelocity = 0;
    private float _addSize = 0;
    private float _addDamage = 0;

    private float CalculateVelocity => _bulletData.velocity + _addVelocity;
    private float CalculateSize => _bulletData.size + _addSize;
    private float CalculateDamage => _bulletData.damage + _addDamage;
    
    public bool isPenetrate = false; // 관통
    public bool isGuided = false; // 유도
    
    private ObjectPoolManager _objectPoolManager;

    private void Awake()
    {
        _objectPoolManager = ObjectPoolManager.Instance;
    }

    private void OnEnable()
    {
        if (_bulletData is null) return;
        
        var newSize = _bulletData.size / 5;
        transform.localScale = new Vector3(newSize, newSize, newSize);
        _nowRange = 0;
    }

    private void Update()
    {
        if (_nowRange > _bulletData.maxRange)
        {
            CleanupBullet();
        }
    }

    private void FixedUpdate()
    {
        _nowRange += Time.deltaTime * CalculateVelocity;
        transform.Translate(Vector3.forward * CalculateVelocity);
    }

    private void CleanupBullet()
    {
        _objectPoolManager.EnqueueObject(_bulletData.poolObject, gameObject);
    }

    public void SetBaseBullet(BulletData bulletData)
    {
        _bulletData = bulletData;
    }
}
