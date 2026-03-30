using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // 要跟随的目标（玩家）
    public Vector3 offset = new Vector3(0, 0, -10); // 相机与目标的偏移（Z轴保持-10，2D相机）
    public float smoothSpeed = 5f;  // 跟随平滑速度

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
