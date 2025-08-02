namespace Assets.Scripts.Buttons
{
    public class InventoryAttackButton : InventoryButton
    {
        private void ItemPicked(AttackButton button)
        {
            button.UpdateItem(attackId);
        }

        protected override void PickItem()
        {
            foreach (var attackButton in attackButtons)
            {
                attackButton.button.onClick.AddListener(() => ItemPicked(attackButton));
            }
        }
    }
}
