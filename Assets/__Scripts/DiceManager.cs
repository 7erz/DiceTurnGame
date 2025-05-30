// __Scripts/DiceManager.cs (부호 주사위 참조 추가 예시)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
// using UnityEngine.UI; // 현재 코드에서는 직접 사용하지 않으므로 주석 처리 가능

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    private BodyDice currentSelectedNumberDice; // 현재 선택된 '숫자' 주사위
    public BodyDice signDice; // 부호 주사위 (인스펙터에서 SDice 오브젝트 할당)

    private List<BodyDice> allDices = new List<BodyDice>(); // 모든 주사위 (기존 방식 유지 시)
                                                            // 또는 숫자 주사위만 관리하는 리스트로 변경 고려

    public UnityEvent<BodyDice> OnDiceSelected = new UnityEvent<BodyDice>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterDice(BodyDice dice)
    {
        if (!allDices.Contains(dice)) // 기존 방식대로 모든 주사위 등록
            allDices.Add(dice);

        // 만약 signDice를 인스펙터에서 할당하지 않고, 이름 등으로 찾으려면 여기서 처리
        // 예: if (dice.gameObject.name == "SDice") signDice = dice;
    }

    public void RollAllDices()
    {
        // signDice와 allDices에 있는 숫자 주사위들을 각각 굴림
        // 현재는 allDices에 signDice도 포함될 수 있으므로, 중복 굴림 방지 또는 명확한 구분 필요
        // 아래는 모든 주사위를 굴리는 간단한 예시 (signDice가 allDices에도 포함되어있다고 가정)
        foreach (var dice in allDices)
        {
            dice.RollDice();
        }
    }

    // 숫자 주사위 선택 로직
    public void SelectDice(BodyDice newDice)
    {
        if (newDice == signDice) // 부호 주사위는 선택 대상이 아님
        {
            Debug.Log("부호 주사위는 직접 선택할 수 없습니다.");
            return;
        }

        if (currentSelectedNumberDice == newDice)
        {
            DeselectAll();
            return;
        }

        if (currentSelectedNumberDice != null)
            currentSelectedNumberDice.SetOutline(false);

        currentSelectedNumberDice = newDice;
        newDice.SetOutline(true);
        OnDiceSelected.Invoke(newDice);
    }

    public void DeselectAll() // 숫자 주사위 선택 해제
    {
        if (currentSelectedNumberDice != null)
        {
            currentSelectedNumberDice.SetOutline(false);
            currentSelectedNumberDice = null;
            OnDiceSelected.Invoke(null); // 선택된 주사위가 없음을 알림
        }
    }

    public BodyDice GetSelectedNumberDice()
    {
        return currentSelectedNumberDice;
    }

    public string GetSignOperation()
    {
        if (signDice != null && !string.IsNullOrEmpty(signDice.LastRollResult))
        {
            return signDice.LastRollResult;
        }
        return "+"; // 기본값 또는 오류 처리
    }

    public bool TryGetSelectedNumberValue(out int value)
    {
        value = 0;
        if (currentSelectedNumberDice != null &&
            int.TryParse(currentSelectedNumberDice.LastRollResult, out value))
        {
            return true;
        }
        return false;
    }
}