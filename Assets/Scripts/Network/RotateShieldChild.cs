using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Network;
using UnityEngine;

public class RotateShieldChild : MonoBehaviour, ICollisionObjectEvent
{
    public RotateShield parent;

    private void Awake()
    {
        parent = gameObject.transform.parent.parent.GetComponent<RotateShield>();
    }

    public void CollisionObjectEvent(NetworkObject bullet)
    {
        parent.TouchChildShield(gameObject, bullet);
    }

    public bool CollisionObjectIsHitCheck()
    {
        return parent.GetComponent<NetworkObject>().HasStateAuthority;
    }
}
