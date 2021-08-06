using UnityEngine;

public sealed class RombRotator : MonoBehaviour, IPauseHandler
{
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float jumpHeight = 0.3f, jumpTime = 0.5f;
    private bool gamePaused;
    private float savedHeight;

    private void Start()
    {
        var sessionMaster = SessionMaster.current;
        gamePaused = sessionMaster.paused;
        sessionMaster.onPauseStart += this.OnPauseStart;
        sessionMaster.onPauseEnd += this.OnPauseEnd;
        savedHeight = transform.position.y;
    }

    public void OnPauseEnd()
    {
        gamePaused = false;
    }

    public void OnPauseStart()
    {
        gamePaused = true;
    }

    void Update()
    {
        if (!gamePaused)
        {
            transform.Rotate(rotation * Time.deltaTime, Space.Self);
            if (jumpTime != 0f)  transform.position = new Vector3(transform.position.x, savedHeight + (Mathf.Sin(Time.time / jumpTime) + 1)  *jumpHeight * 0.5f, transform.position.z); 
            
        }
    }
}
