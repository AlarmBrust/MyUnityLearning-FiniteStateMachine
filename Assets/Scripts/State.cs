using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
Base class State:
1. Four different states (idle, walk, pee, and sit) which are switched randomly. 
2. Two methodes GoalReached() for checking if doggy reaches his destination depending on distance
GoToDestination() moves doggy to his destination and changes his facing direction depending on destination
*/
public class State
{
    public enum STATE
    {
        IDLE, WALK, PEE,SIT
    };
    public enum EVENT
    {
        ENTER, UPDATE, EXIT
    }
    public STATE name;
    protected EVENT stage;
    protected GameObject doggy;
    protected Animator anim;
    protected State nextState;

    protected Vector3 peePos = new Vector3(0f,0f,0f);
    protected SpriteRenderer doggySR;
    protected float walkSpeed = 0.5f;

    public State(GameObject doggy_, Animator anim_)
    {
        doggy = doggy_;
        anim = anim_;
    }

    public virtual void Enter()
    {
        stage = EVENT.UPDATE;
    }

    public virtual void Update()
    {
        stage = EVENT.UPDATE;
    }

    public virtual void Exit()
    {
        stage = EVENT.EXIT;
    }

    public State Process()
    {
        if (stage == EVENT.ENTER){Enter();}
        if (stage == EVENT.UPDATE){Update();}
        if (stage == EVENT.EXIT)
        {
            Exit();
            return nextState;
        }
        return this;
    }

    public bool GoalReached(Vector3 currentPos, Vector3 des)
    {
        if (Vector3.Distance(currentPos,des)<0.1f)
        {
            return true;
        }
        return false;
    }

    public void GoToDestination(Vector3 des, float speed)
    {
        if (des.x > doggy.transform.position.x)
        {
          doggySR.flipX = false;;
        }
        else {doggySR.flipX = true;}
        doggy.transform.position = Vector3.MoveTowards(doggy.transform.position,des,speed*Time.deltaTime);

    }

}

/*
Child class Idle:
It's a Entry state, in which doggy only sticks his tongue without moving anywhere
Next State: Walk.
*/
public class Idle : State
{

    public Idle(GameObject doggy_, Animator anim_): base(doggy_,anim_)
    {
        name = STATE.IDLE;
    }

    public override void Enter()
    {
        anim.SetTrigger("isIdling");
        base.Enter();
    }

    public override void Update()
    {
        if (Random.Range(0f,100.0f)<1.0f)
        {
            nextState = new Walk(doggy,anim);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isIdling");
        base.Exit();
    }
}

/*
Child class Walk:
It's a following state of Idle, in which doggy chooses a position in the nearby and moves.
If he has reached his destination or walk around for too long (> 4sec) then changes a destination
Next States: Sit, Pee.

*/

public class Walk : State
{
    Vector3 des = new Vector3(0.5f,-1.5f,0f);
    float walkDistance;
    float walkAngle;
    float walkCount = 0f;

    public Walk(GameObject doggy_, Animator anim_):base(doggy_,anim_)
    {
        name = STATE.WALK;
    }

    public override void Enter()
    {
        doggySR = doggy.GetComponent<SpriteRenderer>();
        anim.SetTrigger("isWalking");
        base.Enter();
    }

    public override void Update()
    {
        walkCount += Time.deltaTime;
        GoToDestination(des,walkSpeed);
        if (walkCount > 4.0f || GoalReached(doggy.transform.position,des))
        {
            if (Random.Range(0f,100.0f)<20.0f)
            {
                nextState = new Sit(doggy,anim);
                stage = EVENT.EXIT;
            }
            walkCount = 0f;
            des = RandomDestination();
        }


        if (Random.Range(0f,1000.0f)<1.0f)
        {
            nextState = new Pee(doggy,anim);
            stage = EVENT.EXIT;
        }

    }

    public override void Exit()
    {
        anim.ResetTrigger("isWalking");
        base.Exit();
    }

    public Vector3 RandomDestination()
    {
        
        walkDistance = Random.Range(1.0f,2.0f);
        walkAngle = Random.Range(-0.5f*Mathf.PI,0f);
        float xDes = peePos.x + walkDistance*Mathf.Cos(walkAngle);
        float yDes = peePos.y + walkDistance*Mathf.Sin(walkAngle);
        Vector3 destination = new Vector3(xDes,yDes,0);

        return destination;
    }

}

/*
Child class Pee:
It's a following state of Walk, in which doggy moves to a set point and pees.
After that he starts again from idling.
Next State: Idle.
*/

public class Pee : State
{
    float peeCount = 0f;
    public Pee(GameObject doggy_, Animator anim_):base(doggy_,anim_)
    {
        name = STATE.PEE;
    }

    public override void Enter()
    {
        doggySR = doggy.GetComponent<SpriteRenderer>();
        base.Enter();
    }

    public override void Update()
    {
        if (!GoalReached(doggy.transform.position,peePos))
        {
          GoToDestination(peePos,walkSpeed);
          anim.SetTrigger("isWalking");
        }
        else
        {
          anim.ResetTrigger("isWalking");
          anim.SetTrigger("isPeeing");
          peeCount += Time.deltaTime;
          if (peeCount > 2.0f)
          {
            nextState = new Idle(doggy,anim);
            stage = EVENT.EXIT;
          }

        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isPeeing");
        base.Exit();
    }
}

/*
Child class Sit:
It's a following state of Walk, in which doggy sits at a random position while walking
After sitting for a while, he starts again from Idling
Next State Idle.
*/

public class Sit : State
{
    float sitTime = 3.0f;
    float sitCount = 0f;
    public Sit(GameObject doggy_, Animator anim_):base(doggy_,anim_)
    {
        name = STATE.SIT;
    }

    public override void Enter()
    {
        anim.SetTrigger("isSitting");
        base.Enter();
    }

    public override void Update()
    {
        sitCount += Time.deltaTime;
        if (sitCount >= sitTime)
        {
            nextState = new Idle(doggy,anim);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isSitting");
        base.Exit();
    }
}
