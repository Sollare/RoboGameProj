// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class LaserScope : MonoBehaviour
{
    private float scrollSpeed = 0.5f;
    private float pulseSpeed = 1.5f;

    private float noiseSize = 1.0f;

    public float maxWidth = 0.5f;
    public float minWidth = 0.2f;

    public float maxDistance = 10f;

    public Transform laser = null;
    public Transform pointer = null;
    public Transform laserSight = null;

    private LineRenderer lRenderer;
    private float aniTime = 0.0f;
    private float aniDir = 1.0f;

    private Renderer _renderer;

    protected Renderer cachedRenderer
    {
        get
        {
            if (_renderer) return renderer;
            else return (_renderer = renderer);
        }
    }


    private void Start()
    {
        lRenderer = laser.GetComponent<LineRenderer>();
        aniTime = 0f;

        // Change some animation values here and there
        StartCoroutine("ChoseNewAnimationTargetCoroutine");
    }

    IEnumerator ChoseNewAnimationTargetCoroutine()
    {
        while (true)
        {
            aniDir = aniDir*0.9f + Random.Range(0.5f, 1.5f)*0.1f;
            yield return 0;
            minWidth = minWidth*0.8f + Random.Range(0.1f, 1.0f)*0.2f;
            yield return new WaitForSeconds(1.0f + Random.value*2.0f - 1.0f);
        }
    }

    private void Update()
    {
        laser.renderer.material.mainTextureOffset += Vector2.right * Time.deltaTime * aniDir * scrollSpeed;
        laser.renderer.material.SetTextureOffset("_NoiseTex", new Vector2(-Time.time * aniDir * scrollSpeed, 0.0f));

        float aniFactor = Mathf.PingPong(Time.time*pulseSpeed, 1.0f);
        aniFactor = Mathf.Max(minWidth, aniFactor)*maxWidth;
        lRenderer.SetWidth(aniFactor, aniFactor);

        lRenderer.SetPosition(0, laserSight.position);

        RaycastHit hit;
        if (Physics.Raycast(laserSight.position, laserSight.forward, out hit, maxDistance))
        {
            lRenderer.SetPosition(1, (laserSight.transform.position + hit.distance * laserSight.forward));

            laser.renderer.material.mainTextureScale = new Vector2( 0.1f * (hit.distance), laser.renderer.material.mainTextureScale.y);
            laser.renderer.material.SetTextureScale("_NoiseTex", new Vector2(0.1f * hit.distance * noiseSize, noiseSize));

            // Use point and normal to align a nice & rough hit plane
            if (pointer)
            {
                pointer.renderer.enabled = true;
                pointer.position = hit.point + (laserSight.position - hit.point) * 0.01f;
                pointer.rotation = Quaternion.LookRotation(hit.normal, transform.up);
                pointer.eulerAngles = new Vector3(90f, pointer.eulerAngles.y, pointer.eulerAngles.z);
            }
        }
        else
        {
            if (pointer)
                pointer.renderer.enabled = false;

            float maxDist = maxDistance;
            lRenderer.SetPosition(1, (laserSight.transform.position + maxDist * laserSight.forward));
            laser.renderer.material.mainTextureScale = new Vector2(0.1f * (maxDist), laser.renderer.material.mainTextureScale.y);
            laser.renderer.material.SetTextureScale("_NoiseTex", new Vector2(0.1f * (maxDist) * noiseSize, noiseSize));
        }

    }

}