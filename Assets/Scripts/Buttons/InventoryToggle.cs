using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryToggle : MonoBehaviour
{   
    private int attackId;

    [SerializeField]
    public bool isItemButton;
    [SerializeField]
    private TextMeshProUGUI attackNameText;
    [SerializeField]
    private Image attackIcon;
    [SerializeField]
    private Toggle buttonComponent;

    [Header("UI ��� ��������� ������")]
    public GameObject selectedOverlay; // ������, ������� ������������, ���� ����� ������ ��� ����������


    void Start()
    {
        if (buttonComponent == null)
        {
            buttonComponent = GetComponent<Toggle>();
        }

        buttonComponent?.onValueChanged.AddListener(OnButtonClick);
    }
    private void OnButtonClick(bool toggle)
    {
        SetToggleState(true, toggle);
        InventorySystem.Instance.OnClickBuyButton(attackId, this, toggle);
    }

    public void Setup(AttackType attack)
    {
        if (attack != null)
        {
            attackId = attack.Id;
            attackNameText.text = attack.AttackName;
            attackIcon.sprite = attack.AttackIcon;
        }
        else
        {
            Debug.LogError("SkillData ��� ID " + attackId + " �� �������.");
            SetToggleState(false, false);
        }
    }

    public void SetToggleState(bool isEnabled, bool isToggled)
    {
        buttonComponent.interactable = isEnabled;
        buttonComponent.isOn = isToggled;
        selectedOverlay.SetActive(isToggled);
    }
}
