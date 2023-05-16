using DG.Tweening;
using Types;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _aim;
    [SerializeField]
    private RectTransform _aimplus;
    [SerializeField]
    private GameObject target;

    public GameObject testObj;

    public Vector3 pos;
    public Vector3 newPosToWorld;
    
    private Camera _camera;
    private RectTransform _canvas;

    private void Start()
    {
        _camera = Camera.main;
        _canvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (_camera == null) return;
        
        pos = _camera.WorldToViewportPoint(target.transform.position);
        var newPos = new Vector3(pos.x * Screen.width, pos.y * Screen.height, 0);
        _aimplus.transform.DOMoveY(pos.y * Screen.height, 2);
        
        if (pos is { x: > 0, y: > 0} and {x: < 1, y: < 1})
        {
            var duration = RectTransformUtility.RectangleContainsScreenPoint(_aimplus, newPos) ? 1 : 10;
            _aim.transform.DOMove(newPos, duration);
            _aim.SetActive(true);
            
            var ray = _camera.ScreenPointToRay(_aim.transform.position);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
            
            // if (Physics.Raycast(ray, out var hit, Mathf.Infinity, (int)Layer.Entity))
            // {
            //     testObj.transform.position = hit.point;
            // }
        }
        else
        {
            _aim.transform.DOKill();
            _aim.transform.position = newPos;
            _aim.SetActive(false);
        }
    }

    public void OpenCloseMenu(GameObject menu) 
    {
        menu.SetActive(!menu.activeSelf);
    }
}
