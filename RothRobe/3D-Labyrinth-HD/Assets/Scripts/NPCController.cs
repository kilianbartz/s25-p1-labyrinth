using UnityEngine;
using UnityEngine.InputSystem;

public class NPCController : MonoBehaviour
{
    private AnimationController _animationController;
    private float _walkSpeed = 1f;
    private float _rotateSpeed = 50f;
    private float _stopDistance = 0.3f;
    private string _nextAnimation = "";
    
    enum MovementState
    {
        Idle,
        Rotating,
        Walking,
        Crawling,
        Running
    }

    private Vector3 _targetPosition;
    private MovementState _currentState = MovementState.Idle;

    private void Start()
    {
        _animationController = GetComponent<AnimationController>();
    }

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            _animationController.StartCryingAnimation();
        }
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            WalkTo(17,1,8);
        }
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            RunTo(9,1,8);
        }
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            CrawlTo(1,1,8);
        }
        switch (_currentState)
        {
            case MovementState.Rotating:
                RotateTowardsTarget();
                break;

            case MovementState.Walking:
                WalkTowardsTarget();
                break;
            
            case MovementState.Crawling:
                CrawlTowardsTarget();
                break;
            
            case MovementState.Running:
                RunTowardsTarget();
                break;

            // Hier kannst du später Kriechen oder Rennen einfügen
        }
    }

    public void WalkTo(float x, float y, float z)
    {
        WalkTo(new Vector3(x,y,z));
    }
    public void WalkTo(Vector3 position)
    {
        _targetPosition = position;
        _nextAnimation = "Walk";
        _currentState = MovementState.Rotating;
    }


    public void CrawlTo(float x, float y, float z)
    {
        CrawlTo(new Vector3(x,y,z));
    }
    public void CrawlTo(Vector3 position)
    {
        _targetPosition = position;
        _nextAnimation = "Crawl";
        _currentState = MovementState.Rotating;
    }

    public void RunTo(float x, float y, float z)
    {
        RunTo(new Vector3(x,y,z));
    }

    public void RunTo(Vector3 position)
    {
        _targetPosition = position;
        _nextAnimation = "Run";
        _currentState = MovementState.Rotating;
    }

    public void StartCrying()
    {
        _animationController.StartCryingAnimation();
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0;
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotateSpeed * Time.deltaTime);

        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        if (angleDifference < 1f)
        {
            switch (_nextAnimation)
            {
                case "Walk":
                    _animationController.StartWalkAnimation();
            
                    if (_animationController.IsAnimatorInState("MCU_am_OrcHammer_Loco_Walk_Fwd_NoRM"))
                    {
                        _currentState = MovementState.Walking;
                    }
                    break;
                case "Crawl":
                    _animationController.StartCrawlAnimation();
                    _currentState = MovementState.Crawling;
                    break;
                case "Run":
                    _animationController.StartRunAnimation();
            
                    if (_animationController.IsAnimatorInState("Run"))
                    {
                        _currentState = MovementState.Running;
                    }
                    break;
            }
        }
    }


    void WalkTowardsTarget()
    {
        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;

        if (distance > _stopDistance)
        {
            transform.position += transform.forward * (_walkSpeed * Time.deltaTime);
        }
        else
        {
            _currentState = MovementState.Idle;
            _animationController.ResetAllTriggers();
            _animationController.StartIdleAnimation();
        }
    }

    void CrawlTowardsTarget()
    {
        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;

        if (distance < .6f)
        {
            _currentState = MovementState.Idle;
            _animationController.ResetAllTriggers();
            _animationController.StartIdleAnimation();
        }
    }
    
    void RunTowardsTarget()
    {
        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;

        if (distance > _stopDistance)
        {
            transform.position += transform.forward * (_walkSpeed * 2.5f * Time.deltaTime);
        }
        else
        {
            _currentState = MovementState.Idle;
            _animationController.ResetAllTriggers();
            _animationController.StartIdleAnimation();
        }
    }
}
