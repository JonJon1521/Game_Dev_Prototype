using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class gamemanager : MonoBehaviour
{
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] int gameGoal;
    [SerializeField] TMP_Text gameGoalCountText;
    [SerializeField] TMP_Text ammoCurrentText;
    [SerializeField] TMP_Text ammoMaxText;
    [SerializeField] TextMeshProUGUI spell1Text;
    [SerializeField] TextMeshProUGUI spell2Text;

    public Image playerHPBar;
    public GameObject player;
    public playerController playerScript;
    public GameObject playerSpawnPos;
    public GameObject checkpointPopup;
    public GameObject damagePlayerFlash;
    public bool isPaused;
    bool goalUnlocked = false;

    private float timeScaleOriginal;

    private int gameGoalCount;

    public static gamemanager instance;

    int requiredCollectibles;
    int collectedCount;
   
    void Awake()
    {
        instance = this;

        timeScaleOriginal = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();

        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
        gameGoalCount =
        Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;

        gameGoalCountText.text = gameGoalCount.ToString("F0");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuActive != menuPause)
            {
                statePaused();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else
            {
                stateUnpaused();
            }
        }

    }

    public void statePaused()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpaused()
    {
        isPaused = false;
        Time.timeScale = timeScaleOriginal;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        menuActive.SetActive(false);
        menuActive = null;
    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;
        gameGoalCountText.text = gameGoalCount.ToString("F0");

        if (gameGoalCount <= 0)
        {
            // unlock exit instead of instantly winning
            goalUnlocked = true;
        }
    }



    public void youLose()
    {
        statePaused();
        menuActive = menuLose;
        menuLose.SetActive(true);
    }
    public void updateAmmoUI(int current, int max)
    {
        ammoCurrentText.text = current.ToString();
        ammoMaxText.text = max.ToString();
    }
    public void UpdateSpellUI(List<GameObject> spellLoadout)
    {
        if (spellLoadout.Count > 0 && spellLoadout[0] != null)
            spell1Text.text = spellLoadout[0].name;

        if (spellLoadout.Count > 1 && spellLoadout[1] != null)
            spell2Text.text = spellLoadout[1].name;
    }
    public void SetRequiredCollectibles(int amount)
    {
        requiredCollectibles = amount;
    }

    public void AddCollectible()
    {
        collectedCount++;
    }

    public bool IsGoalUnlocked()
    {
        return goalUnlocked;
    }
    public void ShowWinMenu()
    {
        statePaused();
        menuActive = menuWin;
        menuWin.SetActive(true);
    }
}
