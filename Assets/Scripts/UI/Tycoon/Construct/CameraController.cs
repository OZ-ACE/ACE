using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("가구 설치 연출")]
    [SerializeField] private float _furnitureFocusZoom = 4f;
    [SerializeField] private float _furnitureFocusDuration = 0.5f;
    [SerializeField] private float _furnitureShowDuration = 0.4f;
    [SerializeField] private float _furnitureRestoreDuration = 0.5f;

    [SerializeField] private CinemachineCamera _cinemachineCamera;

    private bool _isFurnitureFocusPlaying;

    private Vector3 _previousCameraPosition;
    private float _previousCameraZoom;

    private bool _isFocusPlaying;

    private Transform _focusCameraTransform;
    private Vector3 _previousPosition;
    private float _previousZoom;

    private Camera _camera;
    private bool _isDragging = false;
    private Vector2 _lastMousePosition;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_isFurnitureFocusPlaying == true)
        {
            return;
        }

        if (Keyboard.current == null || EventSystem.current == null || EventSystem.current.IsPointerOverGameObject())
        {
            _isDragging = false;
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

        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            _isDragging = true;
            _lastMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = currentMousePosition - _lastMousePosition;

            if (mouseDelta.sqrMagnitude > 0.01f)
            {
                float dragX = -mouseDelta.x * _dragSpeed;
                float dragY = -mouseDelta.y * _dragSpeed;

                MoveCamera(dragX, dragY, isDrag: true);

                _lastMousePosition = currentMousePosition;
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

    public async UniTask FocusFurnitureAsync(Vector3 targetPosition)
    {
        if (_isFurnitureFocusPlaying == true)
        {
            return;
        }

        _isFurnitureFocusPlaying = true;

        Transform cameraTransform = GetControlledCameraTransform();

        if (cameraTransform == null)
        {
            Debug.LogWarning("CameraController - 제어할 카메라가 없습니다.");
            _isFurnitureFocusPlaying = false;
            return;
        }

        _previousCameraPosition = cameraTransform.position;
        _previousCameraZoom = GetCurrentZoom();

        Vector3 focusPosition = new Vector3(targetPosition.x, targetPosition.y, _previousCameraPosition.z);

        cameraTransform.DOKill();

        cameraTransform.DOMove(focusPosition, _furnitureFocusDuration).SetEase(Ease.InOutQuad);

        DOTween.To(GetCurrentZoom, SetCurrentZoom, _furnitureFocusZoom, _furnitureFocusDuration).SetEase(Ease.InOutQuad);

        await UniTask.Delay(Mathf.CeilToInt(_furnitureFocusDuration * 1000f));
    }

    public async UniTask RestoreFurnitureFocusAsync()
    {
        Transform cameraTransform = GetControlledCameraTransform();

        if (cameraTransform == null)
        {
            _isFurnitureFocusPlaying = false;
            return;
        }

        cameraTransform.DOKill();

        cameraTransform.DOMove(_previousCameraPosition, _furnitureRestoreDuration).SetEase(Ease.InOutQuad);

        DOTween.To(GetCurrentZoom, SetCurrentZoom, _previousCameraZoom, _furnitureRestoreDuration). SetEase(Ease.InOutQuad);

        await UniTask.Delay(Mathf.CeilToInt(_furnitureRestoreDuration * 1000f));

        _isFurnitureFocusPlaying = false;
    }

    public int GetFurnitureShowDurationMilliseconds()
    {
        return Mathf.CeilToInt(_furnitureShowDuration * 1000f);
    }

    private Transform GetControlledCameraTransform()
    {
        if (_cinemachineCamera != null)
        {
            return _cinemachineCamera.transform;
        }

        return transform;
    }

    private float GetCurrentZoom()
    {
        if (_cinemachineCamera != null)
        {
            return _cinemachineCamera.Lens.OrthographicSize;
        }

        if (_camera != null && _camera.orthographic == true)
        {
            return _camera.orthographicSize;
        }

        return _furnitureFocusZoom;
    }

    private void SetCurrentZoom(float zoom)
    {
        if (_cinemachineCamera != null)
        {
            LensSettings lens = _cinemachineCamera.Lens;
            lens.OrthographicSize = zoom;
            _cinemachineCamera.Lens = lens;
            return;
        }

        if (_camera != null && _camera.orthographic == true)
        {
            _camera.orthographicSize = zoom;
        }
    }
}
