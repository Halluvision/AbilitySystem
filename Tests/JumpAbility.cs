using UnityEngine;
using Halluvision.AbilitySystem;

[CreateAssetMenu(menuName = "Halluvision/Ability/JumpAbility")]
public class JumpAbility : Ability
{
    [SerializeField]
    private float _jumpForce = 10f;

    private CollisionEvent _collisionEvent;

    protected override void OnInitialized()
    {
        _collisionEvent = _ownerSystem.GetComponent<CollisionEvent>();
        _collisionEvent.onLand += OnLand;
        _collisionEvent.onDetach += OnDetach;
    }

    protected override void RegisterInput()
    {
        InputManager.InputActions.Gameplay.Jump.performed += BufferInput;
    }

    protected override bool CheckAbilityConditions()
    {
        return _collisionEvent.Grounded;
    }

    protected override void AbilityStarted()
    {
        _rigidbody.AddForce(new Vector3(0f, _jumpForce, 0f), ForceMode.Impulse);
    }

    private void OnLand()
    {
        // Runs when character lands on ground
        FinishAbility();
    }

    private void OnDetach()
    {
        // Runs when character jumps or move off ledge
    }

    private void OnDestroy()
    {
        if (_collisionEvent != null)
        {
            _collisionEvent.onLand -= OnLand;
            _collisionEvent.onDetach -= OnDetach;
        }
    }
}
