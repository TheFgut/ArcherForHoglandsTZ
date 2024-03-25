using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKillable
{
    public event dieCallback onDie;

    public bool isAlive { get; }

    public void Hurt(Arrow arrow);

    public Transform transform { get; }


    public delegate void dieCallback(); 

}
