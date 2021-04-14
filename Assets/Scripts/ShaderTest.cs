using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderTest : MonoBehaviour
{
    public Vector2 pos;
    public Vector2 dir;
    public float distance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("linePos", pos);       
        Shader.SetGlobalVector("lineDir", dir);       
        Shader.SetGlobalFloat("distanceMult", distance);       
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        print("da");
        Graphics.Blit(source, destination);
    }
}
