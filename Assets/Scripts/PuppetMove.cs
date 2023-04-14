using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetMove : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float gravity = -10.0f;
    public float jumpForce = 5.0f;
    public float yVelocity = 0.0f;

    public Transform headTransform;
    public CharacterController charactercontroller;

    private Vector3 moveDir;
    private float h, v;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        //Jump();
    }
    private void Move()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        moveDir = new Vector3(h, 0, v);
        moveDir = headTransform.TransformDirection(moveDir);
        moveDir *= moveSpeed;

        yVelocity += (gravity * Time.deltaTime);
        moveDir.y = yVelocity;
        charactercontroller.Move(moveDir * Time.deltaTime);

    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            yVelocity = 0;
            if (charactercontroller.isGrounded)
            {
                yVelocity = jumpForce;
            }
            
        }
    }

}
