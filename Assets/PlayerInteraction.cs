using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask pickableLayerMask;

    [Header("Placement Distances")]
    [SerializeField][Min(0.1f)] private float minPlacementDistance = 1f;
    [SerializeField][Min(0.1f)] private float maxPlacementDistance = 5f;

    [Header("Pickup")]
    [SerializeField] private Transform pickUpParent;
    [SerializeField] private GameObject inHandItem;

    private Transform playerCameraTransform;
    private RaycastHit hit;
    private PlayerInputHandler inputHandler;

    private Highlight lastHighlight;

    private void Awake()
    {
        playerCameraTransform = Camera.main.transform;
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()  => SubscribeInput(true);
    private void OnDisable() => SubscribeInput(false);

    private void SubscribeInput(bool subscribe)
    {
        if (inputHandler == null) return;

        if (subscribe)
        {
            inputHandler.OnPickUpItem += PickUpItem;
            inputHandler.OnPlaceItem += PlaceItem;
        }
        else
        {
            inputHandler.OnPickUpItem -= PickUpItem;
            inputHandler.OnPlaceItem -= PlaceItem;
        }
    }

    private void PlaceItem()
    {
        if (inHandItem == null) return;

        var pickable = inHandItem.GetComponent<PickableItem>();
        if (pickable != null && pickable.CanPlace())
        {
            pickable.Place();
            pickable.OnDropped();
            inHandItem = null;
        }
        else
        {
            Debug.Log("Can't place here!");
        }
    }

    private void PickUpItem()
    {
        if (hit.collider == null || inHandItem != null) return;

        var pickable = hit.collider.GetComponent<IPickable>();
        if (pickable != null)
        {
            inHandItem = pickable.PickUp(pickUpParent);
        }
    }

    private void Update()
    {
        // Draw ray to max placement distance
        Debug.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * maxPlacementDistance, Color.red);

        if (inHandItem != null)
        {
            var pickable = inHandItem.GetComponent<PickableItem>();

            // Camera pitch: convert 0..360 to -180..180
            float pitch = playerCameraTransform.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            // Map pitch to distance:
            // looking DOWN (+pitch) -> closer (min)
            // looking UP   (-pitch) -> farther (max)
            // Reverse the inverse-lerp to get that behaviour:
            float clampedPitch = Mathf.Clamp(pitch, -60f, 60f);
            float t = Mathf.InverseLerp( 60f, -60f, clampedPitch); // <-- reversed ends
            float distance = Mathf.Lerp(minPlacementDistance, maxPlacementDistance, t);

            // Drive the item with the computed distance
            pickable?.OnHeld(distance);
            return;
        }

        // Clear last highlight if any
        if (lastHighlight != null)
        {
            lastHighlight.ShowOutline(false);
            lastHighlight = null;
        }

        // Perform raycast for looking highlight (uses maxPlacementDistance as range)
        if (Physics.Raycast(playerCameraTransform.position,
                            playerCameraTransform.forward,
                            out hit,
                            maxPlacementDistance,
                            pickableLayerMask))
        {
            var highlight = hit.collider.GetComponent<Highlight>();
            if (highlight != null)
            {
                highlight.ShowOutline(true);
                lastHighlight = highlight;
            }
        }
    }
}
