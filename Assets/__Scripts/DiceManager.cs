// __Scripts/DiceManager.cs (��ȣ �ֻ��� ���� �߰� ����)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq; // LINQ�� ����Ͽ� ����Ʈ ���͸� �� ���� �۾� ����
// using UnityEngine.UI; // ���� �ڵ忡���� ���� ������� �����Ƿ� �ּ� ó�� ����

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    private BodyDice currentSelectedNumberDice; // ���� ���õ� '����' �ֻ���
    public BodyDice signDice; // ��ȣ �ֻ��� (�ν����Ϳ��� SDice ������Ʈ �Ҵ�)

    private List<BodyDice> allRegisteredDices = new List<BodyDice>(); // ��� �ֻ��� (���� ��� ���� ��)
                                                            // �Ǵ� ���� �ֻ����� �����ϴ� ����Ʈ�� ���� ���

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

        // ��ȣ �ֻ��� �ڵ� �Ҵ� (�̸� ��� ����, �ν����� �Ҵ��� �� ������)
        if (dice.gameObject.name.Contains("SDice") && signDice == null) // SDice �������� �̸��� "SDice"�� �����Ѵٰ� ����
        {
            signDice = dice;
            Debug.Log("��ȣ �ֻ��� �ڵ� ��ϵ�: " + dice.gameObject.name);
        }
    }

    public void RollAllDices()
    {
        // ��� ��ϵ� �ֻ����� ���� (�� �ֻ����� RollDice()���� SetUsed(false) ȣ���)
        foreach (var dice in allRegisteredDices)
        {
            dice.RollDice();
        }
    }

    // ���� �ֻ��� ���� ����
    public void SelectDice(BodyDice newDice)
    {
        if (newDice == signDice || newDice.IsUsedThisTurn) // ��ȣ �ֻ��� �Ǵ� �̹� ���� �ֻ����� ���� �Ұ�
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
    public void ResetAllDicesForNewTurn()
    {
        Debug.Log("��� �ֻ��� ��� ���� �ʱ�ȭ.");
        // ToList()�� ����Ͽ� �ݺ� �� �÷��� ���� ���� ���� (RollDice ������ SetUsed ȣ�� ��)
        foreach (var dice in allRegisteredDices.ToList())
        {
            if (dice != signDice) // ��ȣ �ֻ����� ��� ������ �ٸ� �� �����Ƿ� ���� �ֻ�����
            {
                dice.SetUsed(false); // RollDice ���ο����� ȣ�������, ��������� ���⼭�� ȣ���Ͽ� ���� ����
                dice.SetOutline(false); // ���� ����
            }
        }
        DeselectAll(); // ���õ� �ֻ����� ����
    }
}