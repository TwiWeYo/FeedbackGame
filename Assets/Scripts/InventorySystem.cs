using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private AttackDatabaseSO db;

    [SerializeField]
    private InventoryToggle inventoryButtonPrefab;
    [SerializeField]
    private LayoutGroup attacksLayout, itemsLayout;

    private List<InventoryToggle> buyButtons = new();

    private HashSet<int> selectedAttackIds = new();
    private HashSet<int> selectedItemIds = new();

    [SerializeField]
    private AttackButton[] attackButtons;
    [SerializeField]
    private AttackButton[] itemAttackButtons;

    public GameObject skillBuyPanel;

    public event Action OnSkillBuyExit;

    private int maxAttacks = 2;
    private int maxItems = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        attackButtons[0].Setup(db.AttackTypes[0]);
    }

    public void ShowInventoryPanel()
    {
        skillBuyPanel.SetActive(true);

        SetupButtons();

        selectedAttackIds.Clear();
        selectedItemIds.Clear();

        maxAttacks = Math.Min(2, db.AttackTypes.Count(q => !q.IsOnlyForEnemies && !q.IsItem && q.WaveNumberUnlock <= gameManager.gameGrid.WaveNumber));
        maxItems = GetAvailableItemsCount();
    }


    public void HideInventoryPanel()
    {
        skillBuyPanel.SetActive(false);

        foreach (var button in buyButtons)
        {
            Destroy(button.gameObject);
            Destroy(button);
        }

        buyButtons.Clear();

        OnSkillBuyExit?.Invoke();
    }

    private void SetupButtons()
    {
        var availableAttacks = db.AttackTypes.Where(q => !q.IsOnlyForEnemies && !q.IsItem && q.WaveNumberUnlock <= gameManager.gameGrid.WaveNumber).ToList();
        var availableItems = db.AttackTypes.Where(q => !q.IsOnlyForEnemies && q.IsItem && q.WaveNumberUnlock <= gameManager.gameGrid.WaveNumber).ToList();

        foreach (var attack in availableAttacks)
        {
            var newButton = AddButtonToGrid(attacksLayout, attack);
            newButton.isItemButton = false;
            buyButtons.Add(newButton);
        }

        if (availableItems.Count == 0)
            return;

        foreach (var item in availableItems)
        {
            var newButton = AddButtonToGrid(itemsLayout, item);
            newButton.isItemButton = true;
            buyButtons.Add(newButton);
        }
    }

    private InventoryToggle AddButtonToGrid(LayoutGroup group, AttackType attackType)
    {
        var button = Instantiate(inventoryButtonPrefab);
        button.Setup(attackType);
        button.transform.SetParent(group.transform, false);

        button.SetToggleState(true, false);
        return button;
    }

    public void OnClickBuyButton(int attackId, InventoryToggle selectedButton, bool toggle)
    {
        if (!toggle)
        {
            if (selectedButton.isItemButton)
            {
                selectedItemIds.Remove(attackId);
            }
            else
            {
                selectedAttackIds.Remove(attackId);
            }

            selectedButton.SetToggleState(true, toggle);
            return;
        }

        var attackType = GetAttackTypeById(attackId);
        if (attackType == null)
            return;

        if (selectedButton.isItemButton && selectedItemIds.Count < maxItems)
        {
            selectedItemIds.Add(attackId);
            selectedButton.SetToggleState(true, toggle);
            return;
        }

        if (!selectedButton.isItemButton && selectedAttackIds.Count < maxAttacks)
        {
            selectedAttackIds.Add(attackId);
            selectedButton.SetToggleState(true, toggle);
            return;
        }

        selectedButton.SetToggleState(true, false);

    }

    public void OnClickConfirm()
    {
        int index = 0;
        foreach (var attackId in selectedAttackIds)
        {
            attackButtons[index].Setup(GetAttackTypeById(attackId));
            index++;
        }

        index = 0;
        foreach (var itemId in selectedItemIds)
        {
            itemAttackButtons[index].Setup(GetAttackTypeById(itemId));
            index++;
        }

        HideInventoryPanel();
    }

    public AttackType GetAttackTypeById(int attackId)
    {
        var index = db.AttackTypes.FindIndex(q => q.Id == attackId);
        if (index < 0)
            return null;

        return db.AttackTypes[index];
    }

    public bool AreAllItemsSelected => selectedAttackIds.Count == maxAttacks && selectedItemIds.Count == maxItems;

    private int GetAvailableItemsCount()
    {
        var max = gameManager.gameGrid.WaveNumber switch
        {
            int num when num < 13 => 0,
            int num when num < 20 => 1,
            _ => 2
        };

        return Math.Min(max, db.AttackTypes.Count(q => !q.IsOnlyForEnemies && q.IsItem && q.WaveNumberUnlock <= gameManager.gameGrid.WaveNumber));
    }
}
