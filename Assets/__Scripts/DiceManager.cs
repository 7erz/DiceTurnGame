using UnityEngine;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    private BodyDice currentSelectedDice;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
