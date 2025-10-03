using System.Collections.Generic;
using UnityEngine;

public class SkyBoxSetter : MonoBehaviour
{
    [SerializeField] private List<Material> _skyboxMaterials;

    void OnEnable()
    {
        ChangeSkybox(0);
    }

    public void ChangeSkybox(int skyBoxIndex)
    {
        if (skyBoxIndex >= 0 && skyBoxIndex < _skyboxMaterials.Count)
        {
            RenderSettings.skybox = _skyboxMaterials[skyBoxIndex];
            DynamicGI.UpdateEnvironment(); // Optional, if you're using baked lighting or reflection probes
        }
    }
}
