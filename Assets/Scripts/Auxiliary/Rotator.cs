using UnityEngine;

public sealed class Rotator : MonoBehaviour, IPauseHandler
{
    [SerializeField] private Vector3 rotation;
    private bool gamePaused;

    private void Start()
    {
        var sessionMaster = SessionMaster.current;
        gamePaused = sessionMaster.paused;
        sessionMaster.onPauseStart += this.OnPauseStart;
        sessionMaster.onPauseEnd += this.OnPauseEnd;
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
        }
    }
}
