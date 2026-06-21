using UnityEngine;

public class FaceCamera : MonoBehaviour {
    Transform cam;
    void Start() => cam = Camera.main.transform;
    void LateUpdate() => transform.forward = cam.forward; // 永遠面對相機
}
