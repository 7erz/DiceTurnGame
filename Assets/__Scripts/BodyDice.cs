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
        outline.enabled = false;
        DiceManager.Instance.RegisterDice(this);
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

    public void SetOutline(bool state)
    {
        outline.enabled = state;
    }

    private void OnSelected()
    {
        DiceManager.Instance.SelectDice(this);
    }
}
