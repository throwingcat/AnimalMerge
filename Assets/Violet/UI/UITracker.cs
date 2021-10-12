using UnityEngine;

public class UITracker : MonoBehaviour
{
    public RectTransform RectTranform;
    public Transform AttachedTransform;

    private Vector2 Offset = Vector2.zero;

    public void Update()
    {
        if (RectTranform == null || AttachedTransform == null) return;

        // Utils.WorldToCanvas(
        //     ref RectTranform,
        //     ViliageService.Instance.CameraController.Camera,
        //     AttachedTransform.position + (Vector3) Offset,
        //     UIManager.Instance.GetLayer(UIManager.eUILayer.StaticCanvas).CachedRectTransform);
    }

    public void SetTranking(Transform target, Vector2 offset = default)
    {
        AttachedTransform = target;
        Offset = offset;
    }
}