using UnityEngine;
using UnityEngine.UI;

public class AttackButton : MonoBehaviour
{
    private Button button;

    [SerializeField]
    private int attackId;
    [SerializeField]
    private TileSelector tileSelector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectAttack);
    }

    private void SelectAttack()
    {
        Debug.Log($"{gameObject.name} was clicked");
        tileSelector.StartAttacking(attackId);
    }
}
