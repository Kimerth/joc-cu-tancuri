using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    new Rigidbody rigidbody;
    [SerializeField]
    private float speed;

    private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rigidbody.velocity = velocity;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
            switch (context.control.name)
            {
                case "w":
                    velocity.z = speed;
                    break;
                case "s":
                    velocity.z = -speed;
                    break;
                case "a":
                    velocity.x = -speed;
                    break;
                case "d":
                    velocity.x = speed;
                    break;
                default:
                    break;
            }
        else if (context.canceled)
            velocity = Vector3.zero;
    }
}
