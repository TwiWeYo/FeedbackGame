using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AttackButton : MonoBehaviour
{
    public Button button;

    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private bool isItem;

    [SerializeField]
    private int attackId;

    private bool isUsed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectAttack);
        gameManager.gameGrid.PlayerManager.OnPlayerMoved += q => isUsed = isItem && q.Any();
    }

    public void UpdateItem(int attackId)
    {
        isUsed = false;
        this.attackId = attackId;
    }

    private void SelectAttack()
    {
        if (isUsed)
            return;

        gameManager.StartAttacking(attackId);
    }
}
