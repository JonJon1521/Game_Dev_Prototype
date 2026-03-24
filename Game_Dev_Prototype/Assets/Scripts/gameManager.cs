using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class gamemanager : MonoBehaviour
{
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] int gameGoal;
    [SerializeField] TMP_Text gameGoalCountText;

    public Image playerHPBar;
    public GameObject player;
    public playerController playerScript;
    public GameObject playerSpawnPos;
    public GameObject checkpointPopup;
    public GameObject damagePlayerFlash;
    public bool isPaused;

    private float timeScaleOriginal;

    private int gameGoalCount;

    public static gamemanager instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;

        timeScaleOriginal = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();

        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
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
            statePaused();
            menuActive = menuWin;
            menuWin.SetActive(true);

        }
    }

    public void youLose()
    {
        statePaused();
        menuActive = menuLose;
        menuLose.SetActive(true);
    }
}
