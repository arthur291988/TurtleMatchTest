
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance;

    public List<RectTransform> resourcesTransform;
    public List<ParticleSystem> resourcesPullParticles;
    private List<Transform> resourcesPullParticlesTransform;

    private List<Vector2> worldPositionsOfResourcesUI;

    private int [] resourcesCount; //0-food, 1-money, 2- water, 3-eco, 4-energy

    public List<Text> resourceCountTxt;

    private int counter;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        worldPositionsOfResourcesUI = new List<Vector2>() {
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero

        };
        resourcesPullParticlesTransform = new List<Transform>();

        for (int i = 0; i < resourcesPullParticles.Count; i++) {
            resourcesPullParticlesTransform.Add(resourcesPullParticles[i].transform);
        }

        resourcesCount = new int[5];
        setStartResources();
        for (int i = 0; i < resourceCountTxt.Count; i++)
        {
            resourceCountTxt[i].text = resourcesCount[i].ToString();
        }
        setResourcesUIWorldPositions();
    }

    private void setResourcesUIWorldPositions() {
        for (int i = 0; i < resourcesTransform.Count; i++)
        {
            worldPositionsOfResourcesUI[i] = (Vector2)CommonData.Instance._camera.ScreenToWorldPoint(resourcesTransform[i].position);
            resourcesPullParticlesTransform[i].position = worldPositionsOfResourcesUI[i];
        }
    }

    public void setStartResources()
    {
        resourcesCount[0] = 150;
        resourcesCount[1] = 180;
        resourcesCount[2] = 150;
        resourcesCount[3] = 170;
        resourcesCount[4] = 120;
    }

    //0-food, 1-money, 2- water, 3-eco, 4-energy
    public void setResoures(int index, int addValue) {
        resourcesCount[index] += addValue;
        resourcesPullParticles[index].Play();
    }

    public IEnumerator resourcesCounter(int addStep, int index) {
        counter = addStep;
        yield return new WaitForSeconds(0.1f);
        setResoures(index, 1);
        updateResourcesTxt(index);
        counter--;
        if (counter > 0) StartCoroutine(resourcesCounter(counter, index));
    }

    public int getResourceValue(int index) {
        return resourcesCount[index];
    }

    public void updateResourcesTxt(int index) {
        //for (int i = 0; i < resourceCountTxt.Count; i++) {
        //    resourceCountTxt[i].text = resourcesCount[i].ToString();
        //}
        resourceCountTxt[index].text = resourcesCount[index].ToString();
    }


}
