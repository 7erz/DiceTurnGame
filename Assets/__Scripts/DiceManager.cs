// __Scripts/DiceManager.cs (부호 주사위 참조 추가 예시)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq; // LINQ를 사용하여 리스트 필터링 등 편리한 작업 가능
// using UnityEngine.UI; // 현재 코드에서는 직접 사용하지 않으므로 주석 처리 가능

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    private BodyDice currentSelectedNumberDice; // 현재 선택된 '숫자' 주사위
    public BodyDice signDice; // 부호 주사위 (인스펙터에서 SDice 오브젝트 할당)

    private List<BodyDice> allRegisteredDices = new List<BodyDice>(); // 모든 주사위 (기존 방식 유지 시)
                                                            // 또는 숫자 주사위만 관리하는 리스트로 변경 고려

    public UnityEvent<BodyDice> OnDiceSelected = new UnityEvent<BodyDice>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterDice(BodyDice dice)
    {
        if (!allRegisteredDices.Contains(dice))
            allRegisteredDices.Add(dice);

        // 부호 주사위 자동 할당 (이름 기반 예시, 인스펙터 할당이 더 안정적)
        if (dice.gameObject.name.Contains("SDice") && signDice == null) // SDice 프리팹의 이름이 "SDice"를 포함한다고 가정
        {
            signDice = dice;
            Debug.Log("부호 주사위 자동 등록됨: " + dice.gameObject.name);
        }
    }

    public void RollAllDices()
    {
        // 모든 등록된 주사위를 굴림 (각 주사위의 RollDice()에서 SetUsed(false) 호출됨)
        foreach (var dice in allRegisteredDices)
        {
            dice.RollDice();
        }
    }

    // 숫자 주사위 선택 로직
    public void SelectDice(BodyDice newDice)
    {
        if (newDice == signDice || newDice.IsUsedThisTurn) // 부호 주사위 또는 이미 사용된 주사위는 선택 불가
        {
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
    public void ResetAllDicesForNewTurn()
    {
        Debug.Log("모든 주사위 사용 상태 초기화.");
        // ToList()를 사용하여 반복 중 컬렉션 수정 문제 방지 (RollDice 내에서 SetUsed 호출 시)
        foreach (var dice in allRegisteredDices.ToList())
        {
            if (dice != signDice) // 부호 주사위는 사용 개념이 다를 수 있으므로 숫자 주사위만
            {
                dice.SetUsed(false); // RollDice 내부에서도 호출되지만, 명시적으로 여기서도 호출하여 상태 보장
                dice.SetOutline(false); // 선택 해제
            }
        }
        DeselectAll(); // 선택된 주사위도 해제
    }
}