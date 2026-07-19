using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;



//건설용 그리드 카메라 이동, MainCamera에 붙이도록 만듬
public class CameraController : MonoBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _dragSpeed = 0.5f;

    [Header("이동 범위 (격자를 벗어나지 않게)")]
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 10f;
    [SerializeField] private float _minY = -4f;
    [SerializeField] private float _maxY = 4f;

    [Header("줌")]
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minZoom = 3f;
    [SerializeField] private float _maxZoom = 12f;

    [SerializeField] private CinemachineCamera _cinemachineCamera;

    private Camera _camera;
    private bool _isDragging = false;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        { 
            return; 
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.aKey.isPressed == true)
        {
            x -= 1f;
        }

        if (Keyboard.current.dKey.isPressed == true)
        {
            x += 1f;
        }

        if (Keyboard.current.sKey.isPressed == true)
        {
            y -= 1f;
        }

        if (Keyboard.current.wKey.isPressed == true)
        {
            y += 1f;
        }

        if (x != 0f || y != 0f)
        {

            MoveCamera(x, y, false);
        }

        _isDragging = Mouse.current.middleButton.wasPressedThisFrame ? true : false;

        if (_isDragging)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            if (mouseDelta.sqrMagnitude > 0.01f)
            {
                float dragX = -mouseDelta.x * _dragSpeed * Time.deltaTime;
                float dragY = -mouseDelta.y * _dragSpeed * Time.deltaTime;

                MoveCamera(dragX, dragY, true);
            }
        }

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0f)
            {
                ZoomCamera(scroll);
            }
        }
    }

    private void MoveCamera(float x, float y, bool isDrag)
    {
        Vector3 next;

        if (isDrag)
        {
            next = transform.position + new Vector3(x, y, 0f);
        }
        else
        {
            Vector3 move = new Vector3(x, y, 0f);
            next = transform.position + move.normalized * _moveSpeed * Time.deltaTime;
        }

        next.x = Mathf.Clamp(next.x, _minX, _maxX);
        next.y = Mathf.Clamp(next.y, _minY, _maxY);

        transform.position = next;
    }

    private void ZoomCamera(float scroll)
    {
        if (_cinemachineCamera != null)
        {
            var lens = _cinemachineCamera.Lens;
            float size = lens.OrthographicSize;
            size -= Mathf.Sign(scroll) * _zoomSpeed;
            lens.OrthographicSize = Mathf.Clamp(size, _minZoom, _maxZoom);

            _cinemachineCamera.Lens = lens;
        }
        else if (_camera != null && _camera.orthographic)
        {
            float size = _camera.orthographicSize;
            size -= Mathf.Sign(scroll) * _zoomSpeed;
            _camera.orthographicSize = Mathf.Clamp(size, _minZoom, _maxZoom);
        }
    }
}
