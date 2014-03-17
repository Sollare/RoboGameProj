using UnityEngine;
using System.Collections;

public class CameraControllerAdvanced : MonoBehaviour
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
    private Vector3 transitionCameraOffset = Vector3.zero;

    private Quaternion screenMovementSpace;
    private Vector3 screenMovementForward;
    private Vector3 screenMovementRight;

    private Transform _cursor;

    // Выравнивание камеры по вертикали

    // Шаг угла на который камера может подниматься и опускаться (рейкасты идут под этими углами)

    // Задержка перед перемещением камеры в позицию, где она видит цель
    public float ObstacleAvoidanceDelay = 1f;

    private float clearSightAngleStep = 5f;

    public LayerMask obstacleLayer;

    private float clearSightInitialX = 0f;

    private Quaternion initialRotation;

    // Позиция и поворот в предыдущем кадре
    private Vector3 lastFramePosition;
    private Quaternion lastFrameRotation;

    // Должна ли камера перемещаться к новой позиции, где она видит цель
    private bool shouldTransite;
    private float shouldTransiteTime = 0f;

    void Awake()
    {
        _cursor = GameObject.Find("Cursor").transform;

        initialRotation = cachedTransform.rotation;
        initCameraOffset = (cachedTransform.position - target.position);

        lastFramePosition = cachedTransform.position;
        lastFrameRotation = cachedTransform.rotation;

        // Направления движения
        screenMovementSpace = Quaternion.Euler(0, cachedTransform.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;

        clearSightInitialX = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        // Помещаем камеру в исходное положение
        cachedTransform.position = target.position + initCameraOffset;
        cachedTransform.rotation = initialRotation;

        // Определяем нужную позицию и повороту для того, чтобы видеть цель

        // Предполагаем, что цель видима из текущей позиции...
        var visibilityPosition = cachedTransform.position;
        var visibilityRotation = cachedTransform.rotation;

        // Находим позицию и поворот, где цель будет видна
        for (float angle = clearSightInitialX; angle < 90f; angle += clearSightAngleStep)
        {
            if (IsTargetVisible(target))
            {
                // Нашли нужную позицию!
                visibilityPosition = cachedTransform.position;
                visibilityRotation = cachedTransform.rotation;
                break;
            }
            else
                cachedTransform.RotateAround(target.position, cachedTransform.right, clearSightAngleStep);
        }

        // Возвращаем в положение в предыдущем кадре
        cachedTransform.position = lastFramePosition;
        cachedTransform.rotation = lastFrameRotation;

        // Если угол между текущим поворотом камеры и поворотом, где цель видима, больше чем шаг перемещения камеры ...
        if (!shouldTransite)
        {
            if (Quaternion.Angle(cachedTransform.rotation, visibilityRotation) >= clearSightAngleStep)
            {
                // Увеличиваем время ожидания активации перемещения
                shouldTransiteTime = Mathf.Clamp(shouldTransiteTime + Time.deltaTime, 0f, ObstacleAvoidanceDelay);
            }
            else
            {
                // Уменьшаем время ожидания активации перемещения
                shouldTransiteTime = Mathf.Clamp(shouldTransiteTime - Time.deltaTime, 0f, ObstacleAvoidanceDelay);
            }

            // Если с моментам, как мы определили, что должны переместиться прошло определенное время (задержка) ...
            if (shouldTransiteTime >= ObstacleAvoidanceDelay)
            {
                shouldTransite = true;
                // Включаем триггер, что должны переместить камеру в положение, где она будет видеть цель
                shouldTransiteTime = 0f;
            }
        }

        Vector3 targetPosition = Vector3.zero;
        Quaternion targetRotation = Quaternion.identity;

        if (shouldTransite) // Если должны переместить камеру по вертикали ...
        {
            // .. целевая позиция и поворот - такие, в которых видно цель
            targetPosition = visibilityPosition;
            targetRotation = visibilityRotation;
        }
        else
        {
            // ... если не должны переместить, то берем позицию изначального смещения от цели и последний поворот
            targetPosition = target.position + initCameraOffset + transitionCameraOffset;
            targetRotation = lastFrameRotation;
        }

        // Если приблизились к целевому углу по вертикали - отключаем вертикальный поворот, будем ждать снова
        if (shouldTransite && Quaternion.Angle(cachedTransform.rotation, targetRotation) < 0.1f)
        {
            shouldTransite = false;

            // Смещение от изначального запоминаем
            transitionCameraOffset = cachedTransform.position - (target.position + initCameraOffset);
        }
        
        // Окончательно смещаем камеру
        cachedTransform.position = Vector3.SmoothDamp(lastFramePosition, targetPosition, ref cameraVelocity,
            cameraSmoothing * Time.deltaTime);
        cachedTransform.rotation = Quaternion.Lerp(lastFrameRotation, targetRotation, cameraRotationSmoothing*Time.deltaTime);

        //iTween.RotateUpdate(gameObject, targetRotation.eulerAngles, cameraRotationSmoothing);

        lastFramePosition = cachedTransform.position;
        lastFrameRotation = cachedTransform.rotation;


        // Смещение камеры в зависимости от положения курсора ...
        var cursorScreenPosition = Input.mousePosition;

        float halfWidth = Screen.width / 2f;
        float halfHeight = Screen.height / 2f;
        float maxHalf = Mathf.Max(halfWidth, halfHeight);

        Vector3 posRel = cursorScreenPosition - new Vector3(halfWidth, halfHeight, cursorScreenPosition.z);
        posRel.x /= maxHalf;
        posRel.y /= maxHalf;

        var targetPos = targetPosition;
        targetPos.y = 0;

        var cameraAdjustmentVector = posRel.x * screenMovementRight + posRel.y * screenMovementForward;
        cameraAdjustmentVector.y = 0f;



        var tarPosX = target.position.x - Mathf.Clamp((Screen.width / 2f - cursorScreenPosition.x) / 150, -6, 6);
        var tarPosY = target.position.y - Mathf.Clamp((Screen.height / 2f - cursorScreenPosition.y) / 300, -3, 3);
        var cursorPos = new Vector3(tarPosX, 0, tarPosY);

        var modifier = (cursorPos - targetPosition).magnitude;


        cachedTransform.position += cameraAdjustmentVector * cameraPreview * modifier;
        print(Vector3.Angle(Vector3.down, cachedTransform.forward));
    }

    bool IsTargetVisible(Transform playerTarget)
    {
        Ray ray;

        ray = new Ray(cachedTransform.position, target.position - cachedTransform.position);

        return !(Physics.Raycast(ray, Vector3.Distance(cachedTransform.position, target.position) * 0.9f, obstacleLayer));
    }
}
