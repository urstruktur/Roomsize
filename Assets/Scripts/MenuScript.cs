using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

	public void LoadLevel(int index)
    {
        Debug.Log("Loading level " + index);
        SceneManager.LoadScene(index);
    }
}
