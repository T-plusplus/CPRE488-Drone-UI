using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    private Canvas screen;
    private Button startB;
    // Start is called before the first frame update
    void Start()
    {
        screen=Instantiate(Resources.Load<Canvas>("Orientation/Screen"));
        startB=screen.GetComponentInChildren<Button>();
        startB.onClick.AddListener(StartClick);
    }

    // Update is called once per frame
    void Update()
    {
        //Don't know yet might do something fancy
    }
    void StartClick()
    {
        SceneManager.LoadScene("Orientation", LoadSceneMode.Single);
    }
}
