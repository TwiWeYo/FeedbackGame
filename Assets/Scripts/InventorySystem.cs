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

    private const int maxAttacks = 2;
    private int maxItems = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }



    public void ShowInventoryPanel()
    {
        skillBuyPanel.SetActive(true);

        SetupButtons(false);
        SetupButtons(true);

        selectedAttackIds.Clear();
        selectedItemIds.Clear();

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

    private void SetupButtons(bool isItems)
    {
        if (isItems && gameManager.gameGrid.WaveNumber < 3)
        {
            return;
        }

        var uniqueAttackIds = attackButtons
            .Select(q => q.attackId)
            .Where(q => q >= 0)
            .ToHashSet();

        var count = isItems ? 3 : uniqueAttackIds.Count + 3;

        while (uniqueAttackIds.Count < count && uniqueAttackIds.Count < db.AttackTypes.Count(q => q.WaveNumberUnlock < gameManager.gameGrid.WaveNumber))
        {
            var randomId = UnityEngine.Random.Range(0, db.AttackTypes.Count);
            if (uniqueAttackIds.Contains(randomId) || GetAttackTypeById(randomId).WaveNumberUnlock > gameManager.gameGrid.WaveNumber)
                continue;

            uniqueAttackIds.Add(randomId);
        }

        foreach (var attackId in uniqueAttackIds)
        {
            var newButton = AddButtonToGrid(isItems ? itemsLayout : attacksLayout, attackId);
            newButton.isItemButton = isItems;
            buyButtons.Add(newButton);
        }
    }

    private InventoryToggle AddButtonToGrid(LayoutGroup group, int attackType)
    {
        var button = Instantiate(inventoryButtonPrefab);
        button.Setup(GetAttackTypeById(attackType));
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
        return gameManager.gameGrid.WaveNumber switch
        {
            int num when num < 3 => 0,
            int num when num < 5 => 1,
            _ => 2
        };
    }
}
