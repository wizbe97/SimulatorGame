using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickableItem : MonoBehaviour, IPickable
{
    private Collider col;
    private Highlight highlight;
    private Transform originalParent;
    private bool isColliding;

    private void Awake()
    {
        col = GetComponent<Collider>();
        highlight = GetComponent<Highlight>();
    }

    public GameObject PickUp(Transform newParent)
    {
        originalParent = transform.parent;

        // Temporarily make collider a trigger so it doesn’t push physics
        col.isTrigger = true;

        // Snap to hand (localPosition/Rotation reset relative to parent)
        transform.SetParent(newParent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        return gameObject;
    }

    public void Place()
    {
        // Reset collider
        col.isTrigger = false;

        // Reset highlight state back to original
        if (highlight != null)
        {
            highlight.ResetToDefault();
        }

        // Drop back into world
        transform.SetParent(null, true);

        // Raycast down to place on ground
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 10f))
        {
            Vector3 newPos = hitInfo.point;

            if (!(col is MeshCollider))
            {
                // Only add extents if collider is not a MeshCollider
                float halfHeight = col.bounds.extents.y;
                newPos.y += halfHeight;
            }

            transform.position = newPos;
        }
    }

    public void OnHeld()
    {
        if (highlight == null) return;

        // Show green if no collision, red if colliding
        if (isColliding)
        {
            highlight.ShowInvalidPlacement();
        }
        else
        {
            highlight.ShowValidPlacement();
        }
    }

    public void OnDropped()
    {
        highlight?.ResetToDefault();
    }

    // Trigger checks while held
    private void OnTriggerEnter(Collider other)
    {
        if (col.isTrigger) isColliding = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (col.isTrigger) isColliding = false;
    }

    public bool CanPlace() => !isColliding;
}
