using UnityEngine;

namespace vnc.FX
{
    public class CameraController : MonoBehaviour
    {
        public WaterCamera waterCamera;
        public float minHeight = 20;
        public float speed = 6f;

        private void Update()
        {
            int direction = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            transform.position += Vector3.up * direction * speed * Time.deltaTime;

            waterCamera.effectActive = transform.position.y < minHeight;
        }

    }
}

