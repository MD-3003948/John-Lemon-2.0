using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager sGameManager; // singleton - one GM to control entire game
    public bool tutorialComplete = false;

    [SerializeField]
    private GameObject john;
    [SerializeField]
    private Gun gun;

    public SceneSwitcher sceneSwitcher;

    // this is information that needs to be saved and reloaded between scene changes
    [Header("Saved Data")]
    public int savedLoadedAmmo;
    public int savedTotalAmmo;
    public Vector3 savedPosition;

    void Awake()
    {
        if (sGameManager)
        {
            Destroy(this);
        }
        else
        {
            sGameManager = this;
        }
        DontDestroyOnLoad(this); // allows GameManager singleton's data to be saved between scenes

        // subscribe to sceneLoaded event for load order purposes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneManager.GetActiveScene().name)
        {
            Debug.LogError("Loaded scene does not match active scene.");
        }

        sceneSwitcher = null; // clear any old sceneswitchers

        john = GameObject.FindGameObjectWithTag("Player");
        gun = john.GetComponentInChildren<Gun>();
        sceneSwitcher = GameObject.FindGameObjectWithTag("SceneSwitcher")?.GetComponent<SceneSwitcher>();

        if (sceneSwitcher != null)
        {
            sceneSwitcher.currentScene = scene.name;
            // in the tutorial scene, we change scenes (back to the first scene)
            // when john has shot all the targets
            if (scene.name == "TargetPractice2")
            {
                sceneSwitcher.numTargets = 5;
                sceneSwitcher.newScene = "SampleScene";

                gun.totalAmmo = 1000;
                gun.currentLoaded = 0; // we want to explicitly force the player to reload

                // this is used to turn off the tutorial trigger later! 
                tutorialComplete = true;

                john.transform.position = new Vector3(0, 0, 0);
            }
        }
        else
        {
            print("Sceneswitcher not found in scene: " + scene.name);
        }

        // for the first two scene initializations, keep john where he is
        // for the last one, put him where he was right before transition to tutorial scene.
        // also restore his ammo from that point 
        if (tutorialComplete && scene.name != "TargetPractice2")
        {
            gun.totalAmmo = savedTotalAmmo;
            gun.currentLoaded = savedLoadedAmmo;

            john.transform.position = savedPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RemoveTarget()
    {
        sceneSwitcher.numTargets--;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}