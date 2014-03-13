using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerView))]
public class PlayerController : MonoBehaviour {

    private PlayerView _view;
    private Transform _transform;

    protected Transform cachedTransform
    {
        get
        {
            if (_transform) return transform;
            else return (_transform = transform);
        }
    }

    public PlayerMotor characterMotor;

    // Cursor settings
    public float cursorPlaneHeight = 0;
    public float cursorFacingCamera = 0;
    public float cursorSmallerWithDistance = 0;
    public float cursorSmallerWhenClose = 1;

    // Private memeber data
    private Camera mainCamera;

    private Transform mainCameraTransform;
    private Transform cursorObject;

    private Plane playerMovementPlane;

    private Quaternion screenMovementSpace;
    private Vector3 screenMovementForward;
    private Vector3 screenMovementRight;


	// Use this for initialization
	void Awake ()
	{
	    mainCamera = Camera.main;
	    mainCameraTransform = mainCamera.transform;

	    characterMotor = GetComponent<PlayerMotor>();
	    _view = GetComponent<PlayerView>();

	    cursorObject = GameObject.Find("Cursor").transform;

        playerMovementPlane = new Plane(cachedTransform.up, cachedTransform.position);

        // Направления движения
        screenMovementSpace = Quaternion.Euler(0, mainCameraTransform.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;
	}

    private bool jump, roll;

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        jump = Input.GetButtonDown("Jump");
        roll = Input.GetButtonDown("Roll");

        
        // Задаем направление движения
        characterMotor.movementDirection = input.x * screenMovementRight + input.y * screenMovementForward;
        characterMotor.isShifting = Input.GetButton("Shift");

        // Плоскость под игроком для управления курсором
        playerMovementPlane.normal = cachedTransform.up;
        playerMovementPlane.distance = -cachedTransform.position.y + cursorPlaneHeight;
        
        // Перемещение камеры в зависимости от курсора
        var cursorScreenPosition = Input.mousePosition;

        // Перемещение курсора в точку пересечения с плейном
        var cursorWorldPosition = ScreenPointToWorldPointOnPlane(cursorScreenPosition, playerMovementPlane, mainCamera);

        // The facing direction is the direction from the character to the cursor world position
        characterMotor.facingDirection = (cursorWorldPosition - cachedTransform.position);
        characterMotor.facingDirection.y = 0;


        if (jump) characterMotor.Jump();
        if (roll) characterMotor.Roll();

        _view.Move(input);

    }

    void LateUpdate()
    {  
        // Управляем курсором
        var cursorWorldPosition = ScreenPointToWorldPointOnPlane(Input.mousePosition, playerMovementPlane, mainCamera);
        MoveCursorToPosition(cursorWorldPosition);
    }

    // Возвращает точку плоскости перемещения персонажа, на которую указывает курсор
    public static Vector3 ScreenPointToWorldPointOnPlane (Vector3 screenPoint, Plane plane, Camera cam) 
    {
        Ray ray = cam.ScreenPointToRay(screenPoint);

        float dist;
        plane.Raycast(ray, out dist);
	
        return ray.GetPoint(dist);
    }

    void MoveCursorToPosition(Vector3 cursorWorldPosition)
    {
        if (!cursorObject)
            return;

        cursorObject.position = cursorWorldPosition;

        // HANDLE CURSOR ROTATION

        var cursorWorldRotation = cursorObject.rotation;
        if (characterMotor.facingDirection != Vector3.zero)
            cursorWorldRotation = Quaternion.LookRotation(characterMotor.facingDirection);

        // Calculate cursor billboard rotation
        var cursorScreenspaceDirection = Input.mousePosition -
                                         mainCamera.WorldToScreenPoint(transform.position +
                                                                       cachedTransform.up*cursorPlaneHeight);
        cursorScreenspaceDirection.z = 0;
        var cursorBillboardRotation = mainCameraTransform.rotation*
                                      Quaternion.LookRotation(cursorScreenspaceDirection, -Vector3.forward);

        // Set cursor rotation
        cursorObject.rotation = Quaternion.Slerp(cursorWorldRotation, cursorBillboardRotation, cursorFacingCamera);


        // HANDLE CURSOR SCALING

        // The cursor is placed in the world so it gets smaller with perspective.
        // Scale it by the inverse of the distance to the camera plane to compensate for that.
        float compensatedScale = 0.1f*
                                 Vector3.Dot(cursorWorldPosition - mainCameraTransform.position,
                                     mainCameraTransform.forward);

        // Make the cursor smaller when close to character
        float cursorScaleMultiplier = Mathf.Lerp(0.7f, 1.0f,
            Mathf.InverseLerp(0.5f, 4.0f, characterMotor.facingDirection.magnitude));

        // Set the scale of the cursor
        cursorObject.localScale = Vector3.one*Mathf.Lerp(compensatedScale, 1, cursorSmallerWithDistance)*
                                  cursorScaleMultiplier;
    }
}
