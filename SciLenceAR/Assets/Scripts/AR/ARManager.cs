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
    public void ARStateOfMatterScene()
    {
        SceneManager.LoadScene(3);
    }
    public void ARPhotosynthesisScene()
    {
        SceneManager.LoadScene(4);
    }
    public void HomeScene()
    {
        SceneManager.LoadScene(0);
    }
    public void NewTonsLAW()
    {
        SceneManager.LoadScene(5);
    }
    public void CircuitScene()
    {
        SceneManager.LoadScene(6);
    }
    public void BoylesScene()
    {
        SceneManager.LoadScene(7);
    }
    public void PlantCell()
    {
        SceneManager.LoadScene(8);
    }
    public void AtomicScene()
    {
        SceneManager.LoadScene(9);
    }
}
