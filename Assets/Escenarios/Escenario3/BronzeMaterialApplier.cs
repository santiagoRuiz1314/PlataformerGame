using UnityEngine;

[ExecuteInEditMode]
public class BronzeMaterialApplier : MonoBehaviour
{
    [Header("Material a reemplazar")]
    public string materialName = "standardSurface4";

    [Header("Color Bronce")]
    [ColorUsage(false, false)]
    public Color bronzeColor = new Color(0.80f, 0.50f, 0.20f, 1f);
    [Range(0f, 1f)] public float metallic = 0.85f;
    [Range(0f, 1f)] public float smoothness = 0.55f;

    [ContextMenu("Aplicar Bronce Ahora")]
    public void ApplyBronze()
    {
        Material bronzeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bronzeMat.SetColor("_BaseColor", bronzeColor);
        bronzeMat.SetFloat("_Metallic", metallic);
        bronzeMat.SetFloat("_Smoothness", smoothness);
        bronzeMat.SetFloat("_WorkflowMode", 1f);
        bronzeMat.name = "Mat_Bronze";

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in renderers)
        {
            Material[] mats = rend.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].name.Contains(materialName))
                {
                    mats[i] = bronzeMat;
                    changed = true;
                    Debug.Log($"Bronce aplicado al slot [{i}] en '{rend.gameObject.name}'");
                }
            }

            if (changed) rend.sharedMaterials = mats;
        }
    }
}