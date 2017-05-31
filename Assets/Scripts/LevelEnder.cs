using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class LevelEnder : MonoBehaviour {

    public GameObject setActiveOnTrigger;

    void OnTriggerEnter(Collider other)
    {
        if(!ScaleManager.inNormalSize)
        {
            if (other.tag == "Player")
            {
                Invoke("LoadNextLevel", 3);
                Debug.Log("finished level");
                
                if (SceneManager.GetActiveScene().buildIndex+1 < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
                else
                {
                    setActiveOnTrigger.SetActive(true);
                }
            }
        }else
        {
            Debug.Log("not finished level");
        }
    }

    void LoadNextLeve()
    {

    }
}
