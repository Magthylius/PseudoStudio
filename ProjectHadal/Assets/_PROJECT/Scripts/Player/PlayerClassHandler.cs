using Hadal.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassHandler : MonoBehaviour
{
    public PlayerClassData PlayerClass;

    [ContextMenu("Apply Class")]
    void ApplyClass()
    {
        PlayerClass.SetUpUtility();
    }
}
