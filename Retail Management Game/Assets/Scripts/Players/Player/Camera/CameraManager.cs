using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum lockAxisOptions
{
    None,
    X,
    Y,
    Z,
    XY,
    XZ,
    XYZ
}

public class CameraManager : MonoBehaviour 
{
    [Header("Global Variables")]
    [Range(0, 90)]
    public float cameraAngleRotation = 45.0f;
    public float cameraMovementDamp = 1.0f;
    public float cameraRotationDamp = 0.2f;
    public float paddingAmount = 10.0f;
    [Range(0.0f, 1.0f)]
    public float tildEffectAmount = 0.0f;
    public lockAxisOptions LockAxis = lockAxisOptions.None;

    [Header("Default Player Camera Settings")]

    [Header("References")]
    public Camera mainCamera;
    public Transform cameraAim;
    public Transform viewingArea;

    [Header("Debugging")]
    public bool showDebug = false;

    [HideInInspector]
    public GameManager gameManager;

    Vector3 cameraVelocity = Vector3.zero;
    Vector3 dollyVelocity = Vector3.zero;
    Vector3 aimVelocity = Vector3.zero;
    float cameraDistance = 0.0f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (viewingArea)
            Gizmos.DrawWireCube(viewingArea.position, viewingArea.localScale);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        if (cameraAim)
            Gizmos.DrawWireSphere(cameraAim.position, 0.1f);

        Gizmos.color = Color.blue;

        if (cameraAim && mainCamera)
            Gizmos.DrawLine(mainCamera.transform.position, cameraAim.position);
    }

    private void OnValidate()
    {

    }

    private void Awake()
    {

    }

    // Use this for initialization
    void Start () {
        // Caching
		gameManager = GameManager.GetInstance();

        // Set camera to starting position
        Vector3 tiltAddedPosition = Vector3.Lerp(GetViewableBounds().center, GetPlayersBounds().center, tildEffectAmount);

        transform.position = CalculateCameraPosition();
        transform.eulerAngles = new Vector3(cameraAngleRotation, 0.0f, 0.0f);

        mainCamera.transform.localPosition = new Vector3(0.0f, 0.0f, cameraDistance);

        cameraAim.position = tiltAddedPosition;

        mainCamera.transform.LookAt(cameraAim);
    }

    void Update()
    {


    }

    // Update is called once per frame
    void LateUpdate () 
    {
        Vector3 tiltAddedPosition = Vector3.Lerp(GetViewableBounds().center, GetPlayersBounds().center, tildEffectAmount);

        Debug.DrawLine(CalculateCameraPosition(), GetPlayersBounds().center, Color.cyan);

        // Move camera rig position
        transform.position = Vector3.SmoothDamp(transform.position, CalculateCameraPosition(), ref cameraVelocity, cameraMovementDamp);
        transform.eulerAngles = new Vector3(cameraAngleRotation, 0.0f, 0.0f);

        // Move camera
        mainCamera.transform.localPosition = Vector3.SmoothDamp(mainCamera.transform.localPosition, new Vector3(0.0f, 0.0f, cameraDistance), ref dollyVelocity, cameraMovementDamp);

        // Move aim position
        Vector3 newCameraAimPos = Vector3.SmoothDamp(cameraAim.position, tiltAddedPosition, ref aimVelocity, cameraRotationDamp);

        if (LockAxis == lockAxisOptions.X || LockAxis == lockAxisOptions.XY || LockAxis == lockAxisOptions.XYZ || LockAxis == lockAxisOptions.XZ)
            newCameraAimPos.x = transform.position.x;

        if (LockAxis == lockAxisOptions.Y || LockAxis == lockAxisOptions.XY || LockAxis == lockAxisOptions.XYZ)
            newCameraAimPos.y = transform.position.y;

        if (LockAxis == lockAxisOptions.Z || LockAxis == lockAxisOptions.XYZ || LockAxis == lockAxisOptions.XZ)
            newCameraAimPos.z = transform.position.z;


        cameraAim.position = newCameraAimPos;

        // Make camera point at aim position
        mainCamera.transform.LookAt(cameraAim);
    }

    Vector3 CalculateCameraPosition()
    {
        // Picking the lowest FOV (height vs width) according to the screen resolution
        // Lowest FOV wins
        float lowestFOV;

        if (Camera.main.fieldOfView < GetCameraFOVHeight(mainCamera))
        {
            lowestFOV = Camera.main.fieldOfView;
        }
        else
        {
            lowestFOV = GetCameraFOVHeight(mainCamera) * 0.5f;
        }

        Vector3 aimPosition;

        Bounds viewables = GetViewableBounds();

        if (gameManager.GetPlayers().Length == 1)
        {
            aimPosition = gameManager.GetPlayers()[0].position;
        }
        else
        {
            
            aimPosition = viewables.center;
        }

        // Calculate opposite
        float opp = Mathf.Max(viewables.size.x, viewables.size.z) * 0.5f;

        // Add padding to sides
        opp += paddingAmount * 0.5f;

        // Calculate angle
        float halfFOV = (lowestFOV * 0.5f) * Mathf.Deg2Rad;

        // Solve hypotenuse
        cameraDistance = -(opp / Mathf.Tan(halfFOV));

        // Update CameraPivot
        return aimPosition;
    }

    /// <summary>
    /// Calculates the bounding area of all players, but includes the viewing area
    /// </summary>
    /// <returns>Bounds of all viewing objects</returns>
    Bounds GetViewableBounds()
    {
        // Get bounds of everything in viewing area
        Bounds tempBounds = GetPlayersBounds();

        Vector3[] viewingAreaCorners = EssentialFunctions.GetAllCornersOfTransform(viewingArea);

        for (int c = 0; c < viewingAreaCorners.Length; c++)
        {
            tempBounds.Encapsulate(viewingAreaCorners[c]);
        }

        return tempBounds;
    }

    /// <summary>
    /// Calculates the bounding area of all the players
    /// </summary>
    /// <returns>Bounds of all players</returns>
    Bounds GetPlayersBounds()
    {
        Bounds tempBounds = new Bounds(gameManager.GetPlayers()[0].position, Vector3.zero);

        for (int i = 1; i < gameManager.GetPlayers().Length; i++)
        {
            tempBounds.Encapsulate(gameManager.GetPlayers()[i].position);
        }

        return tempBounds;
    }

    /// <summary>
    /// Calculates the vertical field of view of the camera based on the screen aspect ratio
    /// </summary>
    /// <param name="cam">The camera to take the horizontal FOV from.</param>
    /// <returns>The vertial field of view</returns>
    public static float GetCameraFOVHeight(Camera cam)
    {
        // Get camera FOV
        float FOV_W = cam.fieldOfView;

        // Calculate adjacent
        float adjacent = (GetScreenResolution().x * 0.5f) * Mathf.Tan((FOV_W * 0.5f) * Mathf.Deg2Rad);
        float FOV_H = (((GetScreenResolution().y * 0.5f) / adjacent) * Mathf.Rad2Deg) * 2.0f;

        return FOV_H;
    }

    /// <summary>
    /// Gets the current resolution of the game window
    /// </summary>
    /// <returns>Screen resolution in Vector2</returns>
    public static Vector2 GetScreenResolution()
    {
        return new Vector2(Screen.width, Screen.height);
    }

    //void UpdateGUIScreenSpace()
    //{
    //    float height = 2.0f * guiCamera.orthographicSize;
    //    float width = height * guiCamera.aspect;
    //    float depth = guiCamera.farClipPlane;

    //    guiScreenSpace.GetComponent<BoxCollider>().size = new Vector3(width, height, depth);
    //    guiScreenSpace.transform.localPosition = new Vector3(0.0f, 0.0f, depth * 0.5f);
    //}
}
