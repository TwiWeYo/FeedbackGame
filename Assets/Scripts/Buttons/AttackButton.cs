using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackButton : MonoBehaviour
{
    public Button button;

    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private bool isItem;

    public int attackId;
    public Image skillIconImage;
    public TextMeshProUGUI skillNameText; // Для отображения названия скилла

    private bool isUsed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectAttack);

        if (attackId < 0)
        {
            Setup(null);
        }
    }

    public void Setup(AttackType attack)
    {
        if (attack != null)
        {
            attackId = attack.Id;
            button.interactable = true;

            if (attack.AttackIcon != null)
            {
                skillIconImage.sprite = attack.AttackIcon;
                skillIconImage.enabled = true; // Показываем иконку
            }

            if (skillNameText != null) skillNameText.text = attack.AttackName;
        }
        else
        {
            if (skillIconImage?.sprite != null)
                skillIconImage.sprite = null;

            if (skillNameText != null)
                skillNameText.text = isItem ? "Used!" : "Locked";

            button.interactable = false;
        }
    }

    private void SelectAttack()
    {
        if (isUsed)
            return;

        gameManager.StartAttacking(attackId, this, isItem);
    }

    private void UpdateIsUsed(List<Vector3Int> moves)
    {
        isUsed = moves.Any();
        gameManager.gameGrid.PlayerManager.OnPlayerMoved -= UpdateIsUsed;
    }
}
