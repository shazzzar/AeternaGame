using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _turnspeed = 360;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject _weapon;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _aimRotationSpeed = 15f;
    [SerializeField] private float _shootDistance = 100f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _fireRate = 0.5f; // Time between shots

    [Header("UI & Cursor")]
    [SerializeField] private Texture2D _cursorTexture;

    [Header("VFX")]
    [SerializeField] private Transform _gunTip;
    [SerializeField] private Material _tracerMaterial; // Create a simple white 'Unlit' material for this

    private Vector3 _input;
    private bool _isGrounded;
    private bool _hasGun = false;
    private float _nextFireTime = 0f;

    private MiningSystem _miningSystem;

    private void Start()
    {
        _miningSystem = GetComponent<MiningSystem>();

        // Set initial cursor state
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        // ... (Keep your mining check and input gathering)

        CheckGround();
        GatherInput();

        // LOOK LOGIC:
        // If holding Fire and has a gun, snap to target. 
        // Otherwise, use normal movement rotation.
        if (_hasGun && Input.GetMouseButton(0))
        {
            RotateToMouse();
        }
        else
        {
            Look();
        }

        HandleWeaponToggle();
        HandleShooting();
        Animate();
    }

    private void FixedUpdate()
    {
        if (_miningSystem != null && _miningSystem.isMining) return;
        Move();
    }

    void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, 0.2f, _groundLayer);
        _animator.SetBool("isGrounded", _isGrounded);
    }

    void GatherInput()
    {
        _input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _animator.SetTrigger("Jump");
        }
    }

    void Move()
    {
        // Convert your input (WASD) to the Iso perspective
        Vector3 moveDirection = _input.ToIso();

        // Move based on the input direction, NOT where the character is facing
        if (moveDirection.magnitude > 0)
        {
            _rb.MovePosition(transform.position + moveDirection * _speed * Time.deltaTime);
        }
    }

    void Look()
    {
        // Standard movement rotation: face the direction we are walking
        if (_input != Vector3.zero)
        {
            var relative = (transform.position + _input.ToIso()) - transform.position;
            var rot = Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnspeed * Time.deltaTime);
        }
    }

    void RotateToMouse()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 direction = (point - transform.position);
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                // Use Quaternion.RotateTowards for a very fast but clean snap
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnspeed * 2 * Time.deltaTime);
            }
        }
    }

    void Animate()
    {
        _animator.SetFloat("Speed", _input.magnitude);
        _animator.SetFloat("VerticalVelocity", _rb.linearVelocity.y);
        _animator.SetBool("HasGun", _hasGun);
    }

    void HandleWeaponToggle()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _hasGun = !_hasGun;
            _animator.SetBool("HasGun", _hasGun);

            if (_hasGun)
            {
                Cursor.SetCursor(_cursorTexture, new Vector2(_cursorTexture.width / 2, _cursorTexture.height / 2), CursorMode.Auto);
                Cursor.visible = true;
            }
            else
            {
                // Reset to default system cursor or hide it
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                Cursor.visible = false;
            }

            if (_weapon != null) _weapon.SetActive(_hasGun);
        }
    }

    void HandleShooting()
    {
        if (!_hasGun) return;

        // Auto-fire check with cooldown
        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;

            // Speed up animation based on fire rate
            float animSpeedMultiplier = 1f / _fireRate;
            _animator.SetFloat("ShootSpeed", animSpeedMultiplier);

            _animator.SetTrigger("Shoot");
            ShootRayVisual();
        }
    }

    void ShootRayVisual()
    {
        // 1. Create a ray from the camera through the mouse cursor
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        Vector3 origin = _gunTip != null ? _gunTip.position : transform.position;
        Vector3 endPoint;

        // 2. Fire the raycast. This hits whatever is directly under the cursor.
        if (Physics.Raycast(ray, out RaycastHit hit, _shootDistance))
        {
            endPoint = hit.point;

            // 3. Damage logic
            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
                Debug.Log("Direct hit on: " + hit.collider.name);
            }
        }
        else
        {
            // If we hit nothing, the bullet just goes forward
            endPoint = ray.GetPoint(_shootDistance);
        }

        // 4. Visual tracer (starts at gun, ends at what the mouse is over)
        StartCoroutine(SpawnTracer(origin, endPoint));
    }

    IEnumerator SpawnTracer(Vector3 start, Vector3 end)
    {
        // 1. Create a new object and add the LineRenderer
        GameObject tracerObj = new GameObject("BulletTracer");
        LineRenderer lr = tracerObj.AddComponent<LineRenderer>();

        // 2. Setup the look
        lr.material = _tracerMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.02f; // Taper the end slightly for a "streak" look
        lr.positionCount = 2;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        // 3. Animate the tracer "flying"
        float t = 0;
        float speed = 15f; // Adjust this for how fast the tracer "travels"

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            lr.SetPosition(0, start); // Start stays at the gun
            lr.SetPosition(1, Vector3.Lerp(start, end, t)); // End stretches toward target
            yield return null;
        }

        // 4. Brief pause at full length then vanish
        Destroy(tracerObj, 0.05f);
    }
}