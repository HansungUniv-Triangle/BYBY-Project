using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameStatus;
using TMPro;
using Types;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PuppetMove : MonoBehaviour
{
    private GameObject target;

    #region 움직임 관련 변수
    public float Speed = 11.0f;
    public float gravity = 15.0f;
    public float jumpForce = 7.0f;
    public float dodgeForce = 4.0f;
    public float dodgeFrequency = 0.1f;

    private float h, v;
    private float inputH, inputV;
    private Vector3 moveDir;
    private bool isJump = false;
    private bool isDodge = false;
    private CharacterController _characterController;
    #endregion
    
    public bool isCameraFocused = false;
    
    private BaseStat<CharStat> _baseCharStat;
    private WaitForSeconds _oneSec;

    #region UI Settings
    public void IncreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (++Speed).ToString(); }
    public void DecreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (--Speed).ToString(); }
    public void IncreaseJump(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (++jumpForce).ToString(); }
    public void DecreaseJump(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (--jumpForce).ToString(); }
    public void IncreaseDodge(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (++dodgeForce).ToString(); }
    public void DecreaseDodge(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (--dodgeForce).ToString(); }

    public void SetDodgeFrequency(Slider slider)
    {
        dodgeFrequency = slider.value;
        slider.GetComponentInChildren<TextMeshProUGUI>().text = dodgeFrequency.ToString("F2");
    }

    private bool isMove;
    public void ToggleMoving()
    {
        isMove = !isMove;
    }

    private Vector3 _initPos;
    public void InitPosition()
    {
        _characterController.enabled = false;
        transform.position = _initPos;
        _characterController.enabled = true;
    }
    #endregion

    private void Awake()
    {
        _initPos = transform.position;
        _characterController = GetComponent<CharacterController>();
        _oneSec = new WaitForSeconds(1.0f);
    }

    void Start()
    {
        h = v = 0;
        inputH = inputV = 0;
        moveDir = Vector3.zero;
        
        if (!_characterController.isGrounded)
            isJump = true;

        StartCoroutine(SettingMoveValues());
    }

    private IEnumerator SettingMoveValues()
    {
        while (true)
        {
            inputH = Random.Range(-1f, 1f);
            inputV = Random.Range(-1f, 1f);

            if (Random.Range(0, 1f) > 1f - dodgeFrequency && !isDodge)
            {
                isDodge = true;
                DOTween.Sequence()
                    .AppendInterval(0.1f)
                    .OnComplete(() =>
                    {
                        isDodge = false;
                    });
            }
            
            yield return _oneSec;
        }
    }
    
    void Update()
    {
        if (target is null)
        {
            if (GameManager.Instance.NetworkManager is not null && GameManager.Instance.NetworkManager.PlayerCharacter is not null)
            {
                target = GameManager.Instance.NetworkManager.PlayerCharacter.gameObject;
            }
        }
        else
        {
            Move();
        }
    }
    
    private void Move()
    {
        h = inputH;
        v = inputV;
        //v = (global::Move.targetDistance < 2f && v > 0) ? 0 : v;

        if (!isMove)
            h = v = 0;

        var speed = Speed;

        // move
        if (_characterController.isGrounded)
        {
            moveDir = new Vector3(h, 0, v);
            moveDir = transform.TransformDirection(moveDir);
            moveDir *= speed;
        }
        else
        {
            var tmp = new Vector3(h, 0, v);
            tmp = transform.TransformDirection(tmp);
            tmp *= (speed * 0.7f);

            moveDir.x = tmp.x;
            moveDir.z = tmp.z;
        }

        if (isJump)
        {
            moveDir.y = jumpForce;
            isJump = false;
        }

        if (isDodge)
        {
            moveDir.x *= dodgeForce;
            moveDir.z *= dodgeForce;
        }

        moveDir.y -= gravity * Time.deltaTime;

        _characterController.Move(moveDir * Time.deltaTime);
        
        if (isCameraFocused == false && target is not null)
        {
            var relativePosition = target.transform.position - transform.position;
            relativePosition.y = 0; // y축은 바라보지 않도록 함
            var targetRotation = Quaternion.LookRotation(relativePosition);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == LayerMask.NameToLayer("World") && hit.normal.y == 0)
        {
            if (_characterController.isGrounded)
            {
                isJump = true;
            }
        }
    }

    // 플레이어가 나가도 참조하는거 방지
    public void TurnOff()
    {
        gameObject.SetActive(false);
    }
}
