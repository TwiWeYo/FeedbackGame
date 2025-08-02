using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class InventoryButton : MonoBehaviour
{
    private Button button;

    [SerializeField]
    protected GameManager gameManager;
    [SerializeField]
    protected int attackId;
    [SerializeField]
    protected AttackButton[] attackButtons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(PickItem);
    }

    protected abstract void PickItem();
}
