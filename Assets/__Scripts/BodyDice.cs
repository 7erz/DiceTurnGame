using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// 주사위 한 면의 정보를 담는 클래스
[System.Serializable]
public class DiceFace
{
    public string symbol;         // 예: "+", "-", "*", "/"
    public Sprite faceSprite;     // 해당 면의 이미지
    public bool isLocked = false; // 잠금 여부(선택)
}

public class BodyDice : MonoBehaviour
{
    [SerializeField] private List<DiceFace> diceFaces = new List<DiceFace>(6); // 6면 주사위
    [SerializeField] private Image diceImage;
    [SerializeField] private float rollAnimationSpeed = 0.05f;
    [SerializeField] private int animationFrames = 20;
    [SerializeField] private Outline outline; // 주사위 외곽선
    [SerializeField] private Button selectButton;


    public UnityEvent<string> OnDiceRolled; // 결과 심볼(string) 전달
    public string LastRollResult { get; private set; }
    private bool isRolling = false;

    public bool IsUsedThisTurn { get; private set; } = false; // 턴에서 사용 여부

    void Awake()
    {
        if (diceImage == null)
            diceImage = GetComponent<Image>();

        if (diceImage == null || diceFaces == null || diceFaces.Count != 6)
        {
            enabled = false;
            return;
        }
        foreach (var face in diceFaces)
        {
            if (face == null || face.faceSprite == null)
            {
                enabled = false;
                return;
            }
        }
        LastRollResult = diceFaces[0].symbol;
        diceImage.sprite = diceFaces[0].faceSprite;
    }

    void Start()
    {
        if (outline != null) outline.enabled = false;
        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.RegisterDice(this);

            // 자신이 부호 주사위인지 DiceManager를 통해 확인
            if (DiceManager.Instance.signDice == this)
            {
                SetUsed(false); // 부호 주사위는 항상 사용 가능(선택 가능) 상태로 시작
                                // 부호 주사위는 선택 버튼이 없을 수도 있으므로, selectButton null 체크가 중요
                if (selectButton != null) selectButton.interactable = true; // 부호 주사위도 버튼이 있다면 활성화
            }
            else
            {
                SetOutline(true); // 숫자 주사위는 외곽선 표시
                SetUsed(true); // 숫자 주사위는 게임 시작 시 사용된 상태(비활성화)로 시작
            }
        }
    }

    // 주사위 굴리기
    public void RollDice()
    {
        if (!isRolling)
            StartCoroutine(RollTheDiceCoroutine());
    }

    private IEnumerator RollTheDiceCoroutine()
    {
        isRolling = true;
        LastRollResult = "";
        int finalResultIndex = Random.Range(0, diceFaces.Count);

        for (int i = 0; i < animationFrames; i++)
        {
            int randomIndex = Random.Range(0, diceFaces.Count);
            diceImage.sprite = diceFaces[randomIndex].faceSprite;
            yield return new WaitForSeconds(rollAnimationSpeed);
        }

        diceImage.sprite = diceFaces[finalResultIndex].faceSprite;
        LastRollResult = diceFaces[finalResultIndex].symbol;
        isRolling = false;
        Debug.Log("주사위 결과: " + LastRollResult);
        OnDiceRolled?.Invoke(LastRollResult);
    }

    // 주사위 초기화 예시 (+ 5개, * 1개)
    public void InitializeDice(Sprite plusSprite, Sprite starSprite)
    {
        diceFaces.Clear();
        for (int i = 0; i < 5; i++)
            diceFaces.Add(new DiceFace { symbol = "+", faceSprite = plusSprite });
        diceFaces.Add(new DiceFace { symbol = "*", faceSprite = starSprite });
    }

    // 특정 면을 다른 심볼/이미지로 변경
    public void ChangeFace(int index, string newSymbol, Sprite newSprite)
    {
        if (index < 0 || index >= diceFaces.Count) return;
        if (!diceFaces[index].isLocked)
        {
            diceFaces[index].symbol = newSymbol;
            diceFaces[index].faceSprite = newSprite;
        }
    }

    // 전체에서 특정 심볼을 찾아 변경
    public void ChangeAllSymbols(string targetSymbol, string newSymbol, Sprite newSprite)
    {
        for (int i = 0; i < diceFaces.Count; i++)
        {
            if (diceFaces[i].symbol == targetSymbol && !diceFaces[i].isLocked)
                ChangeFace(i, newSymbol, newSprite);
        }
    }

    // 턴 종료 시 * 추가 (랜덤 +를 *로 변경)
    public void AddStarOnTurnEnd(Sprite starSprite)
    {
        List<int> plusIndices = new List<int>();
        for (int i = 0; i < diceFaces.Count; i++)
        {
            if (diceFaces[i].symbol == "+" && !diceFaces[i].isLocked)
                plusIndices.Add(i);
        }
        if (plusIndices.Count > 0)
        {
            int randomIndex = plusIndices[Random.Range(0, plusIndices.Count)];
            ChangeFace(randomIndex, "*", starSprite);
        }
    }

    // 디버프: 무작위 면을 - 또는 / 등으로 변경
    public void ApplyDebuff(string debuffSymbol, Sprite debuffSprite)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < diceFaces.Count; i++)
        {
            if (diceFaces[i].symbol != "*" && !diceFaces[i].isLocked)
                candidates.Add(i);
        }
        if (candidates.Count > 0)
        {
            int idx = candidates[Random.Range(0, candidates.Count)];
            ChangeFace(idx, debuffSymbol, debuffSprite);
        }
    }

    public void SetUsed(bool used)
    {
        IsUsedThisTurn = used;
        if (selectButton != null)
        {
            selectButton.interactable = !used; // 사용되었으면 버튼 비활성화
        }
        // 추가적인 시각적 피드백 (예: 알파값 변경)
        if (diceImage != null)
        {
            Color color = diceImage.color;
            color.a = used ? 0.5f : 1.0f; // 사용되었으면 반투명하게
            diceImage.color = color;
        }
        if (used) // 사용됨으로 설정되면 외곽선도 해제
        {
            SetOutline(false);
        }
    }

    public void SetOutline(bool state)
    {
        if (outline != null && !IsUsedThisTurn) // 사용된 주사위는 외곽선 표시 안 함
        {
            outline.enabled = state;
        }
        else if (outline != null && IsUsedThisTurn && state) // 사용된 주사위에 외곽선 켜려고 하면 강제 해제
        {
            outline.enabled = false;
        }
    }

    public void OnSelected()
    {
        if (IsUsedThisTurn)
        {
            Debug.Log("이 주사위는 이번 턴에 이미 사용되었습니다.");
            return;
        }
        if (isRolling)
        {
            Debug.Log("주사위가 굴러가는 중에는 선택할 수 없습니다.");
            return;
        }

        int num;
        if (int.TryParse(LastRollResult, out num)) // 숫자 주사위만 선택 가능하도록
        {
            DiceManager.Instance.SelectDice(this);
        }
        else
        {
            // 부호 주사위 등 숫자가 아닌 주사위는 이 함수로 선택되지 않음
            // (DiceManager.SelectDice에서 부호 주사위 선택 방지 로직도 있음)
        }
    }
}
