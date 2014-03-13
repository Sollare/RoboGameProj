using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
    private Transform _transform;

    protected Transform cachedTransform
    {
        get
        {
            if (_transform) return transform;
            else return (_transform = transform);
        }
    }

    private Camera _camera;

    protected Camera cachedCamera
    {
        get
        {
            if (_camera) return camera;
            else return (_camera = camera);
        }
    }

    public float cameraSmoothing = 0.01f;
    public float cameraRotationSmoothing = 0.01f;
    public float cameraPreview = 2.0f;
    public Transform target;

    private Vector3 cameraVelocity = Vector3.zero;
    private Vector3 initCameraOffset;
    
    private Quaternion screenMovementSpace;
    private Vector3 screenMovementForward;
    private Vector3 screenMovementRight;

    // Выравнивание камеры по вертикали
    
    // Шаг угла на который камера может подниматься и опускаться (рейкасты идут под этими углами)
    public float clearSightAngleStep = 5f;

    // Сколько раз в секунду проверяем видимость персонажа
    public int clearSightCheckRate = 5;

    public LayerMask obstacleLayer;

    private float clearSightInitialX = 0f;

    private Vector3 initEulerAngles;

    // Позиция и поворот в предыдущем кадре
    private Vector3 lastFramePosition;
    private Quaternion lastFrameRotation;

    void Awake()
    {
        initEulerAngles = cachedTransform.eulerAngles;
        initCameraOffset = (cachedTransform.position - target.position);

        lastFramePosition = cachedTransform.position;
        lastFrameRotation = cachedTransform.rotation;


        // Направления движения
        screenMovementSpace = Quaternion.Euler(0, cachedTransform.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;

        clearSightInitialX = transform.eulerAngles.x;
    }


	void LateUpdate () 
    {
	    UpdateCameraPosition(Input.mousePosition);
	}

    private void UpdateCameraPosition(Vector3 cursorScreenPosition)
    {
        float halfWidth = Screen.width/2f;
        float halfHeight = Screen.height/2f;
        float maxHalf = Mathf.Max(halfWidth, halfHeight);

        Vector3 posRel = cursorScreenPosition - new Vector3(halfWidth, halfHeight, cursorScreenPosition.z);
        posRel.x /= maxHalf;
        posRel.y /= maxHalf;

        var cameraAdjustmentVector = posRel.x*screenMovementRight + posRel.y*screenMovementForward;
        cameraAdjustmentVector.y = 0f;

        var cameraTargetPosition = target.position + initCameraOffset + cameraAdjustmentVector*cameraPreview;

        // ----------------------- //
        //    ВИДИМОСТЬ ИГРОКА     //
        // ----------------------- //

        if (!IsTargetVisible(target))
        {
            
        }

        cachedTransform.position = cameraTargetPosition;
        cachedTransform.eulerAngles = initEulerAngles;

        var visibilityPosition = cameraTargetPosition;
        var visibilityRotation = Quaternion.Euler(initEulerAngles);

        for (float angle = clearSightInitialX; angle < 90f; angle += clearSightAngleStep)
        {
            if (!IsTargetVisible(target))
            {
                cachedTransform.RotateAround(target.position, cachedTransform.right, clearSightAngleStep);
            }
            else
            {
                visibilityPosition = cachedTransform.position;
                visibilityRotation = cachedTransform.rotation;
                break;
            }
        }

        cachedTransform.position = Vector3.SmoothDamp(lastFramePosition, visibilityPosition, ref cameraVelocity,
            cameraSmoothing*Time.deltaTime);
        cachedTransform.eulerAngles = Vector3.Lerp(lastFrameRotation.eulerAngles, visibilityRotation.eulerAngles,
            cameraRotationSmoothing*Time.deltaTime);

        lastFramePosition = cachedTransform.position;
        lastFrameRotation = cachedTransform.rotation;
    }

    //IEnumerator SeekForAngle()
    //{
        
    //}
    
    bool IsTargetVisible(Transform playerTarget)
    {
        Ray ray;

        ray = new Ray(cachedTransform.position, target.position - cachedTransform.position);

        return !(Physics.Raycast(ray, Vector3.Distance(cachedTransform.position, target.position) * 0.9f, obstacleLayer));
    }
}
