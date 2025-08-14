using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    private Animator _animator;
    private static readonly int CrawlTrigger = Animator.StringToHash("Crawl Trigger");
    private static readonly int CrawlEndTrigger = Animator.StringToHash("Crawl End Trigger");
    private static readonly int WalkTrigger = Animator.StringToHash("Walk Trigger");
    private static readonly int IdleTrigger = Animator.StringToHash("Idle Trigger");
    private static readonly int RunTrigger = Animator.StringToHash("Run Trigger");
    private static readonly int CryTrigger = Animator.StringToHash("Cry Trigger");
    private bool _isCrawling = false;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        /*
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            StartIdleAnimation();
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            StartWalkAnimation();
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            StartCrawlAnimation();
        }
        */
    }

    public void StartIdleAnimation()
    {
        _animator.SetTrigger(IdleTrigger);
        if (_isCrawling)
        {
            _isCrawling = false;
            _animator.SetTrigger(CrawlEndTrigger);
        }
    }

    public void StartWalkAnimation()
    {
        //Debug.Log("Walk Trigger");
        _animator.SetTrigger(WalkTrigger);
        if (_isCrawling)
        {
            _isCrawling = false;
            _animator.SetTrigger(CrawlEndTrigger);
        }
    }

    public void StartCrawlAnimation()
    {
        _animator.SetTrigger(CrawlTrigger);
        _isCrawling = true;
    }

    public void StartRunAnimation()
    {
        _animator.SetTrigger(RunTrigger);
        if (_isCrawling)
        {
            _isCrawling = false;
            _animator.SetTrigger(CrawlEndTrigger);
        }
    }

    public void StartCryingAnimation()
    {
        _animator.SetTrigger(CryTrigger);
    }
    
    public bool IsAnimatorInState(string stateName)
    {
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName(stateName);
    }

    public void ResetAllTriggers()
    {
        _animator.ResetTrigger(WalkTrigger);
        _animator.ResetTrigger(CrawlTrigger);
        _animator.ResetTrigger(IdleTrigger);
        _animator.ResetTrigger(RunTrigger);
    }
}
