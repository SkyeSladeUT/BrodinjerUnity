﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Character/Player/Translate/ThirdPerson")]
public class ThirdPersonTranslate : CharacterTranslate
{
    
    private Vector3 _moveVec, _jumpVec;
    private bool invoking, jumping, falling;
    private float currentTime;
    public string JumpTrigger;
    public float JumpDelay;
    private readonly WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();

    private bool dodging = false;
    public float DodgeTime, DodgeAmount;
    private Vector3 DodgeDirection;
    
    
    public override void Init(MonoBehaviour caller, CharacterController _cc, Transform camera, Targeting target, Animator animator)
    {
        invoking = false;
        falling = false;
        dodging = false;
        jumping = false;
        _moveVec = Vector3.zero;
        currentForwardSpeed = ForwardSpeed;
        currentSideSpeed = SideSpeed;
        base.Init(caller, _cc, camera, target, animator);
    }


    public override IEnumerator Move()
    {
        animation.StartAnimation();
        while (canMove)
        {
            if (!invoking && canMove)
            {
                invoking = true;
                caller.StartCoroutine(Invoke());
            }

            if (!canMove)
            {
                vSpeed -= Gravity * Time.deltaTime;
                _moveVec = Vector3.zero;
                _moveVec.y = vSpeed;
                _cc.Move(_moveVec * Time.deltaTime);
            }

            yield return fixedUpdate;
        }

    }

    public override IEnumerator Run()
    {
        while (true)
        {
            if (canRun)
            {
                if (Input.GetButton("Sprint"))
                {
                    currentForwardSpeed = RunForwardSpeed;
                    currentSideSpeed = RunSideSpeed;
                }
                else
                {
                    currentForwardSpeed = ForwardSpeed;
                    currentSideSpeed = SideSpeed;
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public override float getMoveAngle()
    {
        return GetDirection(_cc.transform, _moveVec);
    }
    
    public virtual float GetDirection(Transform player, Vector3 moveDirection)
    {
        Vector3 collisionposition = moveDirection;
        collisionposition.y = 0;
        Vector3 transformposition = player.position;
        transformposition.y = 0;
        Vector3 target = collisionposition - transformposition;
        float angle = Vector3.Angle(target, player.forward);
        Vector3 crossProduct = Vector3.Cross(target, player.forward);
        if (crossProduct.y < 0)
        {
            angle = -angle;
        }

        angle /= 360;
        angle += .5f;
        return angle;
    }

    public override float getSpeed()
    {
        return ConvertRange(0, RunForwardSpeed, 0, 1, _cc.velocity.magnitude);
    }

    public virtual IEnumerator Invoke()
    {
        _moveVec = Camera.forward * currentForwardSpeed * Input.GetAxis("Vertical") +
                   Camera.right * currentSideSpeed * Input.GetAxis("Horizontal");
        _moveVec.y = 0;
        if (_cc.isGrounded) {
            vSpeed = -10;
            if (!jumping && !falling && (Input.GetButtonDown ("Jump"))) {
                if (!dodging && targetScript.targeting && (Input.GetButton(HorizontalAxis) || Input.GetButton(VerticalAxis)))
                {
                    dodging = true;
                    Debug.Log("Dodge");
                    DodgeDirection = _cc.transform.forward*Input.GetAxisRaw(VerticalAxis) + _cc.transform.right*Input.GetAxisRaw(HorizontalAxis);
                    currentTime = 0;
                    while (currentTime < DodgeTime)
                    {
                        _moveVec = DodgeDirection * DodgeAmount;
                        _moveVec.y = 0;
                        _cc.Move(_moveVec * Time.deltaTime);
                        currentTime += Time.deltaTime;
                        yield return fixedUpdate;
                    }
                    dodging = false;
                }
                else
                {
                    jumping = true;
                    anim.SetTrigger(JumpTrigger);
                    currentTime = 0;
                    while (currentTime < JumpDelay)
                    {
                        vSpeed -= Gravity * Time.deltaTime;
                        _moveVec.y = vSpeed;
                        _cc.Move(_moveVec * Time.deltaTime);
                        currentTime += Time.deltaTime;
                        yield return fixedUpdate;
                    }
                    vSpeed = JumpSpeed;
                }
            }
            else
            {
                if ((jumping || falling))
                {
                    falling = false;
                    jumping = false;
                    anim.SetTrigger("Land");
                }
            }
        }
        else
        {
            if (!jumping && !falling)
            {
                falling = true;
                Debug.Log("Fall");
                anim.SetTrigger("Fall");
            }
        }
        vSpeed -= Gravity * Time.deltaTime;
        _moveVec.y = vSpeed;
        _cc.Move(_moveVec * Time.deltaTime);
        invoking = false;
        yield return null;
    }

}
