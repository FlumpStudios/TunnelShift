using UnityEngine;
using KartGame.KartSystems;

public class LevelManager : MonoBehaviour
{
    private static int stageChangeOnLevel = 2;

    private static int currentStage = 0;    
    private static int currentLevel = 0;
    public static int levelTick = 10;
    public Camera cam = null;
    public float initialFov = 63f;
    public float minFov = 50f;    
    public const float SpeedIncreaseOnLevelUp = 50;
    public const float SteerIncreaseOnLevelUp = 5f;


    private static int levelChangesSinceLastStageChange = 0;

    public static void IncreaseLevel()
    {
        currentLevel++;        
        levelChangesSinceLastStageChange++;
        if (levelChangesSinceLastStageChange >= stageChangeOnLevel)
        {
            currentStage++;
            levelChangesSinceLastStageChange = 0;
        }

        ArcadeKart.baseSteer += SteerIncreaseOnLevelUp;
        ArcadeKart.baseTopSpeed += SpeedIncreaseOnLevelUp;
       
    }
    
    
    public static int GetCurrentLevel()
    {
        return currentLevel;
    }

    public static int GetCurrentStage()
    {
        return currentStage;
    }

    // Start is called before the first frame update
    void Start()
    {
        ArcadeKart.baseSteer = 40;
        ArcadeKart.baseTopSpeed = 50;
        cam.fieldOfView = initialFov;
        currentLevel = 0;
        levelTick = 0;
    }


    public float transitionSpeed = 4f;
    void Update()
    {
        levelTick += (int)Time.deltaTime * 60;

        if (cam.fieldOfView > minFov)
        { 
            float targetFOV = initialFov - currentLevel;        
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 1.2f);
        }
    }
}
