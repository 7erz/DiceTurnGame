// GameManager.cs (����)
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SkillPanel skillPanel_UI; // �ν����Ϳ��� SkillPanel �Ҵ�
    public List<SkillData> playerSelectedSkills; // �÷��̾ ������ ��ų��

    void Start()
    {
        // playerSelectedSkills ����� ä�����ٰ� ����
        // ��: LoadPlayerData();
        if (skillPanel_UI != null && playerSelectedSkills != null && playerSelectedSkills.Count > 0)
        {
            skillPanel_UI.PopulateSkillButtons(playerSelectedSkills);
        }
    }
}