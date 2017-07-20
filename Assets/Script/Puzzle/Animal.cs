using System.Collections;
using UnityEngine;

public class Animal : PoolItem
{
    private const string ANIMATION_ANIMAL_NAME = "animal{0}_{1}";
    private const string ANIMAL_NAME = "animal{0}_01";
    private const string ANIMAL_SELECT_NAME = "animal{0}_08";
    private const int START_ANI_NUMBER = 1;
    private const int END_ANI_NUMBER = 12;

    private struct AnimalPositionIndex
    {
        public int x;
        public int y;

        public AnimalPositionIndex(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }


    public System.Action<Animal> after_destroy_event;

    [SerializeField]
    private UISprite sprite;
    [SerializeField]
    private GameObject select_obj;
    [SerializeField]
    private UISpriteAnimation destroy_ani;
    [SerializeField]
    private UILabel test_label;

    private eAnimalType animal_type;
    private AnimalPositionIndex position_idx;
    private bool is_guide_ani_play;

    public int PosIdxX { get { return position_idx.x; } }
    public int PosIdxY { get { return position_idx.y; } }
    public eAnimalType AnimalType { get { return animal_type; } }

    public Vector3 AnimalLocalPosition
    {
        get
        {
            return CachingTransform.localPosition;
        }
        set
        {
            CachingTransform.localPosition = value;
        }
    }



    #region Override Function

    protected override void OnAwake()
    {

    }

    public override void ResetItem()
    {
        destroy_ani.ResetToBeginning();
        SetSelect(false);
        animal_type = eAnimalType.None;
    }

    #endregion

    // 동물 세팅 함수.
    // 실제 위치가 아닌 몇번째 칸에 있는지 세팅함.
    public void SetData(int x_idx, int y_idx)
    {
        if(animal_type == eAnimalType.None)
        {
            int animal_num = Random.Range(1, 7);
            sprite.spriteName = string.Format(ANIMAL_NAME, animal_num);
            animal_type = (eAnimalType)animal_num;
        }

        position_idx = new AnimalPositionIndex(x_idx, y_idx);
        //test_label.text = string.Format("{0}, {1}", x_idx, y_idx);
    }

    public void SetDestroy()
    {
        StartCoroutine(DestroyEffectProcess());
    }

    // 이펙트 실행 후 받은 이벤트 실행.
    private IEnumerator DestroyEffectProcess()
    {
        destroy_ani.gameObject.SetActive(true);

        while (destroy_ani.isPlaying)
        {
            yield return null;
        }

        destroy_ani.gameObject.SetActive(false);

        if (after_destroy_event != null)
            after_destroy_event(this);
    }

    // 가이드 애니 실행.
    public void ShowGuideAni()
    {
        is_guide_ani_play = true;
        StartCoroutine(GuideEffectProcess());
    }

    private IEnumerator GuideEffectProcess()
    {
        int cur_ani_num = START_ANI_NUMBER;
        while(cur_ani_num <= END_ANI_NUMBER && is_guide_ani_play)
        {
            sprite.spriteName = string.Format(ANIMATION_ANIMAL_NAME, (int)animal_type, cur_ani_num.ToString("00"));
            yield return new WaitForSeconds(0.05f);
            cur_ani_num++;
        }

        if(is_guide_ani_play)
        {
            is_guide_ani_play = false;
            sprite.spriteName = string.Format(ANIMAL_NAME, (int)animal_type);
        }
    }

    // 동물이 선택 되었을 때 처리.
    public void SetSelect(bool is_select)
    {
        is_guide_ani_play = false;
        sprite.spriteName = string.Format(is_select ? ANIMAL_SELECT_NAME : ANIMAL_NAME, (int)animal_type);

        if (select_obj.activeSelf != is_select)
            select_obj.SetActive(is_select);
    }

    public void MovePosition(Vector3 destination_pos)
    {
        StartCoroutine(MoveProcess(destination_pos));
    }

    // 동물 배치 할 때 움직임 처리.
    private IEnumerator MoveProcess(Vector3 destination_pos)
    {
        yield return null;

        Vector3 one_time_move_pos = Vector3.up * 140f;
        if(AnimalLocalPosition != destination_pos)
        {
            TweenPosition.Begin(CachingGameObject, 0.1f, AnimalLocalPosition - one_time_move_pos)
                .AddOnFinished(() => { MovePosition(destination_pos); });
        }
    }
}
