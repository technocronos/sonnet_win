using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvironmentBehaviour : MonoBehaviour
{
    public GameObject LeftSoptLight;
    public GameObject RightSoptLight;
    public GameObject PointLight_L;
    public GameObject PointLight_S;
    public GameObject LightRays;
    public GameObject FieldParticle;
    public GameObject RainParticle;
    public GameObject SnowParticle;


    public static EnvironmentBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static EnvironmentBehaviour instance;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        LeftSoptLight.SetActive(true);
        RightSoptLight.SetActive(false);
        PointLight_L.SetActive(true);
        PointLight_S.SetActive(false);
        LightRays.SetActive(false);
        FieldParticle.SetActive(false);
        RainParticle.SetActive(false);
        SnowParticle.SetActive(false);
    }

    public void setEnv(string env)
    {
        switch (env)
        {
            case "cave":
                LeftSoptLight.SetActive(false);
                RightSoptLight.SetActive(false);
                PointLight_L.SetActive(false);
                PointLight_S.SetActive(true);
                LightRays.SetActive(false);
                FieldParticle.SetActive(false);
                RainParticle.SetActive(false);
                SnowParticle.SetActive(false);
                break;
            case "grass":
                LeftSoptLight.SetActive(true);
                RightSoptLight.SetActive(false);
                PointLight_L.SetActive(true);
                PointLight_S.SetActive(false);
                LightRays.SetActive(true);
                FieldParticle.SetActive(true);
                RainParticle.SetActive(false);
                SnowParticle.SetActive(false);
                break;
            case "rain":
                LeftSoptLight.SetActive(false);
                RightSoptLight.SetActive(false);
                PointLight_L.SetActive(true);
                PointLight_S.SetActive(false);
                LightRays.SetActive(false);
                FieldParticle.SetActive(false);
                RainParticle.SetActive(true);
                SnowParticle.SetActive(false);
                break;
            case "snow":
                LeftSoptLight.SetActive(true);
                RightSoptLight.SetActive(false);
                PointLight_L.SetActive(true);
                PointLight_S.SetActive(false);
                LightRays.SetActive(false);
                FieldParticle.SetActive(false);
                RainParticle.SetActive(false);
                SnowParticle.SetActive(true);
                break;
        }
    }

    public void changeColor(string colorString)
    {
        Color newColor;
        if (ColorUtility.TryParseHtmlString("#" + colorString, out newColor))
        {
            PointLight_L.GetComponent<UnityEngine.Rendering.Universal.Light2D>().color = newColor;
            PointLight_S.GetComponent<UnityEngine.Rendering.Universal.Light2D>().color = newColor;
        }
    }

}
