using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainView : MonoBehaviour
{
    private const string PUZZLE_SCENE_NAME = "PUZZLE";

    [SerializeField]
    private UILabel high_score;





    #region MonoBehaviour Function

    void Awake()
    {
        high_score.text = DataManager.Instance.GetHighScore().ToString();
    }

    #endregion



    #region GUI Click Event

    public void OnClickStart()
    {
        SceneManager.LoadSceneAsync(PUZZLE_SCENE_NAME);
    }

    #endregion
}
