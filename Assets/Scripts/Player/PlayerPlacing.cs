using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerPlacing : MonoBehaviour
{
    [SerializeField] private LayerMask pickableLayerMask;
    [SerializeField][Min(1)] private float hitRange = 3;
    private Transform playerCameraTransform;
    private GameObject pickUpUI;


    private RaycastHit hit;

    private void Start()
    {
        playerCameraTransform = Camera.main.transform;
        pickUpUI = GameObject.FindGameObjectWithTag("PickUpPanel");
    }

    private void Update()
    {
        if (hit.collider != null)
        {
            hit.collider.GetComponent<Highlight>()?.ToggleHighlight(false);
            pickUpUI.SetActive(false);
        }
        if (Physics.Raycast(playerCameraTransform.position,
            playerCameraTransform.forward,
            out hit,
            hitRange,
            pickableLayerMask))
        {
            hit.collider.GetComponent<Highlight>()?.ToggleHighlight(true);
            pickUpUI.SetActive(true);
        }
    }
}
