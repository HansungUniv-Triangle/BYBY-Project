using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRay : MonoBehaviour
{
    public int HitDamage = 1;
    public int ExplosionRadius = 3;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100, 1 << LayerMask.NameToLayer("World")))
            {
                var point = hit.point - hit.normal * 0.1f;
                WorldManager.Instance.GetWorld().ExplodeBlocks(point, ExplosionRadius);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100, 1 << LayerMask.NameToLayer("World")))
            {
                var point = hit.point - hit.normal * 0.1f;
                WorldManager.Instance.GetWorld().HitBlock(point, HitDamage);
            }
        }
    }
}
