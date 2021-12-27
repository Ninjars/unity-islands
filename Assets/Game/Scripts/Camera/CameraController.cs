using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
    public Camera playerCam;
    public bool enableEdgeScrolling;
    public float cameraXmax;
    public float cameraXmin;
    public float cameraZmax;
    public float cameraZmin;

    public static bool canUseMouseWheel=true;

    //vertical limits
    public float cameraYmin=5;
    public float cameraYmax=30;

    float cameraSpeed=5;
    float mouseWheelSpeed=5000;

    //tilt limits
    Vector3 camEulers;
    float camRotMin = 30;
    float camRotMax = 70;

    //pixel witdth for edge scrolling
    int edgeScrollBounds = 5;

    float moveSpeed = 50;

    //rename vars
    float mousePosX;
    float mousePosY;

    void Start () {
        if (playerCam==null)
            playerCam=Camera.main;

        camEulers = transform.localEulerAngles;
    }

	void Update () {

        Vector3 newPos = transform.position;

        //check for edge scrolling and move camera controller
        mousePosX = Input.mousePosition.x;
		mousePosY = Input.mousePosition.y;

		if ((enableEdgeScrolling && mousePosX < edgeScrollBounds) || Input.GetAxis ("Scroll Horizontal")<0)
			newPos += (Vector3.right * ProportionalScrollSpeed(-moveSpeed) * Time.unscaledDeltaTime * SettingsManager.instance.scrollSpeed);        
		else if ((enableEdgeScrolling && mousePosX > Screen.width - edgeScrollBounds) || Input.GetAxis ("Scroll Horizontal")>0)
			newPos += (Vector3.right * ProportionalScrollSpeed(moveSpeed) * Time.unscaledDeltaTime * SettingsManager.instance.scrollSpeed);

		if ((enableEdgeScrolling && mousePosY < edgeScrollBounds) || Input.GetAxis ("Scroll Vertical")<0)
			newPos += (Vector3.forward * ProportionalScrollSpeed(-moveSpeed) * Time.unscaledDeltaTime * SettingsManager.instance.scrollSpeed);
		else if ((enableEdgeScrolling && mousePosY > Screen.height - edgeScrollBounds) || Input.GetAxis ("Scroll Vertical")>0)
			newPos += (Vector3.forward * ProportionalScrollSpeed(moveSpeed) * Time.unscaledDeltaTime * SettingsManager.instance.scrollSpeed);


		if (Input.GetAxis ("Zoom")!=0 && canUseMouseWheel)
			newPos += (Vector3.up * -(Input.GetAxis ("Zoom")* mouseWheelSpeed * Time.unscaledDeltaTime));

		//clamp values
		newPos.x = Mathf.Clamp (newPos.x, cameraXmin, cameraXmax);
		newPos.z = Mathf.Clamp (newPos.z, cameraZmin, cameraZmax);
		newPos.y = Mathf.Clamp (newPos.y, cameraYmin, cameraYmax);

		transform.position = newPos;

		//camera rotation
		float camHeightPercent = (transform.position.y - cameraYmin)/(cameraYmax - cameraYmin);
		camEulers.x = Mathf.Lerp (camRotMin, camRotMax, camHeightPercent);
		transform.localEulerAngles = camEulers;

	}

    void LateUpdate() {       
        //lerp camera position to camera controller
        playerCam.transform.position=Vector3.Lerp (playerCam.transform.position, transform.position, Time.unscaledDeltaTime * cameraSpeed);
        playerCam.transform.rotation = Quaternion.Lerp (playerCam.transform.rotation, transform.rotation, Time.unscaledDeltaTime * cameraSpeed);
    }   

    float ProportionalScrollSpeed (float speed){
        return speed  *(transform.position.y/cameraYmax);
    }
}