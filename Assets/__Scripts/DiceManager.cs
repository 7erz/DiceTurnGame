// __Scripts/DiceManager.cs (��ȣ �ֻ��� ���� �߰� ����)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
// using UnityEngine.UI; // ���� �ڵ忡���� ���� ������� �����Ƿ� �ּ� ó�� ����

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    private BodyDice currentSelectedNumberDice; // ���� ���õ� '����' �ֻ���
    public BodyDice signDice; // ��ȣ �ֻ��� (�ν����Ϳ��� SDice ������Ʈ �Ҵ�)

    private List<BodyDice> allDices = new List<BodyDice>(); // ��� �ֻ��� (���� ��� ���� ��)
                                                            // �Ǵ� ���� �ֻ����� �����ϴ� ����Ʈ�� ���� ���

    public UnityEvent<BodyDice> OnDiceSelected = new UnityEvent<BodyDice>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterDice(BodyDice dice)
    {
        if (!allDices.Contains(dice)) // ���� ��Ĵ�� ��� �ֻ��� ���
            allDices.Add(dice);

        // ���� signDice�� �ν����Ϳ��� �Ҵ����� �ʰ�, �̸� ������ ã������ ���⼭ ó��
        // ��: if (dice.gameObject.name == "SDice") signDice = dice;
    }

    public void RollAllDices()
    {
        // signDice�� allDices�� �ִ� ���� �ֻ������� ���� ����
        // ����� allDices�� signDice�� ���Ե� �� �����Ƿ�, �ߺ� ���� ���� �Ǵ� ��Ȯ�� ���� �ʿ�
        // �Ʒ��� ��� �ֻ����� ������ ������ ���� (signDice�� allDices���� ���ԵǾ��ִٰ� ����)
        foreach (var dice in allDices)
        {
            dice.RollDice();
        }
    }

    // ���� �ֻ��� ���� ����
    public void SelectDice(BodyDice newDice)
    {
        if (newDice == signDice) // ��ȣ �ֻ����� ���� ����� �ƴ�
        {
            Debug.Log("��ȣ �ֻ����� ���� ������ �� �����ϴ�.");
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

    public void DeselectAll() // ���� �ֻ��� ���� ����
    {
        if (currentSelectedNumberDice != null)
        {
            currentSelectedNumberDice.SetOutline(false);
            currentSelectedNumberDice = null;
            OnDiceSelected.Invoke(null); // ���õ� �ֻ����� ������ �˸�
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
        return "+"; // �⺻�� �Ǵ� ���� ó��
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