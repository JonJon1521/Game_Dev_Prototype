using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class gamemanager : MonoBehaviour
{
    //~~~~~~~~~~~~~~~~~~~~~GameObjects~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    //~~~~~~~~~~~~~~~~~~~~~Ints~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [SerializeField] int gameGoal;

    //~~~~~~~~~~~~~~~~~~~~~Tmp_Text~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [SerializeField] TMP_Text gameGoalCountText;
    [SerializeField] TMP_Text ammoCurrentText;
    [SerializeField] TMP_Text ammoMaxText;

    //~~~~~~~~~~~~~~~~~~~~~~TExtMEsh~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [SerializeField] TextMeshProUGUI spell1Text;
    [SerializeField] TextMeshProUGUI spell2Text;

    //~~~~~~~~~~~~~~~~~~~~~~Event Tracker~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [SerializeField] EventTracker tracker;

    //~~~~~~~~~~~~~~~~~~~~~~Images~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public Image playerHPBar;
    public Image playerManaBar;
    public Image playerPopularityBar;

    //~~~~~~~~~~~~~~~~~~~~~~Public GameObjects~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public GameObject player;
    public GameObject playerSpawnPos;
    public GameObject checkpointPopup;
    public GameObject damagePlayerFlash;
    //~~~~~~~~~~~~~~~~~~~~~~Public playerController~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public playerController playerScript;

    //~~~~~~~~~~~~~~~~~~~~~~Bools~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public bool isPaused;
    bool goalUnlocked = false;

    //~~~~~~~~~~~~~~~~~~~~~~Private Floats~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    private float timeScaleOriginal;

    //~~~~~~~~~~~~~~~~~~~~~~Private Ints~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    private int gameGoalCount;

    //~~~~~~~~~~~~~~~~~~~~~~???~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public static gamemanager instance;

    //~~~~~~~~~~~~~~~~~~~~~~Ints~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

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

        
    }

    void Start()
    {
        gameGoalCountText.text = gameGoalCount.ToString("F0");

        if (tracker != null && playerPopularityBar != null) // sync the bar when the scene starts so its not empty 
        {
            updatePopularityUI(tracker.popularity); // update the UI
        }
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
        if (!isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
        spell1Text.text = "";
        spell2Text.text = "";

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

    public void updatePopularityUI(int currentPopularity)
    {
        if(playerPopularityBar != null)
        {
            float fullness = (float)currentPopularity / 100f; // divide by 100 becuase the fill amount is between 0 and 1 and changes the size of teh bar stretching its scales

            playerPopularityBar.transform.localScale = new Vector3(fullness, 1, 1); // this scales it on the x axies (long ways up down)
        }
    } 

    void OnEnable()
    {
        if(tracker != null)
        {
            tracker.onPopularityChanged.AddListener(updatePopularityUI); // this well tell teh tracker ; " hey when popularity is changed run this UI"
        }
    }

    void OnDisable()
    {
        if(tracker != null)
        {
            tracker.onPopularityChanged.RemoveListener(updatePopularityUI); // cleans up when teh scene closes 
        }
    }
}
