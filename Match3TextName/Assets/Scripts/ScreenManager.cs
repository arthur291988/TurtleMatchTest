using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    //private float vertScreenSize;
    //private float horisScreenSize;

    private float baseScreenHieght;
    private float baseScreenWidth;
    private float baseYIndex;
    private float baseXIndex;

    private float baseY;
    private float baseSize;
    private float screenRatio;

    // Start is called before the first frame update
    void Start()
    {
        //vertScreenSize = CommonData.Instance._camera.orthographicSize * 2;
        //horisScreenSize = vertScreenSize * Screen.width / Screen.height;
        baseScreenHieght = 1280;
        baseScreenWidth = 720;
        baseXIndex = (float)baseScreenWidth / (float)Screen.width;
        baseY = 1.406f;
        baseSize = 7.8749f;
        screenRatio = (float)Screen.height / (float)Screen.width;
        baseYIndex = ((float)Screen.height * screenRatio) / (float)baseScreenHieght;

        //Debug.Log(screenRatio);


        //CommonData.Instance._camera.transform.position = new Vector3(0, baseY * screenRatio, -10);
        CommonData.Instance._camera.orthographicSize = baseSize * screenRatio;
    }

}
