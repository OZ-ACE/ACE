using Newtonsoft.Json.Bson;
using NUnit.Framework.Internal;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;



//건설용 그리드 카메라 이동, MainCamera에 붙이도록 만듬
public class CameraController : MonoBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float _moveSpeed = 10f;

    [Header("이동 범위 (격자를 벗어나지 않게)")]
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 10f;
    [SerializeField] private float _minY = -4f;
    [SerializeField] private float _maxY = 4f;

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

        if (x == 0f && y == 0f)
        {
            return;
        }

        MoveCamera(x, y);
    }

    private void MoveCamera(float x, float y)
    {
        Vector3 move = new Vector3(x,y,0f);
        Vector3 next = transform.position + move.normalized * _moveSpeed * Time.deltaTime;

        next.x = Mathf.Clamp(next.x, _minX, _maxX);
        next.y = Mathf.Clamp(next.y, _minY, _maxY);

        transform.position = next;
    }
}
