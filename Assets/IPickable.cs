using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickable
{
    GameObject PickUp(Transform newParent);
    void Place();

    void OnHeld();
    void OnDropped();
}

