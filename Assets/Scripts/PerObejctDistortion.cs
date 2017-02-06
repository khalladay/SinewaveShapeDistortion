using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PerObejctDistortion : MonoBehaviour
{
    private Camera cam;
    private Camera maskCam;

    public Material compositeMat;
    public Material stripAlphaMat;

    public float speed = 1.0f;
    public float scaleFactor = 1.0f;
    public float magnitude = 0.01f;

    private int scaledWidth;
    private int scaledHeight;

		void Start ()
    {

        cam                     = GetComponent<Camera>();
        scaledWidth             = (int)(Screen.width * scaleFactor);
        scaledHeight            = (int)(Screen.height * scaleFactor);

        cam.cullingMask         = ~(1 << LayerMask.NameToLayer("Distortion"));
        cam.depthTextureMode    = DepthTextureMode.Depth;

        maskCam                 = new GameObject("Distort Mask Cam").AddComponent<Camera>();
        maskCam.enabled         = false;
        maskCam.clearFlags      = CameraClearFlags.Nothing;
	}

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        RenderTexture distortingRT = RenderTexture.GetTemporary(scaledWidth, scaledHeight, 24, RenderTextureFormat.ARGBFloat);
        Graphics.Blit(src, distortingRT, stripAlphaMat);

        maskCam.CopyFrom(cam);
        maskCam.gameObject.transform.position = transform.position;
        maskCam.gameObject.transform.rotation = transform.rotation;

        //draw the distorting objects into the buffer
        maskCam.clearFlags = CameraClearFlags.Depth;
        maskCam.cullingMask = 1 << LayerMask.NameToLayer("Distortion");
        maskCam.SetTargetBuffers(distortingRT.colorBuffer, distortingRT.depthBuffer);
        maskCam.Render();

        //Composite pass
        compositeMat.SetTexture("_DistortionRT", distortingRT);
        Graphics.Blit(src, dst, compositeMat);

        RenderTexture.ReleaseTemporary(distortingRT);

    }

    void Update ()
    {
        scaleFactor = Mathf.Clamp(scaleFactor, 0.01f, 1.0f);
        scaledWidth = (int)(Screen.width * scaleFactor);
        scaledHeight = (int)(Screen.height * scaleFactor);

        magnitude = Mathf.Max(0.0f, magnitude);
        Shader.SetGlobalFloat("_DistortionOffset", -Time.time * speed);
        Shader.SetGlobalFloat("_DistortionAmount", magnitude/100.0f);

    }
}
