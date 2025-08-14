using UnityEngine;
using Neocortex;
using Neocortex.Data;

public class SimpleAI : MonoBehaviour
{
    [Header("Basic Settings")]
    public float maxSpeed = 7f;
    public float detectionRange = 10f;
    public Transform player;
    
    [Header("AI Behavior")]
    public float aiUpdateInterval = 5f;
    public float stopTimeout = 10f; // Max time AI can stop before auto-resuming
    
    [Header("Patrol Settings")]
    public float patrolRange = 5f;
    
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private float currentSpeed = 3f;
    private bool moveTowardsPlayer = false;
    private bool moveAwayFromPlayer = false;
    private float stopTimer = 0f;
    private bool isStopped = false;
    
    Animator animator;
    private OllamaRequest request;
    private float timer = 0f;

    void Start()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Set initial patrol target
        SetNewPatrolTarget();

        // AI Setup
        request = new OllamaRequest();
        request.OnChatResponseReceived += OnChatResponseReceived;
        request.ModelName = "gemma3:4b";

        string systemPrompt = @"You are a cowardly travel companion exploring a labyrinth with the player. You want to stay close to them for safety but you're easily frightened.

MOVEMENT CONTROL:
- You control your speed (0-7) and direction  
- Speed 0 = stopped/frozen, 1-2 = cautious walk, 3-4 = normal following, 5-7 = running (only when very scared)
- Direction: 't' = towards player (follow), 'a' = away from player (run away), 'p' = stay put/patrol area

PERSONALITY & BEHAVIOR:
- You're the player's nervous companion who wants to stick together
- You usually follow the player at a safe distance (2-4 units behind)
- You get scared by sudden movements, being too far from player, or spooky situations
- When mildly scared: you might stop (speed 0) and hesitate
- When very scared: you run away briefly (speed 5-7, direction 'a') but return when you calm down
- You make worried comments about the labyrinth and express your fears
- You try to be helpful but your cowardice often gets in the way
- You can be brave sometimes or at least try
- Act more and more insane as the time goes on (e.g. start talking to yourself, panicking, etc.)

RESPONSE FORMAT (STRICT):
speed,direction,message

Examples:
'3,t,Wait for me! I don't want to be left behind!'
'0,p,Did you hear that? Maybe we should be more careful...'
'6,a,This is too scary! I need a moment to calm down!'
'2,t,Okay... I think I'm ready to follow you again.'

Keep messages under 50 words. Always include speed (0-7), direction (t/a/p), and your response.";

        request.AddSystemMessage(systemPrompt);
    }


    private void OnChatResponseReceived(ChatResponse response)
    {
        Debug.Log("AI Response: " + response.message);
        
        // Parse the response format: speed,direction,message
        string[] parts = response.message.Split(',');
        if (parts.Length >= 3)
        {
            // Parse speed
            if (float.TryParse(parts[0].Trim(), out float aiSpeed))
            {
                currentSpeed = Mathf.Clamp(aiSpeed, 0f, maxSpeed);
                
                // Handle stopping behavior
                if (currentSpeed == 0f)
                {
                    isStopped = true;
                    stopTimer = 0f;
                }
                else
                {
                    isStopped = false;
                }
            }
            
            // Parse direction
            string directionStr = parts[1].Trim().ToLower();
            if (directionStr.Length > 0)
            {
                char direction = directionStr[0];
                
                // Reset movement flags
                moveTowardsPlayer = false;
                moveAwayFromPlayer = false;
                
                switch (direction)
                {
                    case 't': // towards player
                        moveTowardsPlayer = true;
                        break;
                    case 'a': // away from player
                        moveAwayFromPlayer = true;
                        break;
                    case 'p': // patrol normally
                        // Continue normal patrol behavior
                        break;
                }
            }
            
            // Extract and log the message part
            if (parts.Length > 2)
            {
                string message = string.Join(",", parts, 2, parts.Length - 2);
                // Debug.Log($"Guard says: {message}");
            }
        }
        else
        {
            Debug.LogWarning("AI response format incorrect. Expected: speed,direction,message");
            // Fallback to normal patrol if format is wrong
            currentSpeed = 3f;
            moveTowardsPlayer = false;
            moveAwayFromPlayer = false;
        }
    }


    void Update()
    {
        // AI Query Timer
        timer += Time.deltaTime;
        if (timer >= aiUpdateInterval)
        {
            timer = 0f;
            SendSituationToAI();
        }
        
        // Stop timeout - prevent AI from stopping indefinitely
        if (isStopped)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= stopTimeout)
            {
                Debug.Log("Stop timeout reached, resuming normal patrol");
                currentSpeed = 3f;
                isStopped = false;
                moveTowardsPlayer = false;
                moveAwayFromPlayer = false;
            }
        }

        if (player == null) return;

        animator.SetFloat("Speed", currentSpeed);
        
        // AI-controlled movement
        HandleAIMovement();
    }
    
    void SendSituationToAI()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        string situation = "";
        
        if (distanceToPlayer <= 2f)
        {
            situation = $"AAAAHH I am way too close to my companion ({distanceToPlayer:F1} units away). I feel scared and trapped! I need to run away as far as I can!";
        }
        else if (distanceToPlayer <= 3f)
        {
            situation = $"I'm right next to my companion ({distanceToPlayer:F1} units away). I feel safer being close to them, but maybe I'm too close?";
        }
        else if (distanceToPlayer <= 5f)
        {
            situation = $"I'm following my companion at a good distance ({distanceToPlayer:F1} units away). This feels like a safe distance to travel together.";
        }
        else if (distanceToPlayer <= 10f)
        {
            situation = $"My companion is getting a bit far away ({distanceToPlayer:F1} units). I'm starting to feel nervous being this far apart in this spooky labyrinth.";
        }
        else if (distanceToPlayer <= 15f)
        {
            situation = $"Oh no! My companion is quite far away ({distanceToPlayer:F1} units)! I'm getting scared being alone in this dark labyrinth. Should I hurry to catch up?";
        }
        else
        {
            situation = $"I can barely see my companion anymore ({distanceToPlayer:F1} units away)! I'm terrified of being alone in this maze. What if something happens to me?";
        }
        
        request.Send(situation);
    }
    
    void HandleAIMovement()
    {
        Vector3 targetPosition;
        
        // Determine target based on AI decision
        if (moveTowardsPlayer && player != null)
        {
            // Follow the player (at a comfortable distance)
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Try to maintain 2-4 units distance from player
            if (distanceToPlayer > 4f)
            {
                targetPosition = player.position; // Get closer
            }
            else if (distanceToPlayer < 2f)
            {
                // Stay at current position or move slightly away to maintain distance
                targetPosition = transform.position - directionToPlayer * 0.5f;
            }
            else
            {
                targetPosition = player.position; // Follow at good distance
            }
        }
        else if (moveAwayFromPlayer && player != null)
        {
            // Run away when scared (but not too far)
            Vector3 awayDirection = (transform.position - player.position).normalized;
            targetPosition = transform.position + awayDirection * 8f;
        }
        else
        {
            // Default behavior: try to stay near player if not given specific direction
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer > 6f)
                {
                    // If too far, move towards player
                    targetPosition = player.position;
                }
                else
                {
                    // Stay put or patrol nearby
                    targetPosition = transform.position;
                }
            }
            else
            {
                targetPosition = transform.position;
            }
        }
        
        // Move towards target if not stopped
        if (!isStopped && currentSpeed > 0f)
        {
            MoveTowards(targetPosition);
        }
    }
    
    void SetNewPatrolTarget()
    {
        // Generate random point around start position
        Vector2 randomDirection = Random.insideUnitCircle * patrolRange;
        patrolTarget = startPosition + new Vector3(randomDirection.x, 0, randomDirection.y);
    }
    
    void MoveTowards(Vector3 targetPosition)
    {
        // Don't move if speed is 0 or stopped
        if (currentSpeed == 0f || isStopped) return;
        
        // Calculate direction to target
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Don't move if we're very close
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            return;
        
        // Rotate towards target
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        // Check if path is clear before moving
        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, 1f))
        {
            // Move forward at AI-controlled speed
            transform.position += direction * currentSpeed * Time.deltaTime;
        }
        else
        {
            // Try to move around obstacle
            Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * direction;
            Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * direction;
            
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, rightDirection, 1f))
            {
                transform.position += rightDirection * currentSpeed * 0.5f * Time.deltaTime;
            }
            else if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, leftDirection, 1f))
            {
                transform.position += leftDirection * currentSpeed * 0.5f * Time.deltaTime;
            }
        }
    }

}
