using TMPro;
using UnityEngine;

public class VehicleSelectionController : MonoBehaviour
{
    public GameObject toRotate;
    public float rotateSpeed;
    public VehicleList listOfShips;
    public int shipPointer = 0;
    public TMP_Text shipName;
    public TMP_Text shipTyp;

    private void Awake()
    {
        shipPointer = PlayerPrefs.GetInt("pointer", 0);
        if(shipPointer >= listOfShips.shipList.Count )
            shipPointer = 0;

        SetShip();
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
            PlayerPrefs.SetInt("pointer", ++shipPointer);
            PlayerPrefs.Save();
            SetShip();
        }
    }
    
    public void leftArrowButton()
    {
        if (shipPointer > 0)
        {
            Destroy(GameObject.FindWithTag("Ship"));
            PlayerPrefs.SetInt("pointer", --shipPointer);
            PlayerPrefs.Save();
            SetShip();
        }
    }

    public void SetShip()
    {
        GameObject childObject = Instantiate(listOfShips.shipList[shipPointer], Vector3.zero, Quaternion.identity);
        childObject.transform.parent = toRotate.transform;
        ShipEntity shipEntity = childObject.GetComponent<ShipEntity>();
        shipName.text = shipEntity.shipName;
        shipTyp.text = shipEntity.shipTyp.ToString();
    }
}
