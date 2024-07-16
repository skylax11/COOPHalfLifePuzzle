using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IGunState 
{
    public void Enter();
    public void HandleInput();
    public void Exit();
}
