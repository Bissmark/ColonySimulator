using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float minSpeed = 0.01f; // Adjust this value to change the minimum speed
    public float maxSpeed = 0.5f; // Adjust this value to change the maximum speed
    public float zoomAmount = 10.0f; // Adjust this value to change the zoom amount
    public float minZoom = 0.5f;
    public float maxZoom = -50.0f;

    void Update()
    {
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        float zoomLevel = Mathf.Clamp(transform.position.z, maxZoom, minZoom); // Clamp zoom level to minZoom and maxZoom

        // Calculate the interpolation factor based on the current zoom level
        float t = Mathf.InverseLerp(minZoom, maxZoom, zoomLevel);

        // Interpolate between minSpeed and maxSpeed based on the interpolation factor
        float zoomedSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);

        // Check if scrollValue is not zero AND if the new position after zoom will be within the specified range
        if (scrollValue != 0f && (transform.position.z + scrollValue * zoomAmount) > maxZoom && (transform.position.z + scrollValue * zoomAmount) < minZoom)
        {
            // Zoom in/out by a fixed amount
            transform.Translate(0, 0, scrollValue * zoomAmount, Space.Self);
        }

        // Calculate movement speed based on zoom level
        float xAxisValue = Input.GetAxis("Horizontal") * zoomedSpeed;
        float yAxisValue = Input.GetAxis("Vertical") * zoomedSpeed;

        // Update camera position
        transform.Translate(xAxisValue, yAxisValue, 0);
    }
}
