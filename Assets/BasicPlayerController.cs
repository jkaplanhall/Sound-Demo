using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPlayerController : MonoBehaviour
{


    [SerializeField]
    Transform CamTransform;


    [SerializeField]
    float MouseSen = 2f;
    

    [SerializeField]
    float MaxSpeed = 1f;

    [SerializeField]
    float Acceleration = 5f;

    [SerializeField]
    float Friction = 20f;


    Vector3 Velocity = Vector3.zero;

    float VerticalLerpPos = .5f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {

        //handle mouse movements

        Vector2 MouseDelta = Vector2.zero;
        MouseDelta.x = Input.GetAxis("Mouse X") * MouseSen;
        MouseDelta.y = Input.GetAxis("Mouse Y") * MouseSen;




        transform.Rotate(0, MouseDelta.x, 0);

        VerticalLerpPos = Mathf.Clamp(VerticalLerpPos - MouseDelta.y / Screen.height, 0, 1);

        
        CamTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(-90, 0, 0), Quaternion.Euler(90, 0, 0), VerticalLerpPos);



        //grab and normalize input
        Vector2 input = GetInput();
        input.Normalize();


        //adjust velocity
        if (input.x != 0)
		{
            Velocity.x += input.x * Acceleration * Time.deltaTime;
		}
        else
        {
            Velocity.x = Mathf.MoveTowards(Velocity.x, 0f, Friction * Time.deltaTime);
        }

        //same for y
        if (input.y != 0)
        {
            Velocity.y += input.y * Acceleration * Time.deltaTime;
        }
        else
		{
            Velocity.y = Mathf.MoveTowards(Velocity.y, 0f, Friction * Time.deltaTime);
		}

        

        //max speed
        Velocity = Vector2.ClampMagnitude(Velocity, MaxSpeed);

        //translate
        transform.Translate(Velocity.x * Time.deltaTime, 0, Velocity.y * Time.deltaTime);



        

    }

    Vector2 GetInput()
	{
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
		{
            input.y += 1;
		}
        if (Input.GetKey(KeyCode.A))
        {
            input.x -= 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            input.y -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            input.x += 1;
        }

        return input;
    }
}
