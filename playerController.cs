using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForce = 14f;
    [SerializeField] private float doubleJumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.5f;

    // References
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // State variables
    private float horizontalInput;
    private bool isGrounded;
    private bool isWallSliding;
    private bool isFacingRight = true;
    private bool canDoubleJump;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Input handling
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Jump input with buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Check if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Coyote time implementation
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Wall sliding check
        isWallSliding = CheckWallSlide();

        // Jump logic
        if (jumpBufferCounter > 0)
        {
            // Normal jump with coyote time
            if (coyoteTimeCounter > 0)
            {
                Jump(jumpForce);
                jumpBufferCounter = 0;
            }
            // Wall jump
            else if (isWallSliding)
            {
                WallJump();
                jumpBufferCounter = 0;
            }
            // Double jump
            else if (canDoubleJump && !isGrounded)
            {
                Jump(doubleJumpForce);
                canDoubleJump = false;
                jumpBufferCounter = 0;
            }
        }

        // Cut jump height if button released
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Flip character based on direction
        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }

        // Update animations
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Handle movement
        if (!isWallSliding)
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }
        else
        {
            // Wall sliding
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
    }

    private void Jump(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, force);
        isJumping = true;
        
        // Play jump sound
        AudioManager.Instance.PlaySound("jump");
        
        // Trigger jump VFX
        ParticleSystem jumpVFX = PoolManager.Instance.GetPooledObject("JumpVFX").GetComponent<ParticleSystem>();
        if (jumpVFX != null)
        {
            jumpVFX.transform.position = groundCheck.position;
            jumpVFX.Play();
        }
    }

    private void WallJump()
    {
        // Jump away from wall
        float direction = isFacingRight ? -1f : 1f;
        rb.velocity = new Vector2(direction * wallJumpForce, jumpForce);
        
        // Flip character
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
        {
            Flip();
        }
        
        // Play wall jump sound
        AudioManager.Instance.PlaySound("wallJump");
    }

    private bool CheckWallSlide()
    {
        if (!isGrounded && Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, wallLayer))
        {
            return true;
        }
        return false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void UpdateAnimations()
    {
        // Set animation parameters
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsJumping", isJumping && rb.velocity.y > 0);
        animator.SetBool("IsFalling", rb.velocity.y < -0.1f);
        animator.SetBool("IsWallSliding", isWallSliding);
        
        // Reset jumping state when landing
        if (isGrounded && isJumping)
        {
            isJumping = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        
        // Visualize wall check
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + transform.right * wallCheckDistance);
    }
}
