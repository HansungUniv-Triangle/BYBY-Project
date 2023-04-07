using System;
using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

public class CameraRay : MonoBehaviour
{
    private Camera _camera;
    private int _maxDistance;
    private Transform _target;
    public float _offset = 100f;
    
    public int HitDamage = 1;
    public int ExplosionRadius = 3;

    private void Awake()
    {
        _maxDistance = 100;
    }

    private void Start()
    {
        _camera = Camera.main;
        _target = GameObject.Find("Player").transform;
    }

    private void ViewCharacter()
    {
        _camera.transform.position = _target.transform.position + new Vector3(0, _offset, -_offset);
        _camera.transform.LookAt(_target);
    }

    private void Update()
    {
        if (_camera is null) return;

        ViewCharacter();
            
        if (Input.GetMouseButtonDown(0))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, _maxDistance, (int)Layer.World))
            {
                var point = hit.point - hit.normal * 0.1f;
                WorldManager.Instance.GetWorld().ExplodeBlocks(point, ExplosionRadius, 3);
                //WorldManager.Instance.GetWorld().ExplodeBlocksNoAnimation(point, ExplosionRadius);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, _maxDistance, (int)Layer.World))
            {
                var point = hit.point - hit.normal * 0.1f;
                WorldManager.Instance.GetWorld().HitBlock(point, HitDamage);
            }
        }
    }
}
