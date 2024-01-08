using System.Collections;
using System.Collections.Generic;
using KetosGames.SceneTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LaunchSceneOnStart : MonoBehaviour
{
    public string SceneName = "";
    // Start is called before the first frame update
    void Start()
    {
        SceneLoader.LoadScene(SceneName);
    }
}


