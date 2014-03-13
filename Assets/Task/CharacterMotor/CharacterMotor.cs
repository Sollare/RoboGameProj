using UnityEngine;
using System.Collections;

public abstract class CharacterMotor : MonoBehaviour
{
    private Vector3 _movementDirection;
    // Вектор напаравления движения персонажа
    [HideInInspector]
    public Vector3 movementDirection
    {
        set
        { _movementDirection = value.normalized; }

        get
        {
            return _movementDirection;
        }
    }

    // Направление взгляда персонажа
    [HideInInspector]
    public Vector3 facingDirection;
}
