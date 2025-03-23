using UnityEngine;

public class PacManController : MonoBehaviour
{
    public float speed = 5f;

    private Vector2 moveInput;

    void Update()
    {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        transform.Translate(moveInput * speed * Time.deltaTime);
    }
}
