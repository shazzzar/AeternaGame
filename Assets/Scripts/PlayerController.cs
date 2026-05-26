using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static bool IsWeaponEquipped = false;

    [Header("Movement")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Animator _animator;
    public float speed = 5f;
    [SerializeField] private float _turnspeed = 360f;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject _weapon;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _aimRotationSpeed = 15f;
    [SerializeField] private float _shootDistance = 100f;
    public float damage = 10f;
    public float fireRate = 0.5f;
[SerializeField] private LayerMask _shootMask = ~0;

    [Header("UI & Cursor")]
    [SerializeField] private Texture2D _cursorTexture;

    [Header("VFX")]
    [SerializeField] private Transform _gunTip;
    [SerializeField] private Material _tracerMaterial;

    [Header("Audio")]
    public AudioClip walkingSound;
    public AudioClip shootSound;
    [SerializeField] private float _stepInterval = 0.5f;
private AudioSource _audioSource;
    private float _stepTimer;

    private Vector3 _input;
private bool _isGrounded;
    private bool _hasGun = false;
    private float _nextFireTime = 0f;

    private MiningSystem _miningSystem;

    private void Start()
    {
        _miningSystem = GetComponent<MiningSystem>();

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        if (PlayerStats.Instance != null)
{
            speed = PlayerStats.Instance.speed;
            damage = PlayerStats.Instance.damage;
        }

        IsWeaponEquipped = _hasGun;

        if (_weapon != null)
            _weapon.SetActive(_hasGun);

        UpdateCursorState();
    }

    private void Update()
    {
        CheckGround();
        GatherInput();

        HandleWeaponToggle();

        bool isUIOpen = InventorySlide.IsInventoryOpen || (RoundManager.Instance != null && RoundManager.Instance.isShopPhase) || PauseMenu.IsPaused;

        if (_hasGun && !isUIOpen)
        {
            ApplyWeaponCursor();

            if (Input.GetMouseButton(0))
            {
                RotateToMouse();
            }
            else
            {
                Look();
            }

            HandleShooting();
        }
        else
        {
            if (!isUIOpen)
            {
                UpdateCursorState();
            }
            else
            {
                UpdateCursorState();
            }

            Look();
        }

        Animate();
        HandleFootsteps();
        }

        private void HandleFootsteps()
        {
            if (_isGrounded && _input.magnitude > 0.1f && !(_miningSystem != null && _miningSystem.isMining))
            {
                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    if (walkingSound != null && _audioSource != null)
                    {
                        _audioSource.PlayOneShot(walkingSound);
                    }
                    _stepTimer = _stepInterval;
                }
            }
            else
            {
                _stepTimer = 0f; // Reset so the first step of a new walk plays immediately
            }
        }

        private void FixedUpdate()
{
        if (_miningSystem != null && _miningSystem.isMining) return;
        Move();
    }

    void CheckGround()
    {
        if (_groundCheck == null) return;

        _isGrounded = Physics.CheckSphere(_groundCheck.position, 0.2f, _groundLayer);

        if (_animator != null)
            _animator.SetBool("isGrounded", _isGrounded);
    }

    void GatherInput()
    {
        _input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

            if (_animator != null)
                _animator.SetTrigger("Jump");
        }
    }

    void Move()
    {
        Vector3 moveDirection = _input.ToIso();

        if (moveDirection.magnitude > 0)
        {
            _rb.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);
        }
        }

        void Look()
        {
        if (_input != Vector3.zero)
        {
            Vector3 relative = (transform.position + _input.ToIso()) - transform.position;
            Quaternion rot = Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rot,
                _turnspeed * Time.deltaTime
            );
        }
        }

        void RotateToMouse()
        {
        if (_camera == null) return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 direction = point - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _aimRotationSpeed * 100f * Time.deltaTime
                );
            }
        }
        }

        void Animate()
        {
        if (_animator == null) return;

        _animator.SetFloat("Speed", _input.magnitude);
        _animator.SetFloat("VerticalVelocity", _rb.linearVelocity.y);
        _animator.SetBool("HasGun", _hasGun);
        }

        void HandleWeaponToggle()
        {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (!InventoryManager.Instance.HasWeapon())
            {
                Debug.Log("No weapon in inventory!");
                return;
            }

            _hasGun = !_hasGun;
            IsWeaponEquipped = _hasGun;

            if (_weapon != null)
                _weapon.SetActive(_hasGun);

            if (_animator != null)
                _animator.SetBool("HasGun", _hasGun);

            UpdateCursorState();
        }
        }

        void HandleShooting()
        {
        if (!_hasGun) return;
        if (InventorySlide.IsInventoryOpen) return;

        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + fireRate;

            float animSpeedMultiplier = 1f / fireRate;

            if (_animator != null)
            {
                _animator.SetFloat("ShootSpeed", animSpeedMultiplier);
                _animator.SetTrigger("Shoot");
            }

            if (shootSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(shootSound);
            }

            ShootRayVisual();
        }
        }

        void ShootRayVisual()
        {
        if (_camera == null) return;

        Ray cameraRay = _camera.ScreenPointToRay(Input.mousePosition);

        Vector3 origin = _gunTip != null ? _gunTip.position : transform.position + Vector3.up;
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, _shootDistance, _shootMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(cameraRay, out float distance))
            {
                targetPoint = cameraRay.GetPoint(distance);
            }
            else
            {
                targetPoint = cameraRay.GetPoint(_shootDistance);
            }
        }

        Vector3 shootDirection = (targetPoint - origin).normalized;

        if (Physics.Raycast(origin, shootDirection, out RaycastHit hit, _shootDistance, _shootMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;

            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Direct hit on: " + hit.collider.name);
            }
        }
        else
        {
            targetPoint = origin + shootDirection * _shootDistance;
        }

        StartCoroutine(SpawnTracer(origin, targetPoint));
    }

    IEnumerator SpawnTracer(Vector3 start, Vector3 end)
    {
        GameObject tracerObj = new GameObject("BulletTracer");
        LineRenderer lr = tracerObj.AddComponent<LineRenderer>();

        lr.material = _tracerMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.02f;
        lr.positionCount = 2;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        float t = 0f;
        float speed = 15f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            lr.SetPosition(0, start);
            lr.SetPosition(1, Vector3.Lerp(start, end, t));
            yield return null;
        }

        Destroy(tracerObj, 0.05f);
    }

    void ApplyWeaponCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_cursorTexture != null)
        {
            Cursor.SetCursor(
                _cursorTexture,
                new Vector2(_cursorTexture.width / 2f, _cursorTexture.height / 2f),
                CursorMode.Auto
            );
        }
    }

    public void UpdateCursorState()
    {
        bool isInRoundPauseScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Round_Pause";

        if (PauseMenu.IsPaused || isInRoundPauseScene || (RoundManager.Instance != null && RoundManager.Instance.isShopPhase))
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (InventorySlide.IsInventoryOpen)
{
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (_hasGun)
        {
            ApplyWeaponCursor();
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}