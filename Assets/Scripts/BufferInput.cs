using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BufferInput
{
    public KangarooJackedData.InputActionType actionType;
    public Vector2 directionOfAction;
    public float timeOfInput;
    public BufferInput(KangarooJackedData.InputActionType actionTypeSent, Vector2 directionSent, float timeOfInputSent)
    {
        actionType = actionTypeSent;
        directionOfAction = directionSent;
        timeOfInput = timeOfInputSent;
    }
}
