/*
	Created by @DawnosaurDev at youtube.com/c/DawnosaurStudios
    Feel free to use this in your own games, and I'd love to see anything you make!

    Adapted to Racoon Boy By @Polimthehat
 */

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{

    #region VARIABLES & REFERENCES

    //Scriptable object which holds all the player's movement parameters. If you don't want to use it
    //just paste in all the parameters, though you will need to manuly change all references in this script
    public PlayerData Data;

    #region COMPONENTS
    public Rigidbody2D rb { get; private set; }
    private Animator anim;
    private TrailRenderer tr;
    #endregion

    #region STATE PARAMETERS
    //These are fields are public, allowing for other sctipts to read them
    //but can only be privately written to.
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsBonusJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsWallSliding { get; private set; }

    //Timers
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }
    public float SlideTimeRemaining { get; private set; }

    //Jump
    private bool _isJumpCut;
    private int _bonusJumpsLeft;
    private bool _isJumpFalling;

    //Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    //Dash
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;
    #endregion

    #region INPUTSYSTEM ACTIONS
    private Vector2 _moveInput;
    PlayerInputActions playerInput;
    InputAction move;
    InputAction jump;
    InputAction dash;    

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region CHECK PARAMETERS
    [Header("Physics Checks")]
    //Ground
    //Size should be slightly smaller than the width of the character.
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);

    //Walls
    //Size should be slightly smaller than the width of the character.
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

    //Bools
    bool isGrounded;
    bool isLeftWalled;
    bool isRightWalled;
    #endregion

    #region LAYERS & TAGS

    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    #endregion

    #region INITIALIZATION
    private void Awake()
    {
        //Set the value of all references to components.
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        tr = GetComponent<TrailRenderer>();
        playerInput = new PlayerInputActions();
    }
    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
    }

    private void OnEnable()
    {
        //Enable all of the PlayerInput variables.
        move = playerInput.Player.Move;
        move.Enable();

        jump = playerInput.Player.Jump;
        jump.Enable();
        jump.performed += OnJumpInput;
        jump.canceled += OnJumpRelease;

        dash = playerInput.Player.Dash;
        dash.Enable();
        dash.performed += OnDashInput;
    }
    #endregion

    private void OnDisable()
    {
        //Disable all of the PlayerInput variables.
        //Makes it so that the engine does not recieve inputs while a UI is up, for example.
        move.Disable();
        jump.Disable();
        dash.Disable();
    }


    private void Update()
    {
        #region TIMERS
        //Decrease the time for all timers.
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        SlideTimeRemaining -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        //Read the InputAction Values for movement.
        _moveInput = move.ReadValue<Vector2>();

        if (_moveInput.x != 0)
        {
            CheckDirectionToFace(_moveInput.x > 0);
        }
        #endregion

        #region ANIMATION VARIABLE SETTING
        anim.SetBool("IsDashing", IsDashing);
        anim.SetBool("IsFalling", rb.velocity.y < 0);
        anim.SetBool("IsWallSliding", IsWallSliding);
        anim.SetBool("IsJumping", IsJumping || IsBonusJumping || IsWallJumping);
        anim.SetBool("IsMoving", _moveInput.x != 0);
        #endregion

        #region COLLISION CHECKS
        if (!IsDashing && !IsJumping && !IsBonusJumping)
        {
            //Ground Check
            isGrounded = Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer);
            //Check if Player is Grounded
            if (isGrounded && !IsJumping)
            {
                LastOnGroundTime = Data.coyoteTime; 
                _bonusJumpsLeft = Data.extraJumpAmount;
            }

            //Right Wall Physics Check
            isRightWalled = ((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight));
            if (isRightWalled && !IsWallJumping)
            {
                LastOnWallRightTime = Data.coyoteTime;
                _bonusJumpsLeft = Data.extraJumpAmount;
            }

            //Left Wall Physics Check
            isLeftWalled = ((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight));
            if (isLeftWalled && !IsWallJumping)
            {
                LastOnWallLeftTime = Data.coyoteTime;
                _bonusJumpsLeft = Data.extraJumpAmount;
            }

            //Two checks needed for both left and right walls since whenever the player turns the wall checkPoints swap sides
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);            
        }
        #endregion

        #region JUMP CHECKS
        //Checks if the player is currently falling after a jump
        if (IsJumping && rb.velocity.y < 0)
        {
            IsJumping = false;

            if (!IsWallJumping && !IsBonusJumping)
                _isJumpFalling = true;
        }

        //Checks if the player is currently falling after a bonus jump
        if (IsBonusJumping && rb.velocity.y < 0)
        {
            IsBonusJumping = false;

            if (!IsWallJumping)
                _isJumpFalling = true;
        }

        //Checks if the player is currently in a wallJump
        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            IsWallJumping = false;
        }

        //Checks if the player is on the ground while not jumping, wallJumping, or bonusJumping
        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping && !IsBonusJumping)
        {
            _isJumpCut = false;

            if (!IsJumping)
                _isJumpFalling = false;
        }

        //Checks if the player is currently Dashing, if not allows the player to use any other movement ability
        if (!IsDashing)
        {
            //Jump
            if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsBonusJumping = false;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;

                Jump();
            }
            //Wall Jump
            else if (CanWallJump() && LastPressedJumpTime > 0)
            {
                IsWallJumping = true;
                IsJumping = false;
                IsBonusJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

                WallJump(_lastWallJumpDir);
            }
            //Bonus Jump(s)
            else if (CanBonusJump() && LastPressedJumpTime > 0)
            {
                IsBonusJumping = true;
                IsJumping = false;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;

                BonusJump();
            }

            //Sets a Maximum to the y velocity to a maximum so as to limit the momentum gained from walljumping repeatedly.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, Data.maxJumpSpeed));
        }
        #endregion

        #region DASH CHECKS
        //Checks if the player is able to dash
        if (CanDash() && LastPressedDashTime > 0)
        {
            //Freeze game for split second. Adds a bit of forgiveness over directional input
            Sleep(Data.dashSleepTime);

            //If no direction is pressed, dash towards the direction the player is facing
            if (_moveInput != Vector2.zero)
                _lastDashDir = _moveInput;
            else
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;
            IsDashing = true;
            IsJumping = false;
            IsBonusJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;

            StartCoroutine(nameof(StartDash), _lastDashDir);
        }
        #endregion

        #region WALLSLIDE CHECKS
        //Checks if the player can slide and if the player is currently on a wall, while also sending the correct horizontal input to push into the wall
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)) && SlideTimeRemaining > 0)
        {
            SlideTimeRemaining = Data.maxSlideTime;
            IsWallSliding = true;
        }
        else
        {
            IsWallSliding = false;
        }
        #endregion

        #region GRAVITY
        //Removes Gravity if the player is wallSliding, allowing better control of the descent speed.
        if (IsWallSliding)
        {
            SetGravityScale(0);
        }
        //Checks if player is falling and holding the down button.
        else if (rb.velocity.y < 0 && _moveInput.y < 0)
        {
            //Raises gravity to allow for faster falling if desired.
            SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);

            //Caps maximum fall speed, so as to not accelerate too much.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFastFallSpeed));
        }
        //Checks if the player released the jump key early.
        else if (_isJumpCut)
        {
            //Increase gravity to allow for more jump control.
            SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);

            //Caps maximum fall speed, so as to not accelerate too much.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFallSpeed));
        }
        //Checks if the player is doing any Jump, or falling after a jump. 
        else if ((IsJumping || IsWallJumping || IsBonusJumping || _isJumpFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
        {
            //Sets the gravity to the correct value for a jumpHang.
            SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
        }
        //Checks if the player is falling.
        else if (rb.velocity.y < 0)
        {
            //Increase Gravity
            SetGravityScale(Data.gravityScale * Data.fallGravityMult);

            //Caps maximum fall speed, so as to not accelerate too much.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFallSpeed));
        }
        else
        {
            //Resets gravity to default if standing on a platform or moving upwards
            SetGravityScale(Data.gravityScale);
        }
        #endregion
    }

    private void FixedUpdate()
    {
        #region MOVEMENT
        //Check if the player is Dashing
        //Lerps towards the correct movement value based off of the previous player action.
        if (!IsDashing) 
        {
            //Checks if the player is WallJumping
            if (IsWallJumping)
            {
                Run(Data.wallJumpRunLerp);
            }
            else
            {
                Run(Data.defaultLerpAmount);
            }
        }
        else if (_isDashAttacking)
        {
            Run(Data.dashEndRunLerp);
        }

        //Handle WallSlide 
        if (IsWallSliding)
            WallSlide();
        #endregion
    }

    #region INPUT ACTIONS
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    public void OnJumpRelease(InputAction.CallbackContext context)
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        LastPressedDashTime = Data.dashInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    //Sets the gravityScale of the rigidbody to the parameter value.
    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }

    //Method used to avoid calling StartCoroutine everywhere
    private void Sleep(float duration)
    {        
        StartCoroutine(nameof(PerformSleep), duration);
    }

    //Used by Sleep(float) to delay code from happening.
    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        //Must be WaitForSecondsRealtime instead of WaitForSeconds since timeScale is set to 0 (so as to truly stop the clock)
        yield return new WaitForSecondsRealtime(duration); 
        Time.timeScale = 1;
    }
    #endregion

    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        //Calculate the targetVelocity. Sign determines direction.
        float targetVelocity = _moveInput.x * Data.runMaxSpeed;
        //We can increase control using Lerp() this smoothes out changes to our direction and speed
        targetVelocity = Mathf.Lerp(rb.velocity.x, targetVelocity, lerpAmount);

        #region Calculate AccelRate
        float accelRate;

        //Gets an acceleration value based on if we are accelerating or trying to decelerate. As well as applying a multiplier if we are midair.
        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
        #endregion

        #region Add Bonus Jump Apex Acceleration
        //Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
        if ((IsJumping || IsBonusJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetVelocity *= Data.jumpHangMaxSpeedMult;
        }
        #endregion

        #region Conserve Momentum
        //Don't slow the player down if they are moving in their desired direction. even while at a greater speed than their targetVelocity
        if (Data.doConserveMomentum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetVelocity) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetVelocity) && Mathf.Abs(targetVelocity) > 0.01f && LastOnGroundTime < 0)
        {
            //Prevent any deceleration from happening, or in other words conserve all current momentum
            accelRate = 0;
        }
        #endregion

        //Calculate difference between current velocity and targetVelocity
        float speedDif = targetVelocity - rb.velocity.x;

        //Calculate force along x-axis to apply to thr player
        float movement = speedDif * accelRate;

        //Convert this to a vector and apply to rigidbody
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Turn()
    {
        //Stores localScale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS
    private void Jump()
    {
        //Ensures Jump cannot be called multiple times from one input.
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        #region Perform Jump
        //Increase the force applied if the player is falling.
        float force = Data.jumpForce;
        if (rb.velocity.y < 0)
            force -= rb.velocity.y;

        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }

    private void BonusJump()
    {
        //Ensures BonusJump cannot be called multiple times from one input, and decrements the total jumps.
        LastPressedJumpTime = 0;
        _bonusJumpsLeft--;

        #region Perform Bonus Jump
        //Increase the force applied if the player is falling.
        float force = Data.jumpForce;
        if (rb.velocity.y < 0)
            force -= rb.velocity.y;

        rb.AddForce(Vector2.up * force * Data.extraJumpMult, ForceMode2D.Impulse);
        #endregion
    }

    private void WallJump(int dir)
    {
        //Ensures WallJump cannot be called multiple times from one input.
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        #region Perform Wall Jump
        Vector2 force;

        //Decides if the player is inputting any horizontal movement. If not, does a neutralWallJump as opposed to a regular one.
        if (_moveInput.x != 0)
            force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        else
            force = new Vector2(Data.neutralWallJumpForce.x, Data.neutralWallJumpForce.y);

        //Applies force in opposite direction of wall
        force.x *= dir;

        // TODO -- Player Turns around after wall jumping, correctly stopping other turning until a certain time after the jump
        //if (Data.doTurnOnWallJump)
        //    Turn();

        if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(force.x))
            force.x -= rb.velocity.x;

        //Checks whether player is falling, if so subtracts the velocity.y (counteracting force of gravity). This ensures the player always reaches the desired jump force or greater
        if (rb.velocity.y < 0) 
            force.y -= rb.velocity.y;

        //Usiing ForceMode2D.Impulse to ensure mass is considered, compared to ForceMode2D.Force.
        rb.AddForce(force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region DASH METHODS
    //Dash Coroutine
    private IEnumerator StartDash(Vector2 dir)
    {
        //Minics the Celeste-Style Dash
        LastOnGroundTime = 0;
        LastPressedDashTime = 0;
        float startTime = Time.time;
        _dashesLeft--;
        _isDashAttacking = true;
        SetGravityScale(0);
        tr.emitting = true;

        //Keeps the player's velocity at dash speed during the "attack" phase (in celeste the first 0.15s)
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            rb.velocity = dir.normalized * Data.dashSpeed;
            yield return null;
        }
        startTime = Time.time;
        _isDashAttacking = false;

        //Begins the "end" of our dash where some control is returned to the player but run acceleration is still limited
        SetGravityScale(Data.gravityScale);
        rb.velocity = Data.dashEndSpeed * dir.normalized;
        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        //Dash is over, reset global vars.
        IsDashing = false;
        tr.emitting = false;
    }

    //Cooldown timer for the Dash
    private IEnumerator RefillDash(int amount)
    {
        //Short cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
        _dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void WallSlide()
    {
        //Works the similar to Run but only in the y-axis
        float speedDif = Data.slideSpeed - rb.velocity.y;
        float movement = speedDif * Data.slideAccel;

        //Clamps the movement to prevent over corrections
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        rb.AddForce(movement * Vector2.up);
    }
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanBonusJump()
    {
        return _bonusJumpsLeft > 0 && LastOnGroundTime < -Data.coyoteTime;
    }

    private bool CanJumpCut()
    {
        return (IsBonusJumping || IsJumping) && rb.velocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && rb.velocity.y > 0;
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }
        return _dashesLeft > 0;
    }

    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion
}