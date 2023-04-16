using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class awakeControlller : MonoBehaviour
{
    public GameObject toRotate;
    public float rotateSpeed;
    public vehicleList listOfShips;
    public int shipPointer = 0;

    private void Awake()
    {
        shipPointer = PlayerPrefs.GetInt("pointer");
        GameObject childObject = Instantiate(listOfShips.shipList[shipPointer], Vector3.zero, Quaternion.identity);
        childObject.transform.parent = toRotate.transform;
    }

    private void FixedUpdate()
    {
        //toRotate.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }


    public void rightArrowButton()
    {
        if (shipPointer < listOfShips.shipList.Count - 1)
        {
            Destroy(GameObject.FindWithTag("Ship"));
            shipPointer++;
            PlayerPrefs.SetInt("pointer",shipPointer);
            GameObject childObject = Instantiate(listOfShips.shipList[shipPointer], Vector3.zero, Quaternion.identity);
            childObject.transform.parent = toRotate.transform;
        }
    }
    
    public void leftArrowButton()
    {
        if (shipPointer > 0)
        {
            Destroy(GameObject.FindWithTag("Ship"));
            shipPointer--;
            PlayerPrefs.SetInt("pointer",shipPointer);
            GameObject childObject = Instantiate(listOfShips.shipList[shipPointer], Vector3.zero, Quaternion.identity);
            childObject.transform.parent = toRotate.transform;
        }
    }
}
