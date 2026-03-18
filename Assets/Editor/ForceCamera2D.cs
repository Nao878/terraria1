using UnityEngine;
using UnityEditor;

public class ForceCamera2D
{
    [MenuItem("Setup/Force Camera 2D")]
    public static void Run()
    {
        Debug.Log("--- Forcing Camera to 2D ---");

        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.transform.rotation = Quaternion.identity;
            mainCam.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, -10f);
            Debug.Log("Main Camera fixed.");
        }
        
        var vcamObj = GameObject.Find("PlayerVCam");
        if (vcamObj != null)
        {
            vcamObj.transform.rotation = Quaternion.identity;
            vcamObj.transform.position = new Vector3(vcamObj.transform.position.x, vcamObj.transform.position.y, -10f);
            
            // Try to set via Reflection for Cinemachine 3
            var camComponent = vcamObj.GetComponent("Unity.Cinemachine.CinemachineCamera") as MonoBehaviour;
            if (camComponent != null)
            {
                var type = camComponent.GetType();
                var lensProp = type.GetProperty("Lens");
                if (lensProp != null)
                {
                    var lens = lensProp.GetValue(camComponent);
                    var lensType = lens.GetType();
                    var orthoField = lensType.GetField("Orthographic");
                    if (orthoField != null)
                    {
                        orthoField.SetValue(lens, true);
                        lensProp.SetValue(camComponent, lens);
                        Debug.Log("Cinemachine Lens Orthographic set to true.");
                    }
                }
            }

            var composer = vcamObj.GetComponent("Unity.Cinemachine.CinemachinePositionComposer") as MonoBehaviour;
            if (composer != null)
            {
                var composerType = composer.GetType();
                var distSetting = composerType.GetField("CameraDistance");
                if (distSetting != null)
                {
                    distSetting.SetValue(composer, 10f);
                    Debug.Log("PositionComposer CameraDistance set to 10.");
                }
            }
            
            Debug.Log("PlayerVCam fixed.");
        }
    }
}