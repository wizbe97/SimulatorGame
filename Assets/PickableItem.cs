using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickableItem : MonoBehaviour, IPickable
{
    [SerializeField] private float minPlacementDistance = 2f;   // minimum push away from player

    private Collider col;
    private Highlight highlight;
    private Transform originalParent;
    private bool isColliding;

    private float pickupDistance;
    private float lockedY;              // lock Y height at pickup
    private Quaternion lockedRotation;  // lock rotation at pickup

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

        // Calculate distance from player
        pickupDistance = Vector3.Distance(newParent.position, transform.position);
        if (pickupDistance < minPlacementDistance)
            pickupDistance = minPlacementDistance;

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

        // Snap to grid but keep locked Y
        Vector3 snapped = GridManager.Instance.SnapToGrid(transform.position);
        snapped.y = lockedY;

        transform.position = snapped;
        transform.rotation = lockedRotation;
    }

    public void OnHeld()
    {
        if (highlight == null) return;

        if (transform.parent != null)
        {
            // Use full forward (with pitch)
            Vector3 dir = transform.parent.forward.normalized;

            // Move object along forward vector
            Vector3 targetPos = transform.parent.position + dir * pickupDistance;

            // Snap to grid, but force Y to locked height
            targetPos = GridManager.Instance.SnapToGrid(targetPos);
            targetPos.y = lockedY;

            transform.position = targetPos;
            transform.rotation = lockedRotation;
        }

        // Placement validity
        if (isColliding) highlight.ShowInvalidPlacement();
        else highlight.ShowValidPlacement();
    }


    public void OnDropped()
    {
        highlight?.ResetToDefault();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!col.isTrigger) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) return;
        isColliding = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!col.isTrigger) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) return;
        isColliding = false;
    }

    public bool CanPlace() => !isColliding;
}
