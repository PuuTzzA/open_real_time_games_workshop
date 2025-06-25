using System;
using UnityEngine.InputSystem;

public interface IQTE 
{
    void Init(PlayerInput p1, PlayerInput p2, Action<QTEResult, QTEResult> onFinished);    
}