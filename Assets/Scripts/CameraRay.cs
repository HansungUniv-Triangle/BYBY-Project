using System;
using System.Collections;
using System.Collections.Generic;
using Type;
using UnityEngine;

public class CameraRay : MonoBehaviour
{
    private Camera _camera;
    private int _maxDistance;
    
    public int HitDamage = 1;
    public int ExplosionRadius = 3;

    private void Awake()
    {
        _maxDistance = 100;
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_camera is null) return;

        if (Input.GetMouseButtonDown(0))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, _maxDistance, (int)Layer.World))
            {
                var point = hit.point - hit.normal * 0.1f;
                WorldManager.Instance.GetWorld().ExplodeBlocks(point, ExplosionRadius);
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
