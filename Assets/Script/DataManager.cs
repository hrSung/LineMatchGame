using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DataManager>();
                if (instance == null)
                {
                    Debug.LogError("DataManager Instance NULL!!!!!");
                    return null;
                }
            }
            return instance;
        }
    }





    #region MonoBehaviour Function

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    #endregion

    public void SetHighScore(int high_score)
    {
        PlayerPrefs.SetInt("HIGHSCORE", high_score);
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HIGHSCORE", 0);
    }
}
