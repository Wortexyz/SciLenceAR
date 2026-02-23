using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }
    public void ARTestnext()
    {
        SceneManager.LoadScene("ARScene");
    }
    public void ARGravityScene()
    {
        SceneManager.LoadScene(2);
    }
    public void HomeScene()
    {
        SceneManager.LoadScene(0);
    }
}
