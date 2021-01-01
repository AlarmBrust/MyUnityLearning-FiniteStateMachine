using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Animator anim;
    State currentState;

    void Start()
    {
        anim = this.GetComponent<Animator>();
        currentState = new Idle(this.gameObject,anim);
    }

    void Update()
    {
        currentState = currentState.Process();
    }

}
