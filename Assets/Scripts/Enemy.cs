using System.Collections;
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

    [Header("Stats")]
    public float health = 50f;

    [Header("Visuals")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _flashMaterial; // Drag your white/bright material here
    [SerializeField] private float _flashDuration = 0.1f;

    private Material _originalMaterial;
    private Material[] _originalMaterials;
    private Material[] _flashMaterials;
    private Coroutine _flashRoutine;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else
            {
                PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
                if (pc != null) player = pc.transform;
            }
        }

        agent = GetComponent<NavMeshAgent>();
anim = GetComponent<Animator>();

        if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();

        if (_renderer != null)
        {
            // 1. Store all 4 original materials
            _originalMaterials = _renderer.materials;

            // 2. Create an array of 4 flash materials to match
            _flashMaterials = new Material[_originalMaterials.Length];
            for (int i = 0; i < _flashMaterials.Length; i++)
            {
                _flashMaterials[i] = _flashMaterial;
            }
        }
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

    public void TakeDamage(float amount)
    {
        health -= amount;

        StopAllCoroutines();
        _flashRoutine = StartCoroutine(FlashRoutine());

        if (health <= 0) Destroy(gameObject);
    }

    private IEnumerator FlashRoutine()
    {
        if (_renderer == null || _flashMaterial == null) yield break;

        // 3. Swap the entire array to flash
        _renderer.materials = _flashMaterials;

        yield return new WaitForSeconds(_flashDuration);

        // 4. Swap back to the original array
        _renderer.materials = _originalMaterials;

        _flashRoutine = null;
    }
}