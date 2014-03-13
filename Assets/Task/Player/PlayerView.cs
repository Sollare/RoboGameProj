using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerView : MonoBehaviour
{
    private Animator _animator;
    private Transform _faceDirection;

    private Transform _mainCameraTransform;

    private Vector2 _lastInput;

	void Start ()
	{
	    _animator = GetComponent<Animator>();
        _faceDirection = transform.FindChildInHierarchy("FaceDirection");

	    _mainCameraTransform = Camera.main.transform;
	}

    public void Move(Vector2 input)
    {
        // Получаем угол между направлением взгляда и входными данными перемещения
        var a = SignedAngle(new Vector3(input.x, 0, input.y), _faceDirection.forward);

        if (a < 0)
            a *= - 1f;
        else
            a = 360 - a; 

        a += _mainCameraTransform.eulerAngles.y;

        var aRad = a*Mathf.Deg2Rad;

        // Если входные данные есть (перемещаемся), считаем новый ввод, основываясь на повороте модели
        if (input.x != 0f || input.y != 0f)
        {
            input = new Vector2(Mathf.Sin(aRad), Mathf.Cos(aRad));
        }

        float xVelocity = 0f, yVelocity = 0f;
        float smoothTime = 0.05f;

        // Интерполяция между вводом с предыдущего кадра и новым значением
        input = new Vector2(Mathf.SmoothDamp(_lastInput.x, input.x, ref xVelocity, smoothTime), Mathf.SmoothDamp(_lastInput.y, input.y, ref yVelocity, smoothTime));
        
        _lastInput = input;

        _animator.SetFloat("VelX", input.x);
        _animator.SetFloat("VelZ", input.y);

        // Поворот персонажа
    }

    public void Jump()
    {
        
    }

    public void Roll()
    {
        
    }

    private float SignedAngle(Vector3 a, Vector3 b)
    {
        return Vector3.Angle(a, b) * Mathf.Sign(Vector3.Cross(a, b).y);
    }
}
