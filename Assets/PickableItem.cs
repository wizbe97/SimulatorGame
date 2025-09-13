using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickableItem : MonoBehaviour, IPickable
{
    private Collider col;
    private Highlight highlight;
    private Transform originalParent;
    private bool isColliding;

    private float lockedY;              // lock Y height at pickup
    private Quaternion lockedRotation;  // lock rotation at pickup

    // optional cache so we still satisfy IPickable.OnHeld()
    private float cachedDistance;

    private void Awake()
    {
        col = GetComponent<Collider>();
        highlight = GetComponent<Highlight>();
    }

    public GameObject PickUp(Transform newParent)
    {
        originalParent = transform.parent;

        // Make collider a trigger while held
        col.isTrigger = true;

        // Save world rotation & Y height at pickup
        lockedRotation = transform.rotation;
        lockedY = transform.position.y;

        // Reparent to player but keep world transform
        transform.SetParent(newParent, true);

        return gameObject;
    }

    public void Place()
    {
        col.isTrigger = false;
        highlight?.ResetToDefault();

        // Detach from player
        transform.SetParent(null, true);

        // Snap to grid, keep locked Y
        Vector3 snapped = GridManager.Instance.SnapToGrid(transform.position);
        snapped.y = lockedY;

        transform.position = snapped;
        transform.rotation = lockedRotation;
    }

    // Interface requirement; delegates to cached value (set every frame by OnHeld(distance))
    public void OnHeld()
    {
        OnHeld(cachedDistance);
    }

    // Called by PlayerInteraction with the computed distance each frame
    public void OnHeld(float placeDistance)
    {
        cachedDistance = placeDistance; // keep for interface call

        if (highlight == null) return;

        if (transform.parent != null)
        {
            // Move along player's forward on XZ plane by the provided distance
            Vector3 dir = transform.parent.forward;
            dir.y = 0f;                // don't tilt with pitch; pitch only affects the distance we were given
            if (dir.sqrMagnitude > 0f) dir.Normalize();

            Vector3 targetPos = transform.parent.position + dir * placeDistance;

            // Snap to global grid, keep locked Y
            targetPos = GridManager.Instance.SnapToGrid(targetPos);
            targetPos.y = lockedY;

            transform.position = targetPos;
            transform.rotation = lockedRotation; // never rotate with player
        }

        // Highlight validity
        if (isColliding) highlight.ShowInvalidPlacement();
        else             highlight.ShowValidPlacement();
    }

    public void OnDropped()
    {
        highlight?.ResetToDefault();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!col.isTrigger) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) return; // ignore ground
        isColliding = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!col.isTrigger) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) return; // ignore ground
        isColliding = false;
    }

    public bool CanPlace() => !isColliding;
}
