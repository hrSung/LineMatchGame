using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    private const int PLUS_SCORE = 20;
    private const int BASE_SCORE = 50;
    private const int MINIMUM_COMPLETE_PUZZLE_COUNT = 3;
    private const int TIME_LIMIT = 60;
    private const float NONE_CLICK_TIME = 3f;

    [SerializeField]
    private ObjectPool object_pool;
    [SerializeField]
    private List<TileLine> tile_lines;
    [SerializeField]
    private GameObject start_text_obj;
    [SerializeField]
    private UILabel score_text;
    [SerializeField]
    private UILabel high_score;
    [SerializeField]
    private UILabel time_text;
    [SerializeField]
    private GameObject time_out_text_obj;
    [SerializeField]
    private StageResult stage_result;

    private List<Animal>[] arr_animals;
    private List<Animal> select_animals = new List<Animal>();
    private List<Animal> guide_animal = new List<Animal>();
    private bool is_click_puzzle;   // 퍼즐 동물들이 클릭이 되는지의 여부
    private int destroy_count;
    private int cur_score;
    private int time_value;
    private float none_click_time;





    #region MonoBehaviour Function

    void Awake()
    {
        arr_animals = new List<Animal>[tile_lines.Count];
        stage_result.restart_event = ReStart;

        Init();

        StartCoroutine(SetPuzzleProcess());
    }

    void Update()
    {
        if (!is_click_puzzle)
            return;

        if (UICamera.currentCamera)
        {
            if (Input.GetMouseButton(0))    // 클릭 중일 때
            {
                none_click_time = 0;
                PuzzleRayCastProcess();
            }
            else if(select_animals.Count > 0)    // 하나 이상 클릭 후 뗐을 때
            {
                CompletePuzzle();
            }
            else    // 클릭이 없을 때
            {
                none_click_time += Time.deltaTime;
                if(none_click_time > NONE_CLICK_TIME)
                {
                    ShowGuidePuzzle();
                    none_click_time = 0;
                }
            }
        }
    }

    #endregion

    // 퍼즐 세팅전 초기화.
    private void Init()
    {
        none_click_time = 0;
        destroy_count = 0;
        is_click_puzzle = false;
        time_value = TIME_LIMIT;
        time_text.text = time_value.ToString();
        cur_score = 0;
        score_text.text = cur_score.ToString();
        high_score.text = DataManager.Instance.GetHighScore().ToString();
        time_out_text_obj.SetActive(false);
        select_animals.Clear();
        guide_animal.Clear();
        InitAnimals();
    }

    // 퍼즐 동물들 초기화.
    private void InitAnimals()
    {
        select_animals.Clear();
        for (int i = 0; i < arr_animals.Length; i++)
        {
            List<Animal> list = arr_animals[i];
            if (list == null)
                continue;

            for (int j = 0; j < list.Count; j++)
            {
                object_pool.RealeseItem(list[j]);
            }
        }

        System.Array.Clear(arr_animals, 0, arr_animals.Length);
    }

    // 퍼즐 초기 세팅.
    private IEnumerator SetPuzzleProcess()
    {
        yield return new WaitForEndOfFrame();

        float wait_second = 0;
        for (int i = 0; i < tile_lines.Count; i++)
        {
            TileLine tile_line = tile_lines[i];
            Vector3 first_tile_pos = tile_line.CreatePosition;
            List<Animal> list = new List<Animal>();
            for (int j = 0; j < tile_line.TileCount; j++)
            {
                Animal animal = CreateAnimal();
                animal.SetData(i, j);
                animal.AnimalLocalPosition
                    = first_tile_pos + Vector3.up * (140f + 140 * (j + (IsEvneNumber(i) ? 0 : 1)));
                animal.MovePosition(tile_line.GetTilePosition(j));
                list.Add(animal);
            }
            arr_animals[i] = list;
            wait_second += 0.15f;
        }

        yield return new WaitForSeconds(wait_second);

        StartCoroutine(StartEventProcess());
    }

    // 퍼즐 시작 이벤트.
    private IEnumerator StartEventProcess()
    {
        start_text_obj.transform.localScale = Vector3.zero;
        start_text_obj.SetActive(true);
        TweenScale tween = TweenScale.Begin(start_text_obj, 0.2f, Vector3.one);

        yield return new WaitForSeconds(1.5f);

        tween.PlayReverse();

        yield return new WaitForSeconds(0.25f);
        start_text_obj.SetActive(false);

        is_click_puzzle = true;
        StartCoroutine(TimeLimitProcess());
    }

    // 제한 시간 체크.
    private IEnumerator TimeLimitProcess()
    {
        while(time_value > 0)
        {
            yield return new WaitForSeconds(1f);

            time_value--;
            time_text.text = time_value.ToString();
        }

        is_click_puzzle = false;
        ShowTimeOut();

        yield return new WaitForSeconds(2f);
        time_out_text_obj.SetActive(false);

        stage_result.SetData(cur_score);
    }

    // 퍼즐 동물 초기 생성.
    private Animal CreateAnimal()
    {
        Animal animal = (Animal)object_pool.GetItem();
        animal.after_destroy_event = DestroyAnimal;
        return animal;
    }

    // 퍼즐 동물들만 체크.
    private void PuzzleRayCastProcess()
    {
        Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.lastEventPosition);
        RaycastHit2D raycast = Physics2D.GetRayIntersection(ray, Mathf.Infinity
            , 1 << LayerMask.NameToLayer("Animal"));

        if (raycast)
            SetAnimalCount(raycast.transform.GetComponent<Animal>());
    }

    // 체크 후 선택 중인 상태인지 아닌지 처리.
    private void SetAnimalCount(Animal animal)
    {
        Animal select_animal = select_animals.Find(x => x == animal);
        if (select_animal == null)
        {
            if (select_animals.Count > 0)
            {
                Animal last_animal = select_animals[select_animals.Count - 1];
                if (!CheckAnimalAdjoinAndType(animal, last_animal))
                    return;
            }

            animal.SetSelect(true);
            select_animals.Add(animal);
        }
        else
        {
            for (int i = select_animals.Count - 1; i > -1; i--)
            {
                if (select_animals[i] == animal)
                    break;

                select_animals[i].SetSelect(false);
                select_animals.RemoveAt(i);
            }
        }
    }

    // 클릭 중인 동물을 기준으로 인접하고 같은 동물만 선택 가능하도록 체크.
    private bool CheckAnimalAdjoinAndType(Animal select_animal, Animal last_animal)
    {
        int select_animal_x = select_animal.PosIdxX;
        int select_animal_y = select_animal.PosIdxY;
        int last_animal_x = last_animal.PosIdxX;
        int last_animal_y = last_animal.PosIdxY;
        int value_x = last_animal_x - select_animal_x;
        int value_y = last_animal_y - select_animal_y;
        bool is_even_number_x = IsEvneNumber(last_animal_x);

        if (last_animal.AnimalType == select_animal.AnimalType)
        {
            if (Mathf.Abs(value_x) > 1)
                return false;
            if (value_x == 0 && Mathf.Abs(value_y) <= 1)
                return true;
            if (is_even_number_x && (value_y == 0 || value_y == 1))
                return true;
            if (!is_even_number_x && (value_y == 0 || value_y == -1))
                return true;
        }

        return false;
    }

    // 동물 선택 후 뗐을 때 처리.
    private void CompletePuzzle()
    {
        int select_count = select_animals.Count;
        if(select_count < MINIMUM_COMPLETE_PUZZLE_COUNT)    // 정해진 개수보다 선택된 개수가 작을 때.
        {
            for (int i = 0; i < select_count; i++)
            {
                select_animals[i].SetSelect(false);
            }
        }
        else
        {
            int score = 0;
            is_click_puzzle = false;
            for (int i = 0; i < select_count; i++)
            {
                if (i > MINIMUM_COMPLETE_PUZZLE_COUNT)
                {
                    score += BASE_SCORE + ((i - (MINIMUM_COMPLETE_PUZZLE_COUNT - 1)) * PLUS_SCORE);
                }
                else
                {
                    score += BASE_SCORE;
                }
                select_animals[i].SetDestroy();
            }
            SetScore(score);
        }
        select_animals.Clear();
        StartCoroutine(WaitDestroy(select_count));
    }

    // 클릭이 없을 때 맞출 수 있는 동물 퍼즐 애니 실행.
    private void ShowGuidePuzzle()
    {
        for (int i = 0; i < arr_animals.Length; i++)
        {
            List<Animal> list = arr_animals[i];
            for (int j = 0; j < list.Count; j++)
            {
                guide_animal.Clear();
                GetFindAnimalPuzzleCount(list[j]);

                // 최소 동물 퍼즐 개수 이상이 되면 애니 실행.
                if(guide_animal.Count >= MINIMUM_COMPLETE_PUZZLE_COUNT)
                {
                    for (int k = 0; k < guide_animal.Count; k++)
                    {
                        guide_animal[k].ShowGuideAni();
                    }
                    return;
                }
            }
        }
    }

    // 동물 퍼즐을 이어서 계속 찾음.
    private void GetFindAnimalPuzzleCount(Animal animal)
    {
        List<Animal> list = FindAnimalAdjoin(animal);
        if (list.Count == 0)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            Animal ani = list[i];
            GetFindAnimalPuzzleCount(ani);
        }
    }

    // 인접한 동물 퍼즐 리스트를 찾음.
    private List<Animal> FindAnimalAdjoin(Animal animal)
    {
        List<Animal> list = new List<Animal>();
        for (int i = 0; i < arr_animals.Length; i++)
        {
            List<Animal> animals = arr_animals[i];
            for (int j = 0; j < animals.Count; j++)
            {
                Animal ani = animals[j];
                if (!guide_animal.Find(x => x == ani) && CheckAnimalAdjoinAndType(animal, ani))
                {
                    list.Add(ani);
                    guide_animal.Add(ani);
                }
            }
        }

        return list;
    }

    // 퍼즐 동물들이 사라질 때 까지 대기.
    private IEnumerator WaitDestroy(int select_count)
    {
        while (select_count != destroy_count)
        {
            yield return null;
        }

        StartCoroutine(RegenerationAnimalProcess());
    }

    // 빈곳에 퍼즐을 채워 넣음.
    private IEnumerator RegenerationAnimalProcess()
    {
        float wait_second = 0;
        for (int i = 0; i < tile_lines.Count; i++)
        {
            TileLine tile_line = tile_lines[i];
            List<Animal> list = arr_animals[i];
            int before_count = list.Count;
            for (int j = 0; j < tile_line.TileCount; j++)
            {
                if (before_count > j)
                {
                    Animal animal = list[j];
                    animal.SetData(i, j);
                    animal.MovePosition(tile_line.GetTilePosition(j));
                }
                else
                {
                    Animal create_animal = CreateAnimal();
                    create_animal.SetData(i, j);
                    create_animal.AnimalLocalPosition = tile_line.CreatePosition
                        + new Vector3(0, 140f + 140f * (j - before_count));
                    create_animal.MovePosition(tile_line.GetTilePosition(j));
                    list.Add(create_animal);
                    wait_second += 0.11f;
                }
            }
        }

        yield return new WaitForSeconds(wait_second);

        destroy_count = 0;
        is_click_puzzle = true;
    }

    // 각 퍼즐 동물들이 사라질 때 처리.
    private void DestroyAnimal(Animal animal)
    {
        object_pool.RealeseItem(animal);
        List<Animal> list = arr_animals[animal.PosIdxX];
        list.Remove(animal);
        destroy_count++;
    }

    // 점수 등록.
    private void SetScore(int score)
    {
        StartCoroutine(ScoreNumberEffectProcess(score));
    }

    private IEnumerator ScoreNumberEffectProcess(int score)
    {
        float value = cur_score;
        float plus_value = score;
        for (int i = 0; i < 20; i++)
        {
            value += plus_value / 20f;

            score_text.text = ((int)value).ToString();
            yield return new WaitForSeconds(0.025f);
        }
        cur_score += score;
        score_text.text = cur_score.ToString();
    }

    // 제한 시간이 다 되었을 때 문구 표시.
    private void ShowTimeOut()
    {
        time_out_text_obj.transform.localScale = Vector3.zero;
        time_out_text_obj.SetActive(true);
        TweenScale.Begin(time_out_text_obj, 0.2f, Vector3.one);
    }

    // 재시작.
    private void ReStart()
    {
        Init();

        StartCoroutine(SetPuzzleProcess());
    }

    // 홀수인지 짝수인지 체크.
    private bool IsEvneNumber(int num)
    {
        return (num & 1) == 0;
    }
}
