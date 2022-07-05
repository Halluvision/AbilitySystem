using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEvent : MonoBehaviour
{
    public delegate void OnLand();
    public OnLand onLand;
    public delegate void OnDetach();
    public OnLand onDetach;

    [SerializeField]
    LayerMask groundLayer;

    private CapsuleCollider _capsuleCollider;
    private Vector3 _contactPosition;
    private Vector3 _halfExtent = new Vector3(0.1f, 0.05f, 0.1f);
    
    [SerializeField]
    private bool _isGrounded;
    public bool Grounded { get { return _isGrounded; } }

    private void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        RaycastGround();
    }

    private void RaycastGround()
    {
        _contactPosition = new Vector3(_capsuleCollider.bounds.center.x, _capsuleCollider.bounds.min.y + 0.01f, _capsuleCollider.bounds.center.z);
        bool grounded = Physics.Raycast(_contactPosition, Vector3.down, 0.05f, groundLayer);
        Debug.DrawLine(_contactPosition, _contactPosition + Vector3.down, Color.red);
        if (_isGrounded != grounded)
        {
            if (grounded)
            {
                _isGrounded = true;
                onLand?.Invoke();
            }
            else
            {
                _isGrounded = false;
                onDetach?.Invoke();
            }
        }
        else
            _isGrounded = grounded;
    }
}
