using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary_mesh : MonoBehaviour
{
    public OVRInput.Controller L_controller = OVRInput.Controller.LTouch;
    public OVRInput.Controller R_controller = OVRInput.Controller.RTouch;
    public float moveSpeed = 2.0f;
    public float rotationSpeed = 100.0f;

    public Transform cameraRigTransform;       // OVR Camera Rig for rotation
    public Transform centerEyeAnchor;          // Headset center eye anchor (for direction)
    public BoxCollider playerCollider;         // BoxCollider representing player's body

    public BoxCollider boundaryCollider;       // The invisible outer boundary (air wall)
    private Bounds boundaryBounds;

    void Start()
    {
        boundaryBounds = boundaryCollider.bounds;
    }

    void Update()
    {
        // Rotate the player with the left thumbstick
        Vector2 rotationInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, L_controller);
        float rotationAmount = rotationInput.x * rotationSpeed * Time.deltaTime;
        cameraRigTransform.Rotate(0, rotationAmount, 0, Space.World);

        // Move the player with the right thumbstick
        Vector2 movementInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, R_controller);

        Vector3 moveDirection = centerEyeAnchor.forward * movementInput.y + centerEyeAnchor.right * movementInput.x;
        moveDirection.y = 0; // Ignore vertical movement

        Vector3 moveOffset = moveDirection * moveSpeed * Time.deltaTime;

        // Simulate future movement using the full BoxCollider volume
        if (CanMoveToOffset(moveOffset))
        {
            playerCollider.transform.Translate(moveOffset, Space.World);
        }
    }

    bool CanMoveToOffset(Vector3 offset)
    {
        // Prepare parameters for BoxCast
        Vector3 origin = playerCollider.bounds.center;
        Vector3 halfExtents = playerCollider.bounds.extents;
        Quaternion orientation = playerCollider.transform.rotation;
        Vector3 direction = offset.normalized;
        float distance = offset.magnitude;

        // Use BoxCast to simulate collision with walls
        if (Physics.BoxCast(origin, halfExtents, direction, out RaycastHit hit, orientation, distance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return false; // Movement would collide with a wall
            }
        }

        // Simulate future bounds after movement
        Bounds futureBounds = new Bounds(origin + offset, playerCollider.bounds.size);

        // Check if future bounds are still inside the defined boundary
        if (!boundaryBounds.Contains(futureBounds.min) || !boundaryBounds.Contains(futureBounds.max))
        {
            return false; // Movement would go outside the boundary
        }

        return true; // Valid movement: no wall collision and still within boundary
    }
}
