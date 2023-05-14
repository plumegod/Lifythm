using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RhythmScoreManager : MonoBehaviour
{
    //todo: 节奏分数计算和管理
    
    public int perfectHitScore = 10;
    public int goodHitScore = 7;
    public int missScore = 0;

    private int currentScore = 0;
    private int combo = 0;
    private int miss = 0;
    private int maxCombo = 0;
    
    public int CurrentScore => currentScore;
    public int MaxCombo => maxCombo;

    [SerializeField] private Color[] backGroundColors;

    [SerializeField] private GameObject scoreText;

    void Start()
    {
        UpdateScoreDisplay();
    }

    public void Init()
    {
        combo = 0;
        currentScore = 0;
        miss = 0;
    }

    public void OnPerfectHit()
    {
        combo++;
        currentScore += perfectHitScore * combo;
        UpdateScoreDisplay();
        GameObject.Find("BackGround").GetComponent<SpriteRenderer>().color = backGroundColors[Random.Range(0, backGroundColors.Length)];
        UpdateMaxCombo();
    }

    public void OnGoodHit()
    {
        combo++;
        currentScore += goodHitScore * combo;
        UpdateScoreDisplay();
        GameObject.Find("BackGround").GetComponent<SpriteRenderer>().color = backGroundColors[Random.Range(0, backGroundColors.Length)];
        UpdateMaxCombo();
    }

    public void OnMiss()
    {
        combo = 0;
        currentScore += missScore;
        UpdateScoreDisplay();
        GameObject.Find("BackGround").GetComponent<SpriteRenderer>().color = new Color(255, 255, 255,255);
        miss++;
    }

    private void UpdateScoreDisplay()
    {
        // 这里你需要根据实际情况更新分数和连击数的显示
        // 例如，如果你使用TextMeshPro，可以这样更新：
        // scoreText.text = $"Score: {currentScore}";
        // comboText.text = $"Combo: {combo}";
        scoreText.GetComponent<TMP_Text>().text = $"分数  {currentScore}";
        scoreText.GetComponent<Animator>().Play("Play");
    }

    private void UpdateMaxCombo()
    {
        if(combo > maxCombo)
            maxCombo = combo;
    }
}
