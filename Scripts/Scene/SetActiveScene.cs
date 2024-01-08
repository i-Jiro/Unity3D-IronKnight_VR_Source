using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetActiveScene : MonoBehaviour
{
    public string activeScene;

    void Awake()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(activeScene));
    }
}
