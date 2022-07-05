using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    private static AbilityInputAction _inputActions;
    public static AbilityInputAction InputActions 
    { get
        {
            if (_inputActions == null)
            {
                _inputActions = new AbilityInputAction();
                _inputActions.Enable();
            }
            return _inputActions;
        }
    }
}
