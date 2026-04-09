using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float damage = 10f;
    public float attackRate = 1.5f;

    private float nextAttackTime = 0f;
    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Automatically find the player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);



        if (distance <= attackRange)
        {
            // Make the dino face the player even when stopped
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            AttackPlayer();
        }
        else if (distance <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            StopChasing();
        }
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        anim.SetBool("isWalking", true);
    }

    void StopChasing()
    {
        agent.isStopped = true;
        anim.SetBool("isWalking", false);
    }

    // This just starts the animation
    void AttackPlayer()
    {
        agent.isStopped = true;
        anim.SetBool("isWalking", false);

        if (Time.time >= nextAttackTime)
        {
            anim.SetTrigger("attack");
            nextAttackTime = Time.time + attackRate;
        }
    }

    // This is called by the ANIMATION EVENT at the end of the swing/bite
    public void DealDamage()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Double check the player is still in range when the hit actually lands
        if (distance <= attackRange + 0.5f)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }

    // Call this at the very beginning of the attack animation
    public void StartAttack()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // Stops any sliding momentum
    }

    // Call this at the very end of the attack animation
    public void EndAttack()
    {
        agent.isStopped = false;
    }
}