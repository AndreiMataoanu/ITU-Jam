using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private float sensitivity;
    [SerializeField] private float defaultSensitivity = 120f;
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 defaultRot = new Vector3(20f, -60f, 0f);
    private Vector3 defaultPos = new Vector3(0f, -0.5f, 0.5f);
    private Vector3 itemBoxRot = new Vector3(20f, -60f, 0f); //rotation when looking at item box
    private Vector3 itemBoxPos = new Vector3(0f, -1f, 1f); //position when looking at item box
    private Vector3 shopRot = new Vector3(10f, -120f, 0f); //rotation when looking at shop
    private Vector3 shopPos = new Vector3(0f, -0.5f, 0.5f); //position when looking at shop

    private bool lookingAtItemBox = false;
    private bool lookingAtShop = false;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isMoving = false;
    private const float moveThreshold = 0.1f;
    private const float rotThreshold = 0.5f;

    private void Start()
    {
        targetPos = defaultPos;
        targetRot = Quaternion.Euler(defaultRot);
        transform.localPosition = defaultPos;
        transform.localEulerAngles = defaultRot;
        sensitivity = defaultSensitivity;
        CursorLock(true);
    }

    void Update()
    {
        //mouseLook
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        transform.Rotate(-y * sensitivity * Time.deltaTime, x * sensitivity * Time.deltaTime, 0);
        Vector3 angles = transform.eulerAngles;
        angles.z = 0;
        transform.eulerAngles = angles;

        if (!lookingAtItemBox && !lookingAtShop && Input.GetKeyDown(KeyCode.D))
        {
            lookingAtItemBox = true;
            targetPos = itemBoxPos;
            targetRot = Quaternion.Euler(itemBoxRot);
            sensitivity = 30f;
            CursorLock(false);
            isMoving = true;
        }
        else if (lookingAtItemBox && Input.GetKeyDown(KeyCode.A))
        {
            lookingAtItemBox = false;
            targetPos = defaultPos;
            targetRot = Quaternion.Euler(defaultRot);
            sensitivity = defaultSensitivity;
            CursorLock(true);
            isMoving = true;
        }
        else if (!lookingAtShop && !lookingAtItemBox && Input.GetKeyDown(KeyCode.A))
        {
            lookingAtShop = true;
            targetPos = shopPos;
            targetRot = Quaternion.Euler(shopRot);
            sensitivity = 30f;
            CursorLock(false);
            isMoving = true;
        }
        else if (lookingAtShop && Input.GetKeyDown(KeyCode.D))
        {
            lookingAtShop = false;
            targetPos = defaultPos;
            targetRot = Quaternion.Euler(defaultRot);
            sensitivity = defaultSensitivity;
            CursorLock(true);
            isMoving = true;
        }

        if (isMoving)
        {
            //Lerp camera to target pos/rot
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * moveSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * moveSpeed);

            //Close enough
            if (Vector3.Distance(transform.localPosition, targetPos) < moveThreshold && Quaternion.Angle(transform.localRotation, targetRot) < rotThreshold)
            {
                transform.localPosition = targetPos;
                transform.localRotation = targetRot;
                isMoving = false;
            }
        }
    }

    private void CursorLock(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
