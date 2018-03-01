using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public struct StateEstimate
{
    public float xAtt;
    public float yAtt;
    public float zAtt;
    public float xPos;
    public float yPos;
    public float zPos;
    public bool isValid;
}

public class ProcessCameraImages : MonoBehaviour {

    public UIUpdater uiUpdater;

    public RenderTexture rt;
    public float timeBetweenCaptures;

    public string groundProcessingServerUrl;

    private int imageIdx = 0;

	void Start()
    {
        StartCoroutine(PeriodicallyCaptureImagesAndSendToGround());
	}

    private StateEstimate UnpackStateEstimate(byte[] bytes)
    {
        StateEstimate se = new StateEstimate();

        if (bytes != null)
        {
            Stream s = new MemoryStream(bytes);
            BinaryReader br = new BinaryReader(s);
            

            if (br.BaseStream.Length == sizeof(float) * 6)
            {
                se.xAtt = br.ReadSingle();
                se.yAtt = br.ReadSingle();
                se.zAtt = br.ReadSingle();
                se.xPos = br.ReadSingle();
                se.yPos = br.ReadSingle();
                se.zPos = br.ReadSingle();
                se.isValid = true;
            }
            else
            {
                Debug.LogError("Error: wrong byte count (" + br.BaseStream.Length + ")");
                se.isValid = false;
            }
        }
        else
        {
            Debug.LogError("Error: invalid byte array!");
            se.isValid = false;
        }

        return se;
    }

    private IEnumerator PeriodicallyCaptureImagesAndSendToGround()
    {
        while (true)
        {
            Texture2D tx = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);

            RenderTexture.active = rt;
            tx.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

            yield return null;

            tx.Apply();

            yield return null;

            RenderTexture.active = null;

            byte[] imageBytes = tx.EncodeToJPG();

            yield return null;

            UnityWebRequest www = UnityWebRequest.Put(groundProcessingServerUrl, imageBytes);

            www.timeout = 5;

            yield return www.Send();

            if (www.isNetworkError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Upload complete!");

                byte[] receivedData = www.downloadHandler.data;
                Debug.Log("Received response of length " + receivedData.Length + " bytes");

                StateEstimate se = UnpackStateEstimate(receivedData);

                if (se.isValid)
                {
                    Debug.Log("Valid packet!");

                    uiUpdater.UpdateTextFromStateEstimate(se);
                }
                else
                {
                    Debug.LogError("Invalid packet!");
                }
            }

            imageIdx++;

            yield return new WaitForSecondsRealtime(timeBetweenCaptures);
        }
    }
}
