using System.Collections;
using UnityEngine;

public class FlickeringLights : MonoBehaviour
{
    public bool isflickering = false;
    public float flickerDelay;
    Material thisLampMaterial;
    // also change the emission colour as it flickers
    [Header("Emission")]
    public Renderer tubeRenderer;

    // default emission color
    public Color emissionOn = Color.white;
    public float emissionOff = 0.05f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // grab reference
        thisLampMaterial = tubeRenderer.material;
        thisLampMaterial.EnableKeyword("_EMISSION");   
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isflickering == false)
        {
            StartCoroutine(RandomFlicker());
        }
        
    }

    IEnumerator RandomFlicker()
    {

        isflickering = true;

        // flicker light on
        this.gameObject.GetComponent<Light>().enabled = true;
        thisLampMaterial.SetColor("_EmissionColor", emissionOn);
        flickerDelay = Random.Range(0.01f, 0.5f);
        yield return new WaitForSeconds(flickerDelay);

        // flicker light off
        this.gameObject.GetComponent<Light>().enabled = false;
        // material.SetColor(“_EmissionColor”, color * intensity);
        thisLampMaterial.SetColor("_EmissionColor", emissionOn * emissionOff);
        flickerDelay = Random.Range(0.01f, 0.8f);
        yield return new WaitForSeconds(flickerDelay);

        isflickering = false;

    }
}
