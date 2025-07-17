using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform mainCamera;
    public float moveSpeed = 5f;

    private float inputX, inputY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(inputX, inputY).normalized;
        rb.velocity = movement * moveSpeed;

        // 使角色始終面對攝影機
        Vector3 cameraDirection = mainCamera.position - transform.position;
        cameraDirection.y = 0; // 鎖定 Y 軸，避免角色傾斜
        transform.forward = -cameraDirection.normalized;
    }
}
