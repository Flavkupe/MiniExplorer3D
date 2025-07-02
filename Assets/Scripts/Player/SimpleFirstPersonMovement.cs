using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * h + transform.forward * v).normalized * moveSpeed;

        // No gravity, no vertical movement
        move.y = 0;

        controller.Move(move * Time.deltaTime);
    }
}
