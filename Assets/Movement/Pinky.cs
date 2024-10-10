using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinky : GeneralMovement
{
    private bool jumpOrderIn;
    private bool jumpOrderPrev;
    private bool attackOrderIn;
    private bool attackOrderPrev;
    public override bool getJump() {
        jumpOrderIn = Input.GetAxisRaw("JumpPink") > 0;
        bool jump = jumpOrderIn && jumpOrderPrev != jumpOrderIn;
        jumpOrderPrev = jumpOrderIn;
        return jump;
    }

    public override float getHorizontalMovement()
    {
        return Input.GetAxisRaw("HorizontalPink");
    }

    public override bool getAttack()
    {
        float attackOrder = Input.GetAxisRaw("AttackPink");

        return attackOrder > 0;
    }
}
