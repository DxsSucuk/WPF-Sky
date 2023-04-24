public class ShipEnter : Interactable
{
    public ShipMovement shipMovement;

    public override void InteractWithInteractable()
    {
        base.InteractWithInteractable();
        if (photonView.IsMine)
        {
            shipMovement.ShipEntering();
        }
    }
}