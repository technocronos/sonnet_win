/// <summary>
/// write by 52cwalk,if you have some question ,please contract lycwalk@gmail.com
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class DeviceCameraController : MonoBehaviour {

	public enum CameraMode
	{
		FACE_C,
		DEFAULT_C,
		NONE
	}

	[HideInInspector]
	public WebCamTexture cameraTexture;
    
    private bool isPlay = false;
	GameObject e_CameraPlaneObj;
	bool isCorrected = false;
	float screenVideoRatio = 1.0f;
    
	public bool isPlaying
	{
		get{
			return isPlay;
		}
	}

	// Use this for initialization  
	async void Awake()  
	{

#if UNITY_ANDROID
        int granted = await CameraSetting.RequestCameraPermissions();
        if (granted <= 0)
        {
            return;
        }
#endif
        e_CameraPlaneObj = transform.Find("CameraPlane").gameObject;
        StartWork();
        
    }
    
	// Update is called once per frame  
	void Update()  
	{  
		if (isPlay) {  
			if(e_CameraPlaneObj.activeSelf)
			{
				e_CameraPlaneObj.GetComponent<Renderer>().material.mainTexture = cameraTexture;
			}
		}


		if (cameraTexture != null && cameraTexture.isPlaying) {
			if (cameraTexture.width > 200 && !isCorrected) {
				correctScreenRatio();
			}
		}
	}

    public void StartWork()
    {
        StartCoroutine(initCamera());
    }

    IEnumerator initCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            string DeviceName = devices[0].name;
            cameraTexture = new WebCamTexture(DeviceName, 640, 480, 60);

            cameraTexture.Play();
            isPlay = true;
        }
    }
    
    public void StopWork()
    {
        isPlay = false;
        if (this.cameraTexture != null && this.cameraTexture.isPlaying)
        {
            this.cameraTexture.Stop();
            Destroy(this.cameraTexture);
            this.cameraTexture = null;
        }
    }

    /// <summary>
    /// Corrects the screen ratio.
    /// </summary>
    void correctScreenRatio()
	{
		int videoWidth = 640;
		int videoHeight = 480;
		int ScreenWidth = 640;
		int ScreenHeight = 480;

		float videoRatio = 1;
		float screenRatio = 1;

		if (cameraTexture != null) {
			videoWidth = cameraTexture.width;
			videoHeight = cameraTexture.height;
		}
		videoRatio = videoWidth * 1.0f / videoHeight;
		ScreenWidth = Mathf.Max (Screen.width, Screen.height);
		ScreenHeight = Mathf.Min (Screen.width, Screen.height);
		screenRatio = ScreenWidth * 1.0f / ScreenHeight;

		screenVideoRatio = screenRatio / videoRatio;
		isCorrected = true;

		if (e_CameraPlaneObj != null) {
			e_CameraPlaneObj.GetComponent<CameraPlaneController>().correctPlaneScale(screenVideoRatio);
		}
	}

	public float getScreenVideoRatio()
	{
		return screenVideoRatio;
	}
    

}


