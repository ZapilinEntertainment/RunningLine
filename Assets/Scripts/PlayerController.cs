using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public sealed class PlayerController : MonoBehaviour, IPauseHandler
{
    [SerializeField] private float FORMATION_CHANGE_SPEED = 5f, FORMATION_WIDTH = 10f, FORMATION_LENGTH = 6f;
    [SerializeField] private int START_GUYS = 10, MAX_GUYS = 50;
    [SerializeField] private GameObject guyPref;
    [SerializeField] private SplineFollower splineFollower;

    private bool guysRelocation = false, gamePaused = false, gameStarted = false;
    private Transform cameraTransform;
    private Vector3 lookVector = new Vector3(0f, 6f, -15f);
    private BitArray aliveMap;
    private SmallGuy[] guys;
    private Vector2[] newGuysPositions = new Vector2[0]; // в формате от 0 до 1
    private List<SmallGuy> disabledGuysPool, injuredGuysPool;

    private void Awake()
    {
        var sessionMaster = SessionMaster.current;
        sessionMaster.AssignPlayerController(this);
        cameraTransform = Camera.main.transform;
        disabledGuysPool = new List<SmallGuy>();
        injuredGuysPool = new List<SmallGuy>();
        //
        gamePaused = sessionMaster.paused;
        sessionMaster.onPauseStart += this.OnPauseStart;
        sessionMaster.onPauseEnd += this.OnPauseEnd;
    }

    private SmallGuy GetGuy()
    {
        if (disabledGuysPool.Count > 0)
        {
            int k = disabledGuysPool.Count - 1;
            var g = disabledGuysPool[k];
            g.Restore();
            disabledGuysPool.RemoveAt(k);
            g.transform.parent = transform;
            g.transform.localRotation = Quaternion.identity;
            g.gameObject.SetActive(true);
            var rg = g.GetComponent<Rigidbody>();
            rg.isKinematic = true;
            rg.useGravity = false;
            rg.velocity = Vector3.zero;
            g.RestartAnimator();
            return g;
        }
        else
        {
            var g = Instantiate(guyPref);
            var t = g.transform;
            t.parent = transform;
            t.localRotation = Quaternion.identity;
            return g.GetComponent<SmallGuy>();
        }
    }

    void Start()
    {
        PrepareStartRow();
        splineFollower.onEndReached += this.Finish;
    }
    private void PrepareStartRow()
    {
        SmallGuy g;
        guys = new SmallGuy[START_GUYS];
        aliveMap = new BitArray(START_GUYS, true);
        float p = 1f / START_GUYS;
        Vector3 zeroPos = Vector3.left * (START_GUYS / 2f) * p, vx;
        if (START_GUYS % 2 == 0) zeroPos.x += p / 2f;

        for (int i = 0; i < START_GUYS; i++)
        {
            g = GetGuy();
            g.ChangeAnimationMode(GuyAnimationMode.Idle);
            vx = zeroPos + Vector3.right * p * i;
            vx.x *= FORMATION_WIDTH; vx.y = SmallGuy.HEIGHT; vx.z *= FORMATION_LENGTH;
            g.transform.localPosition = vx;
            g.transform.localRotation = Quaternion.identity;
            guys[i] = g;
        }
    }

    private void Update()
    {
        if (gamePaused | !gameStarted) return;

        if (guysRelocation)
        {
            int count = guys.Length, positionsCount = newGuysPositions.Length;
            if (count > positionsCount) // установка позиций для новоприбывших
            {
                int neededPositionsCount = count - positionsCount;
                AddNewPositions(neededPositionsCount);
                //DenseCrowd();
                if (neededPositionsCount > positionsCount - 1)
                { // не все смогли уплотниться - ставим в хвост
                    int readyPositionsCount = 2 * positionsCount - 1; // количество уже установленных позиций
                    Vector2 lastPosition = newGuysPositions[readyPositionsCount - 1]; // позиция последнего в цепочке
                    for (int i = readyPositionsCount; i < newGuysPositions.Length; i++)
                    {
                        newGuysPositions[i] = lastPosition + Vector2.down * i * 0.02f;
                    }
                }
            }

            void AddNewPositions(int x)
            {
                var na = new Vector2[positionsCount + x];
                int i = 0;
                for (; i < positionsCount; i++)
                {
                    na[i] = newGuysPositions[i];
                }
                int k = 0;
                for (; i < positionsCount + x; i++)
                {
                    int a = positionsCount - 1 - i;
                    int b = positionsCount - 2 - i;
                    if (a < 0 || b < 0) break;
                    newGuysPositions[positionsCount + i] =
                    Vector2.Lerp(newGuysPositions[a], newGuysPositions[b], 0.5f);
                }
                newGuysPositions = na;
                na = null;
            }
            void DenseCrowd()
            {
                // уплотняем толпу, ставя новых между предыдущими
                int a, b;
                for (int i = 0; i < positionsCount - 1; i++)
                {
                    a = positionsCount - 1 - i;
                    b = positionsCount - 2 - i;
                    if (a < 0 || b < 0) break;
                    newGuysPositions[positionsCount + i] =
                    Vector2.Lerp(newGuysPositions[a], newGuysPositions[b], 0.5f);
                }
            }

            Vector3 v1, v2,vx;
            float t = Time.deltaTime;
            for (int i = 0; i < count; i++)
            {
                int matchCount = 0; // совпадение позиций
                if (aliveMap[i] == true)
                {
                    v1 = guys[i].transform.localPosition;
                    vx = newGuysPositions[i];
                    v2 = new Vector3((vx.x - 0.5f) * FORMATION_WIDTH, SmallGuy.HEIGHT, (vx.y - 0.5f) * FORMATION_LENGTH);
                    guys[i].transform.localPosition = Vector3.MoveTowards(v1, v2, FORMATION_CHANGE_SPEED * t);
                }
                if (matchCount == count) guysRelocation = false;
            }
        }
    }

    public void StartRun()
    {
        splineFollower.follow = true;
        foreach (var g in guys)
        {
            g?.ChangeAnimationMode(GuyAnimationMode.Run);
        }
        gameStarted = true;
    }
    public void Restart()
    {
        guysRelocation = false;
        gameStarted = false;
        splineFollower.Restart();
        PrepareStartRow();        
    }

    private void LateUpdate()
    {
        cameraTransform.position = transform.TransformPoint(lookVector);
        cameraTransform.LookAt(transform.position);
    }

    public int GetRealGuysCount()
    {
        int x = 0, count = aliveMap.Length;
        if (count != 0)
        {
            foreach (bool b in aliveMap)
            {
                if (b) x++;
            }
        }
        return x;
    }

    public void ApplyFormation(Vector2[] positions)
    {
        CheckGuysArray();
        newGuysPositions = positions;
        foreach (var p in positions)
        {
            //Debug.Log(p);
        }
        guysRelocation = true;
    }

    private void CheckGuysArray()
    {
        int guysAlive = 0, count = aliveMap.Length;
        if (count != 0)
        {
            foreach (bool b in aliveMap) { if (b) { guysAlive++; } }
        }        

        if (guysAlive == 0)
        {
            splineFollower.follow = false;
            SessionMaster.current.Fail();
            gameStarted = false;
        }
        else
        {
            if (guysAlive != count)
            {
                var nguys = new SmallGuy[guysAlive];
                if (guysAlive != 0)
                {
                    int k = 0;
                    for (int i = 0; i < count; i++)
                    {
                        if (aliveMap[i]) nguys[k++] = guys[i];
                    }
                }
                guys = nguys; nguys = null;
                aliveMap = new BitArray(guysAlive, true);
            }
        }
    }

    public void MoveGuyToInjured(SmallGuy sg)
    {
        int count = aliveMap.Length;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (guys[i] == sg)
                {
                    aliveMap[i] = false;
                    guys[i] = null;                    
                }
            }
        }
        injuredGuysPool.Add(sg);
        CheckGuysArray();
    }
    public void MoveGuyFromInjuredToDisabled(SmallGuy sg)
    {
        injuredGuysPool.Remove(sg);
        disabledGuysPool.Add(sg);
    }

    public void AddNewGuy(Vector3 position)
    {
        if (guys.Length < MAX_GUYS)
        {
            int count = aliveMap.Length;
            var nga = new SmallGuy[count + 1];
            var naa = new BitArray(count + 1);
            int i = 0;
            for (; i < count; i++)
            {
                nga[i] = guys[i];
                naa[i] = aliveMap[i];
            }
            var g = GetGuy();
            g.ChangeAnimationMode(GuyAnimationMode.Run);
            g.transform.position = position;
            nga[i] = g;
            naa[i] = true;
            guys = nga; nga = null;
            aliveMap = naa; naa = null;
        }
        else Debug.Log("Превышен лимит");
    }


    public void Finish()
    {
        SessionMaster.current.Victory();
        //
        SmallGuy sg;
        for (int i = 0; i < aliveMap.Length; i++)
        {
            if (aliveMap[i])
            {
                sg = guys[i];
                sg.ChangeAnimationMode(GuyAnimationMode.Victory);
                sg.transform.Rotate(Vector3.up * 180f, Space.Self);
            }
        }
    }

    public void OnPauseStart()
    {
        gamePaused = true;
        splineFollower.follow = false;
    }

    public void OnPauseEnd()
    {
        gamePaused = false;
        splineFollower.follow = true;
    }
}
