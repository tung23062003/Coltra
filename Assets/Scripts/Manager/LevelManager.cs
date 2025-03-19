using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] LevelDataSO levelData;
    [SerializeField] private DesignDataSO designData;
    [SerializeField] private ObjectiveUI objectiveUI;
    [SerializeField] private Transform objectivePrefab;

    private List<EnemyDataInstance> currentEnemyDataClone;
    private List<EnemyData> currentEnemyData;
    private Dictionary<CharacterColor, Transform> objectiveInstances = new();

    private int currentLevel;

    [SerializeField] private GameObject monsterObj;
    [SerializeField] private GameObject bulletObj;
    [SerializeField] private GameObject bossImgObj;

    public bool isWin;

    protected override void Awake()
    {
        GameEvent.OnEnemyKill += EnemyKilled;
    }

    private void OnDestroy()
    {
        GameEvent.OnEnemyKill -= EnemyKilled;
    }

    private void Start()
    {
        currentLevel = SceneController.Instance.GetCurrentSceneIndex();
        if (currentLevel > GameManager.instance.GetLevel()) GameManager.instance.SetLevel(currentLevel);
        IngameController.Instance.SetLevelText(currentLevel);
        currentEnemyData = levelData.GetEnemyDataByLevel(currentLevel);

        currentEnemyDataClone = new List<EnemyDataInstance>();
        foreach (var data in currentEnemyData)
        {
            currentEnemyDataClone.Add(new EnemyDataInstance(data.characterColor, data.quantity));
        }

        UpdateObjectiveUI();
    }

    public void EnemyKilled(CharacterColor color, bool isBoss)
    {
        if (isBoss)
        {
            GameEvent.OnBossKilled?.Invoke();
            return;
        }
        EnemyDataInstance enemyData = currentEnemyDataClone.Find(data => data.characterColor == color);
        if (enemyData != null)
        {
            enemyData.killed++;

            UpdateObjectiveUI();
            CheckMissionProgress();
        }
    }

    public void UpdateProgress(Dictionary<CharacterColor, string> progressData)
    {
        if(currentLevel == 6)
        {
            bossImgObj.SetActive(true);
            return;
        }
        foreach (KeyValuePair<CharacterColor, string> item in progressData)
        {
            if (!objectiveInstances.ContainsKey(item.Key))
            {
                Transform obj = Instantiate(objectivePrefab, objectiveUI.transform);
                obj.GetChild(0).GetComponent<Image>().color = ColorData.GetColor(item.Key);
                objectiveInstances[item.Key] = obj;
            }

            objectiveInstances[item.Key].GetChild(1).GetComponent<TextMeshProUGUI>().text = item.Value;
        }
    }

    public void UpdateObjectiveUI()
    {
        var progressData = new Dictionary<CharacterColor, string>();

        foreach (var enemyData in currentEnemyDataClone)
        {
            string progress = $"{enemyData.killed}/{enemyData.quantity}";
            progressData.Add(enemyData.characterColor, progress);
        }

        UpdateProgress(progressData);
    }



    private void CheckMissionProgress()
    {
        foreach (var enemyData in currentEnemyDataClone)
        {
            if (enemyData.killed < enemyData.quantity)
            {
                return;
            }
        }

        CompleteLevel();
    }

    private void CompleteLevel()
    {
        Debug.Log("Objective Completed!");
        GameEvent.OnCompleteObjective?.Invoke();
        //SceneController.Instance.NextLevel();
        Destroy(monsterObj);
        //Destroy(bulletObj);
    }

}

public class EnemyDataInstance
{
    public CharacterColor characterColor;
    public int killed;
    public int quantity;

    public EnemyDataInstance(CharacterColor color, int quantity)
    {
        this.characterColor = color;
        this.quantity = quantity;
        this.killed = 0;
    }
}