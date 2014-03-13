using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class PlayerMoveParameters
{
    public float movementAcceleration = 20f;

    public float normalSpeed = 5f;
    public float runSpeed = 5f;

    public float jumpForce = 10f;

    public float rotationSpeed = 4f;
}

[Serializable]
public class RollParameters
{
    // Продолжительность переката
    public float Duration = 1f;
    public float Force = 25f;

    // Кривая переката
    // Горизонтальная ось 0..1 - время от начала переката до конца
    // Вертикальная ось 0..1 - коэффициент прилагаемой силы в данный момент переката
    public AnimationCurve TimeForceCurve;
}


public class PlayerMotor : CharacterMotor
{
    private Rigidbody _rigidbody;

    protected Rigidbody cachedRigidbody
    {
        get
        {
            if (_rigidbody) return rigidbody;
            else return (_rigidbody = rigidbody);
        }
    }

    private Transform _transform;

    protected Transform cachedTransform
    {
        get
        {
            if (_transform) return transform;
            else return (_transform = transform);
        }
    }

    public PlayerMoveParameters moveParameters;
    public RollParameters rollParameters;

    public Transform upperBody;
    
    [HideInInspector]
    public bool isShifting; // Бежит ли с ускорением
    [HideInInspector]
    public bool controlEnabled;

    private bool isGrounded;
    private Transform groundCheck;
    private bool jump, roll;

    void Awake()
    {
        groundCheck = transform.FindChild("groundCheck");
    }

    void Update()
    {
        isGrounded = Physics.Linecast(cachedTransform.position, groundCheck.position);
    }

    void FixedUpdate()
    {
        // Если управление отключено - двигаемся согласно текущей скорости
        if (controlEnabled)
        {
            var currentSpeed = isShifting ? moveParameters.runSpeed : moveParameters.normalSpeed;

            if (facingDirection == Vector3.zero) // Если нет направления движения ...
            {
                facingDirection = movementDirection;
                // ... перестаем поворачиваться
                cachedRigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                float angle = AngleAroundAxis(transform.forward, facingDirection, Vector3.up);
                cachedRigidbody.angularVelocity = (Vector3.up * angle * moveParameters.rotationSpeed);
            }

            Vector3 velocity = movementDirection * currentSpeed;
            Vector3 deltaVelocity = velocity - new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
            deltaVelocity.y = 0;

            if (isGrounded)
            {
                cachedRigidbody.AddForce(deltaVelocity * moveParameters.movementAcceleration);
            }

            if (roll && isGrounded)
            {
                var rollDirection = (movementDirection == Vector3.zero) ? facingDirection : movementDirection;
                print("face: " + facingDirection.normalized + " / move: " + movementDirection.normalized);
                print(rollDirection.normalized);

                StopCoroutine("RollCoroutine");
                StartCoroutine("RollCoroutine", rollDirection.normalized);
            }

            if (jump && isGrounded)
            {
                var jumpDirection = (movementDirection == Vector3.zero) ? facingDirection : movementDirection;
                cachedRigidbody.AddForce((Vector3.up + jumpDirection.normalized) * moveParameters.jumpForce, ForceMode.Impulse);
            }
        }

        roll = false;
        jump = false;
    }

    void FreezeMovementControl(float time)
    {
        controlEnabled = false;

        Invoke("ActivateMovementControl", time);
    }

    void ActivateMovementControl()
    {
        controlEnabled = true;
    }

    public void Jump()
    {
        jump = true;
    }

    public void Roll()
    {
        roll = true;
    }

    IEnumerator RollCoroutine(Vector3 direction)
    {
        float time = 0f;
        direction.y = 0;

        controlEnabled = false;
        cachedRigidbody.velocity = Vector3.zero;

        while (time < rollParameters.Duration)
        {
            // Вычисляем силу, которую нужно приложить к игроку на основе кривой переката
            var force = rollParameters.TimeForceCurve.Evaluate(time / rollParameters.Duration) * rollParameters.Force;

            cachedRigidbody.MovePosition(cachedTransform.position + direction * force * Time.fixedDeltaTime);
            //cachedRigidbody.AddForce(direction * force);

            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        controlEnabled = true;
    }

    /// <summary>
    /// Минимальный угол между направлениями по оси
    /// </summary>
    /// <param name="dirA">Первое направление</param>
    /// <param name="dirB">Второе направление</param>
    /// <param name="axis">По какой оси считается угол</param>
    static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
	    // Проекция векторов на ось axis
	    dirA = dirA - Vector3.Project (dirA, axis);
	    dirB = dirB - Vector3.Project (dirB, axis);
	   
	    // Определяем угол между векторами
	    var angle = Vector3.Angle (dirA, dirB);
	   
	    // Угол между целями умножается на -1 если угол между ними больше 90 градусов
	    return angle * (Vector3.Dot (axis, Vector3.Cross (dirA, dirB)) < 0 ? -1 : 1);
	}
}
