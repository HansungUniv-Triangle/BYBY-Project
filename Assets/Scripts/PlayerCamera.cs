using System;
using TMPro;
using UnityEngine;
using Types;
using NetworkPlayer = Network.NetworkPlayer;
using Random = UnityEngine.Random;

public class PlayerCamera : MonoBehaviour
{
    private CameraMode _cameraMode;

    private Transform _target;
    private Transform _player;
    private NetworkPlayer _networkPlayer;
    private Transform _cameraPos;
    private Transform _cameraFocusPos;
    
    private Vector3 _originalCameraPos;
    private Vector3 _originalCameraFocusPos;

    private readonly float _moveSpeed = 3f; // 고정된 값 필요, 너무 빠르면 어지러움증 유발
    private float _rotationSpeed = 8f;

    private RaycastHit _hit;
    private Ray _ray;

    public float distance = 10.0f; // 카메라와 캐릭터 사이의 거리
    public float height = 5.0f; // 카메라의 높이
    public float smoothSpeed = 0.25f; // 카메라 이동 속도
    public float horizontalSpeed = 0.1f; // 카메라 수평 이동 속도
    private float _timer;
    public Transform _worldViewPos;
    
    #region UI Settings
    public float zAngle;
    private float zOffset = 0.8f;

    public void IncreaseZoffset(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (zOffset += 0.01f).ToString("F2"); }
    public void DecreaseZoffset(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (zOffset -= 0.01f).ToString("F2"); }

    public void ResetZangle() { zAngle = 0; }

    public void ToggleGyro()
    {
        GameManager.Instance.ToggleGyro();
        if (GameManager.Instance.IsGyroOn)
            StartGyro();
        else
            StopGyro();
    }

    public void StartGyro() { 
        Input.gyro.enabled = true;
    }
    public void StopGyro() { 
        Input.gyro.enabled = false;
        ResetZangle();
        transform.Rotate(new Vector3(0, 0, 0));
    }
    #endregion

    private void Awake()
    {
        if (GameManager.Instance.IsGyroOn)
            StartGyro();
    }

    private void Start()
    {
        _cameraMode = CameraMode.None;
        _timer = 11f;
    }
    
    public void AddPlayer(Transform player)
    {
        _player = player;
        _networkPlayer = player.GetComponent<NetworkPlayer>();
        _cameraPos = player.transform.Find("CameraPos");
        _cameraFocusPos = player.transform.Find("CameraFocusPos");
        _originalCameraPos = _cameraPos.localPosition;
        _originalCameraFocusPos = _cameraFocusPos.localPosition;

        if (_target != null)
        {
            _cameraMode = CameraMode.Player;
        }
    }
    
    public void AddEnemy(Transform enemy)
    {
        _target = enemy;

        if (_player != null)
        {
            _cameraMode = CameraMode.Player;
        }
    }

    public void ChangeCameraMode(CameraMode cameraMode)
    {
        _cameraMode = cameraMode;
        _timer = 11f;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.NetworkManager.SinglePlayMode)
        {
            GameView();
            CameraGyroRotate();
        }
        else
        {
            switch (_cameraMode)
            {
                case CameraMode.None:
                    WorldView();
                    break;
                case CameraMode.Game:
                    GameView();
                    CameraGyroRotate();
                    break;
                case CameraMode.Winner:
                    RotateCamera(GameManager.Instance.NetworkManager.IsPlayerWin ? _player : _target);
                    break;
                case CameraMode.Player:
                    RotateCamera(_player);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void WorldView()
    {
        RotateCamera(_worldViewPos);
    }

    private void RotateCamera(Transform target)
    {
        if (_timer > 10.0f)
        {
            var randomPosition = Random.insideUnitSphere * distance;
            randomPosition = target.position + randomPosition;
            randomPosition.y = Mathf.Max(randomPosition.y, target.position.y + height);
            transform.position = randomPosition;
            _timer = 0;
        }
        else
        {
            var newPosition = Quaternion.AngleAxis(horizontalSpeed, Vector3.up) * transform.position;
            transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed);
            transform.LookAt(target);
            _timer += Time.deltaTime;
        }
    }
    
    private void CameraGyroRotate()
    {
        if (!GameManager.Instance.IsGyroOn) { return; }

        var gyroRotationRate = Input.gyro.rotationRateUnbiased;

        zAngle = Mathf.Clamp(zAngle += gyroRotationRate.z * zOffset, -30f, 30f);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, zAngle);
    }

    private void GameView()
    {
        if(!_player || !_target) return;

        var position = _player.position;
        _ray.origin = position;
        _ray.direction = (_cameraPos.position - position).normalized;
        _rotationSpeed = _networkPlayer.GetCharStat(CharStat.Calm).Total * 0.3f + 8f;

        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.white);
        if (Physics.Raycast(_ray, out _hit, 10, (int)Layer.World))
        {
            _cameraPos.position = _hit.point;

            if (Vector3.Distance(position, _hit.point) < 0.5f)
                _cameraFocusPos.position = _hit.point;
        }
        else
        {
            if (_cameraPos.localPosition != _originalCameraPos)
                _cameraPos.localPosition = _originalCameraPos;

            if (_cameraFocusPos.localPosition != _originalCameraFocusPos)
                _cameraFocusPos.localPosition = _originalCameraFocusPos;
        }

        if (_networkPlayer.IsCameraFocused)
        {
            transform.position = Vector3.Lerp(transform.position, _cameraFocusPos.position, Time.deltaTime * _moveSpeed);

            var q = Quaternion.Lerp(transform.rotation, _cameraFocusPos.rotation, Time.deltaTime * _rotationSpeed);
            transform.rotation = Quaternion.Euler(q.eulerAngles.x, q.eulerAngles.y, 0);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _cameraPos.position, Time.deltaTime * _moveSpeed);

            var relativePosition = _target.position - transform.position;
            var targetRotation = Quaternion.LookRotation(relativePosition);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);

            _cameraFocusPos.rotation = targetRotation;
        }
    }

    public void ReverseCameraPos(bool isLeft)
    {
        if (isLeft)
        {
            if (_cameraPos.localPosition.x < 0)
            {
                _cameraPos.localPosition = new Vector3(_cameraPos.localPosition.x * -1, _cameraPos.localPosition.y, _cameraPos.localPosition.z);
                _cameraFocusPos.localPosition = new Vector3(_cameraFocusPos.localPosition.x * -1, _cameraFocusPos.localPosition.y, _cameraFocusPos.localPosition.z);

                _originalCameraPos = new Vector3(_originalCameraPos.x * -1, _originalCameraPos.y, _originalCameraPos.z);
                _originalCameraFocusPos = new Vector3(_originalCameraFocusPos.x * -1, _originalCameraFocusPos.y, _originalCameraFocusPos.z);
            }
        }
        else
        {
            if (_cameraPos.localPosition.x > 0)
            {
                _cameraPos.localPosition = new Vector3(_cameraPos.localPosition.x * -1, _cameraPos.localPosition.y, _cameraPos.localPosition.z);
                _cameraFocusPos.localPosition = new Vector3(_cameraFocusPos.localPosition.x * -1, _cameraFocusPos.localPosition.y, _cameraFocusPos.localPosition.z);

                _originalCameraPos = new Vector3(_originalCameraPos.x * -1, _originalCameraPos.y, _originalCameraPos.z);
                _originalCameraFocusPos = new Vector3(_originalCameraFocusPos.x * -1, _originalCameraFocusPos.y, _originalCameraFocusPos.z);
            }
        }
    }
    
    public void SetCameraFocusMode()
    {
        transform.position =_cameraFocusPos.position;
        transform.rotation = Quaternion.Euler(_cameraFocusPos.eulerAngles.x, _cameraFocusPos.eulerAngles.y, 0);
    }

    public void SetFocusedCameraRotation(Vector3 target)
    {
        var relativePosition = target - _cameraFocusPos.position;
        var targetRotation = Quaternion.LookRotation(relativePosition);

        _cameraFocusPos.rotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, 0);
    }

    public static Vector2 GetRotatedCoordinates(float x, float y)
    {
        var camAngle = Camera.main.transform.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector2(x * Mathf.Cos(camAngle) - y * Mathf.Sin(camAngle), x * Mathf.Sin(camAngle) + y * Mathf.Cos(camAngle));
    }
}
