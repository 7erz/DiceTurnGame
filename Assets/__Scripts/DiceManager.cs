using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    private BodyDice currentSelectedDice;

    private List<BodyDice> allDices = new List<BodyDice>();



    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterDice(BodyDice dice)
    {
        if (!allDices.Contains(dice))
            allDices.Add(dice);
    }

    public void RollAllDices()
    {
        foreach (var dice in allDices)
        {
            dice.RollDice();
        }
    }

    public void SelectDice(BodyDice newDice)
    {
        if (currentSelectedDice == newDice)
        {
            DeselectAll();
            return;
        }

        if (currentSelectedDice != null)
            currentSelectedDice.SetOutline(false);

        currentSelectedDice = newDice;
        newDice.SetOutline(true);
    }

    public void DeselectAll()
    {
        if(currentSelectedDice != null)
        {
            currentSelectedDice.SetOutline(false);
            currentSelectedDice = null;
        }
    }
}
