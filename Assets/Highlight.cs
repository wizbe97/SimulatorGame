using UnityEngine;
using System.Collections.Generic;

public class Highlight : MonoBehaviour
{
    [Header("Highlight Materials")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;

    private Renderer rend;
    private List<Material> originalMaterials = new List<Material>();

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalMaterials.AddRange(rend.sharedMaterials);
    }

    private void SetMaterial(Material mat)
    {
        if (rend == null || mat == null) return;

        // Copy original materials, then append the highlight one
        var mats = new List<Material>(originalMaterials);
        mats.Add(mat);
        rend.materials = mats.ToArray();
    }

    private void ClearHighlight()
    {
        if (rend == null) return;
        rend.materials = originalMaterials.ToArray();
    }

    // Public API
    public void ShowOutline(bool enable)
    {
        if (enable) SetMaterial(outlineMaterial);
        else ClearHighlight();
    }

    public void ShowValidPlacement()
    {
        SetMaterial(validMaterial);
    }

    public void ShowInvalidPlacement()
    {
        SetMaterial(invalidMaterial);
    }

    public void ResetToDefault()
    {
        ClearHighlight();
    }
}
