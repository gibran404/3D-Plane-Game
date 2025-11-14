using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("UI")]
    public TMPro.TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;

    [Header("Scoring")]
    public float scoreMultiplier = 1f; // score per second or per unit distance
    private float score;

    [Header("State")]
    public bool isGameOver = false;

    private Transform player;
    private float startZ;

    [SerializeField]
    private PlayerController playerController;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startZ = player.position.z;

        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver)
        {
            Debug.Log("Game Over - not updating score");
            return;
        }

        UpdateScore();
    }

    void UpdateScore()
    {
        float distance = Time.deltaTime * 10;
        score += distance * scoreMultiplier;
        scoreText.text = Mathf.FloorToInt(score).ToString();
    }

    public void Crash()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        // Stop time or stop helicopter movement
        Time.timeScale = 0.05f;
        playerController.enabled = false;
        gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
