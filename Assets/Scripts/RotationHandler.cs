using UnityEngine;
using UnityEngine.InputSystem;

public class RotationHandler : MonoBehaviour
{
    public bool leftClick { get; private set; }
    public bool rightClick { get; private set; }
    private Vector2 mouseDelta;

    private float angleLimits = 85f;
    private float currentXAngle = 0f, currentYAngle = 0f;
    private float horizontalMouseSensitivity = .1f, verticalMouseSensitivity = .06f;

    private void Awake()
    {
        PointerInitialization();
    }

    private void Update()
    {
        if (leftClick) AngleTheCamera();
    }

    public void OnLeftClick(InputValue iv) => leftClick = iv.Get<float>() > 0 ? true : false;
    public void OnRightClick(InputValue iv) => rightClick = iv.Get<float>() > 0 ? true : false;
    public void OnMouseDelta(InputValue iv) => mouseDelta = iv.Get<Vector2>();

    public Vector2 GetMouseDelta()
    {
        //On retourne la valeur modifiee par la sensibilite, vu que c'est la valeur que le joueur va "voir" au final
        return new Vector2(mouseDelta.x * horizontalMouseSensitivity, mouseDelta.y * (mouseDelta.x > 2.5f ? (verticalMouseSensitivity / 2f) : verticalMouseSensitivity));
    }

    private void PointerInitialization()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void AngleTheCamera()
    {
        currentXAngle = Mathf.Clamp(currentXAngle + (mouseDelta.y * (mouseDelta.x > 2.5f ? (verticalMouseSensitivity / 2f) : verticalMouseSensitivity)), -angleLimits, angleLimits);
        currentYAngle += mouseDelta.x * horizontalMouseSensitivity;

        transform.rotation = Quaternion.Euler(currentXAngle, currentYAngle, 0);
    }
}
