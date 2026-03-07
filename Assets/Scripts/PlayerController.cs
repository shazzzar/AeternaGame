using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Animator _animator; // Assign in Inspector
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _turnspeed = 360;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private LayerMask _groundLayer; // Set to your "Ground" layer
    [SerializeField] private Transform _groundCheck; // A child object at the player's feet

    private Vector3 _input;
    private bool _isGrounded;

    private MiningSystem _miningSystem;
    private void Start()
    {
        _miningSystem = GetComponent<MiningSystem>();
    }

    private void Update()
    {
        if (_miningSystem != null && _miningSystem.isMining)
        {
            _input = Vector3.zero;
            _animator.SetFloat("Speed", 0f); // Keep animator in Idle
            return; // Stops Look() and GatherInput() from running
        }

        CheckGround();
        GatherInput();
        Look();
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

        // Names must match your Animator parameters exactly
        _animator.SetBool("isGrounded", _isGrounded);
        _animator.SetFloat("VerticalVelocity", _rb.velocity.y);
        _animator.SetFloat("Speed", _input.magnitude);
    }

    void GatherInput()
    {
        _input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // Jump Trigger
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _animator.SetTrigger("Jump");
        }
    }

    void Look()
    {
        if (_input != Vector3.zero)
        {

            var relative = (transform.position + _input.ToIso()) - transform.position;
            var rot = Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnspeed * Time.deltaTime);
        }
    }

    void Move()
    {
        _rb.MovePosition(transform.position + (transform.forward * _input.magnitude) * _speed * Time.deltaTime);
    }

    void Animate()
    {
        // Pass the movement magnitude to the Animator
        // If magnitude > 0.5, it transitions from Walk to Run in your Blend Tree
        _animator.SetFloat("Speed", _input.magnitude);

        // Vertical velocity for the "Idle Jump" (falling/rising) animation state
        _animator.SetFloat("VerticalVelocity", _rb.velocity.y);
    }
}