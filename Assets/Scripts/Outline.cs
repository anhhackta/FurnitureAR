using UnityEngine;

public class Outline : MonoBehaviour
{
    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer)
        {
            Material outlineMat = new Material(Shader.Find("Standard"));
            outlineMat.SetColor("_Color", Color.yellow);
            renderer.materials = new Material[] { renderer.material, outlineMat };
        }
    }
}