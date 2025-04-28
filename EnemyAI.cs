using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private Transform[] patrolPoints;
    
    [Header("Combat")]
    [SerializeField] private int damage = 1;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    // References
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private BehaviorTree behaviorTree;
    
    // State variables
    private int currentPatrolIndex = 0;
    private bool isFacingRight = true;
    private bool canAttack = true;
    private bool isStunned = false;
    private EnemyState currentState;

    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Stunned,
        Retreat
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Initialize behavior tree
        SetupBehaviorTree();
    }

    private void Update()
    {
        if (isStunned)
            return;
            
        // Update behavior tree
        behaviorTree.Update();
        
        // Update animations based on state
        UpdateAnimations();
    }

    private void SetupBehaviorTree()
    {
        // Create behavior tree
        behaviorTree = new BehaviorTree();
        
        // Create root selector node
        BehaviorNode rootNode = new SelectorNode();
        
        // Add child nodes to root
        rootNode.AddChild(CreateStunnedSequence());
        rootNode.AddChild(CreateAttackSequence());
        rootNode.AddChild(CreateChaseSequence());
        rootNode.AddChild(CreatePatrolSequence());
        
        // Set root node
        behaviorTree.SetRootNode(rootNode);
    }

    private BehaviorNode CreateStunnedSequence()
    {
        SequenceNode stunSequence = new SequenceNode();
        
        // Check if stunned
        stunSequence.AddChild(new CheckBoolNode(() => isStunned));
        
        // Wait until stun is over
        stunSequence.AddChild(new ActionNode(() => {
            currentState = EnemyState.Stunned;
            rb.velocity = Vector2.zero;
            return BehaviorNodeStatus.Running;
        }));
        
        return stunSequence;
    }

    private BehaviorNode CreateAttackSequence()
    {
        SequenceNode attackSequence = new SequenceNode();
        
        // Check if player is in attack range
        attackSequence.AddChild(new CheckDistanceNode(() => Vector2.Distance(transform.position, player.position), 0, attackRange));
        
        // Check if can attack
        attackSequence.AddChild(new CheckBoolNode(() => canAttack));
        
        // Perform attack
        attackSequence.AddChild(new ActionNode(() => {
            currentState = EnemyState.Attack;
            StartCoroutine(PerformAttack());
            return BehaviorNodeStatus.Success;
        }));
        
        return attackSequence;
    }

    private BehaviorNode CreateChaseSequence()
    {
        SequenceNode chaseSequence = new SequenceNode();
        
        // Check if player is in detection range
        chaseSequence.AddChild(new CheckDistanceNode(() => Vector2.Distance(transform.position, player.position), 0, detectionRange));
        
        // Chase player
        chaseSequence.AddChild(new ActionNode(() => {
            currentState = EnemyState.Chase;
            ChasePlayer();
            return BehaviorNodeStatus.Running;
        }));
        
        return chaseSequence;
    }

    private BehaviorNode CreatePatrolSequence()
    {
        SequenceNode patrolSequence = new SequenceNode();
        
        // Always true - fallback behavior
        patrolSequence.AddChild(new ActionNode(() => {
            currentState = EnemyState.Patrol;
            Patrol();
            return BehaviorNodeStatus.Running;
        }));
        
        return patrolSequence;
    }

    private void ChasePlayer()
    {
        if (patrolPoints.Length == 0)
            return;
            
        // Set chase speed
        moveSpeed = chaseSpeed;
        
        // Move towards player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        
        // Flip if needed
        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
        {
            Flip();
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;
            
        // Set patrol speed
        moveSpeed = patrolSpeed;
        
        // Get current patrol point
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        
        // Move towards patrol point
        Vector2 direction = (targetPoint.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        
        // Flip if needed
        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
        {
            Flip();
        }
        
        // Check if reached patrol point
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private IEnumerator PerformAttack()
    {
        // Stop movement
        rb.velocity = Vector2.zero;
        
        // Set attack animation
        animator.SetTrigger("Attack");
        
        // Wait for animation to play
        yield return new WaitForSeconds(0.3f); // Adjust based on animation timing
        
        // Check for player in attack radius
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (hitPlayer != null)
        {
            // Deal damage to player
            hitPlayer.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
        
        // Start attack cooldown
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeStun()
    {
        if (isStunned)
            return;
            
        StartCoroutine(StunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        isStunned = true;
        animator.SetBool("IsStunned", true);
        rb.velocity = Vector2.zero;
        
        yield return new WaitForSeconds(stunDuration);
        
        isStunned = false;
        animator.SetBool("IsStunned", false);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void UpdateAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("IsChasing", currentState == EnemyState.Chase);
        animator.SetBool("IsAttacking", currentState == EnemyState.Attack);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Visualize attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Visualize attack point
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
