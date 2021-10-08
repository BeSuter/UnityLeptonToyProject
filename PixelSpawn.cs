using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelSpawn : MonoBehaviour
{
    public float fixedZ = -7.8f;
    public float updateTime = 15.0f;
    private List<Tuple<int, int, float>> newFrame;
    private Tuple<int, int, float> pixelInfo;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("PixelUpdate", 0, updateTime);
    }

    void PixelUpdate() 
    {
        newFrame = TCP_Client.current.GetFrame();

        for (int i = 0; i < newFrame.Count; i++) 
        {
            pixelInfo = newFrame[i];
            GameObject obj = ObjectPooler.current.GetPooledObject();

            if (obj == null) return;

            Color newColor = new Color(0.7f, 0.0f, (pixelInfo.Item3 - 25.4f) / 10.6f, 0.6f);
            obj.transform.position = new Vector3((pixelInfo.Item1 - 60.0f) / 15.0f, (pixelInfo.Item2 - 80.0f) / 20.0f, fixedZ);
            obj.GetComponent<Renderer>().material.SetColor("_Color", newColor);
            obj.SetActive(true);
        }
    }
}
