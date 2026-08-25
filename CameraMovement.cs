using UnityEngine;
using Unity.Mathematics;

public class CameraMovement : MonoBehaviour
{
    public float MaxCamX = 10f;
    public float MaxCamY = 10f;

    public float smoothTime = 0.25f;
    public float CamX = 0f;
    public float CamY = 0f;
    public float CamSpeed;

    private Vector3 velocity = Vector3.zero;
    void Start()
    {
    }
    void Update()
    {
        CamX += Input.GetAxisRaw("Horizontal") * Time.deltaTime * CamSpeed;
        CamY += Input.GetAxisRaw("Vertical") * Time.deltaTime * CamSpeed;
        CamX = math.clamp(CamX, -MaxCamX, MaxCamX);
        CamY = math.clamp(CamY, -MaxCamY, MaxCamY);
    }
    void LateUpdate()
    {
        float Xpos = math.clamp(CamX, -MaxCamX, MaxCamX);
        float Ypos = math.clamp(CamY, -MaxCamY, MaxCamY);

        Vector3 targetPosition = new Vector3(Xpos, Ypos, transform.position.z);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}