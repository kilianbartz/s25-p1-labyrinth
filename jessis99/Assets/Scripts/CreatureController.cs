using UnityEngine;

public class CreatureController : MonoBehaviour
{
    [SerializeField]
    private Controller.CreatureMover creature;
    [SerializeField]
    private GameObject player;

    public void Update(){
        //MoveTo(player.transform.position, true);
    }

    public void MoveTo(Vector3 target, bool run = false)
    {
        Vector3 direction = (target - creature.transform.position).normalized;
        Vector2 axis = new Vector2(direction.x, direction.z);

        creature.SetInput(axis, target, run, false);
    }

    public void FleeFrom(Vector3 source, bool run = true)
    {
        Vector3 direction = (creature.transform.position - source).normalized;
        Vector2 axis = new Vector2(direction.x, direction.z);

        creature.SetInput(axis, source, run, false);
    }

    public void Stop()
    {
        creature.SetInput(Vector2.zero, creature.transform.position, false, false);
    }

    public void LookAt(Vector3 target)
    {
        // Just update the target position - the LookAt IK is handled internally
        creature.SetInput(Vector2.zero, target, false, false);
    }
}
