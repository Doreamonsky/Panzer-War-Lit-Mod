using UnityEngine;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using System.IO;
using ShanghaiWindy.Core;

public class RenderVehicle : MonoBehaviour
{
    [Header("You can drag models to here and renderer them.")]
    public GameObject[] VehiclePrefabs;

    private delegate void LoadVehicle(string vehicleName);

    void Start()
    {
        StartCoroutine(ScreenShot());
    }


    private IEnumerator ScreenShot()
    {
        yield return new WaitForSeconds(2);

        foreach (var vehiclePrefab in VehiclePrefabs)
        {
            var instance = Instantiate(vehiclePrefab);

            yield return new WaitForSeconds(1);

            Rect lRect = new Rect(0f, 0f, Screen.width, Screen.height);

            yield return new WaitForEndOfFrame();

            Texture2D capturedImage = zzTransparencyCapture.capture(lRect);

            byte[] byt = capturedImage.EncodeToPNG();

            File.WriteAllBytes("Others/Renderering/Common/" + vehiclePrefab.name + ".png", byt);

            Debug.Log("Save at Others/Renderering/Common/" + vehiclePrefab.name + ".png");

            yield return new WaitForEndOfFrame();

            Destroy(instance);

            yield return new WaitForSeconds(1);
        }
    }

}
#endif