using System.Collections;
using UnityEngine;

public class StageResult : UI
{
    public System.Action restart_event;

    [SerializeField]
    private UILabel score;
    [SerializeField]
    private UILabel high_score;
    [SerializeField]
    private GameObject new_record;





    #region Override Function

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    #endregion

    public void SetData(int score)
    {
        Show();

        StartCoroutine(ResultProcess(score));
    }

    private IEnumerator ResultProcess(int score)
    {
        int high_score = DataManager.Instance.GetHighScore();
        this.high_score.text = high_score.ToString();

        /////////// 숫자 올라가는 효과 ///////////
        float value = 0;
        float max_value = score;
        for (int i = 0; i < 20; i++)
        {
            value += max_value / 20f;

            this.score.text = ((int)value).ToString();
            yield return new WaitForSeconds(0.025f);
        }
        this.score.text = ((int)max_value).ToString();
        ///////////////////////////////////////

        yield return new WaitForSeconds(0.15f);

        if (high_score < score)
        {
            new_record.SetActive(true);
            DataManager.Instance.SetHighScore(score);
        }
    }

    #region GUI Click Event

    public void OnClickReStart()
    {
        new_record.SetActive(false);
        Hide();

        if (restart_event != null)
            restart_event();
    }

    #endregion
}
