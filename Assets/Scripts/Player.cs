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

        // �Ϩ���l�׭�����v��
        Vector3 cameraDirection = mainCamera.position - transform.position;
        cameraDirection.y = 0; // ��w Y �b�A�קK����ɱ�
        transform.forward = -cameraDirection.normalized;
    }
}
