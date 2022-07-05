using UnityEngine;
using Halluvision.AbilitySystem;

[CreateAssetMenu(menuName = "Halluvision/Ability/MoveAbility")]
public class MoveAbility : Ability
{
    [SerializeField]
    private float _speed = 6f;
    [SerializeField]
    private float _turnSpeed = 3f;

    private float _vertical;
    private float _horizontal;
    private float _turn;
    private Vector3 _direction;

    public override void Update()
    {
        _vertical = Input.GetAxis("Vertical");
        _horizontal = Input.GetAxis("Horizontal");
        _turn = Input.GetAxis("Mouse X");

        _transform.Rotate(0f, _turn * _turnSpeed, 0f);

        _direction = _transform.forward * _vertical + _transform.right * _horizontal;

        if (_direction != Vector3.zero)
            RunAbility();
        else
            FinishAbility();
    }

    public override void FixedUpdate()
    {
        if (AbilityIsRunning)
        {
            _rigidbody.velocity = new Vector3(_direction.x * _speed, _rigidbody.velocity.y, _direction.z * _speed);
        }
    }
}
