using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask pickableLayerMask;
    [SerializeField][Min(1)] private float hitRange = 3;
    [SerializeField] private Transform pickUpParent;
    [SerializeField] private GameObject inHandItem;

    private Transform playerCameraTransform;
    private RaycastHit hit;
    private PlayerInputHandler inputHandler;

    // Track the last highlighted object so we can clear its outline
    private Highlight lastHighlight;

    private void Awake()
    {
        playerCameraTransform = Camera.main.transform;
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable() => SubscribeInput(true);
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
        if (inHandItem != null)
        {
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
    }

    private void PickUpItem()
    {
        if (hit.collider != null && inHandItem == null)
        {
            var pickable = hit.collider.GetComponent<IPickable>();
            if (pickable != null)
            {
                inHandItem = pickable.PickUp(pickUpParent);
            }
        }
    }

    private void Update()
    {
        Debug.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * hitRange, Color.red);

        // If holding an item, let PickableItem handle its highlight
        if (inHandItem != null)
        {
            var pickable = inHandItem.GetComponent<PickableItem>();
            pickable?.OnHeld();
            return;
        }

        // Clear last highlight if any
        if (lastHighlight != null)
        {
            lastHighlight.ShowOutline(false);
            lastHighlight = null;
        }

        // Perform raycast for looking highlight
        if (Physics.Raycast(playerCameraTransform.position,
                            playerCameraTransform.forward,
                            out hit,
                            hitRange,
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
