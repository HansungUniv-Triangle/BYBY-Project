using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;
    
    public Animator animator;

    public void RotateModel(Vector3 dir, float speed)
    {
        LerpLookRotation(gameObject.transform, dir, speed);
    }
    
    private void LerpLookRotation(Transform origin, Vector3 dir, float speed)
    {
        var targetRotation = Quaternion.LookRotation(dir);
        origin.rotation = Quaternion.Lerp(origin.rotation, targetRotation, Time.deltaTime * speed);
    }

    public void UpdateAnimation(int index, float speed)
    {
        switch (index)
        {
            case 8:
                animator.Rebind();
                animator.Update(0f);
                SoundManager.Instance.Play3DSound("dash", Types.Sound.Effect, Position);
                break;
            case 9:
                SoundManager.Instance.Play3DSound("jump", Types.Sound.Effect, Position);
                break;
            case 18:
                animator.SetFloat("walkSpeed", speed);
                break;
            default:
                break;
        }

        animator.SetInteger("animation", index);
    }
    
    public void UpdateRotation(Quaternion quaternion)
    {
        gameObject.transform.rotation = quaternion;
    }
}
