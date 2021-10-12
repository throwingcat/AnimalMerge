using UnityEngine;
using Violet;

public class RenderTextureManager : MonoSingleton<RenderTextureManager>
{
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private GameObject _camera;


    private GameObject _model;

    public RenderTexture Capture(GameObject obj)
    {
        _camera.SetActive(true);
        if (_model != null)
            Destroy(_model);

        _model = obj;

        _model.gameObject.layer = LayerMask.NameToLayer("RenderTexture");
        var transforms = _model.transform.GetComponentsInChildren<Transform>();
        foreach (var t in transforms)
            t.gameObject.layer = LayerMask.NameToLayer("RenderTexture");

        _model.transform.SetParent(transform);
        _model.transform.localPosition = Vector3.zero;
        _model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        _model.transform.localScale = Vector3.one;

        return _renderTexture;
    }

    public void Release()
    {
        _camera.SetActive(false);
        if (_model != null)
            Destroy(_model);
    }
}