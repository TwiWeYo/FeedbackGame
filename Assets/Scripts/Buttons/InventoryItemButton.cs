namespace Assets.Scripts.Buttons
{
    public class InventoryItemButton : InventoryButton
    {
        private int counter = 0;
        protected override void PickItem()
        {
            if (counter == attackButtons.Length - 1)
            {
                return;
            }

            attackButtons[counter].UpdateItem(attackId);

            counter++;
        }
    }
}
