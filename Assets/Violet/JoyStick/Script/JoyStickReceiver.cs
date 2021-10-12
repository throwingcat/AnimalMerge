using UnityEngine;

[SerializeField]
public interface IJoystickReceiver
{
    void OnDrag(Vector3 direction, float range);
    void OnPointerUp();
    void OnPointerDown();
}