using UnityEngine;
using UnityEngine.UI;

public class ConfirmButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.interactable = false;
        button.onClick.AddListener(InventorySystem.Instance.OnClickConfirm);
    }

    // Update is called once per frame
    void Update()
    {
        button.interactable = InventorySystem.Instance.AreAllItemsSelected;
    }
}
