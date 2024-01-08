using System.Collections;
using System.Collections.Generic;
using KetosGames.SceneTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNewScene : MonoBehaviour
{
    public string SceneName;

    public void LoadScene()
    {
        SceneLoader.LoadScene(SceneName);
    }
}
