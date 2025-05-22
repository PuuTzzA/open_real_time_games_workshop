using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject x;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            x.SetActive(true);
        }
    }



    public void Jab()
    {
        Debug.Log("JabJab");
    }
}
