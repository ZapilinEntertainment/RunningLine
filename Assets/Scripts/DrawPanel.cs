using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Dreamteck.Splines;

[RequireComponent(typeof(RectTransform))]
public sealed class DrawPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPauseHandler
{
    [SerializeField] private SplineComputer splineComputer;   
    [SerializeField] private Renderer lineRenderer;
    [SerializeField] private Camera linesCamera;

    [SerializeField] private float MIN_TOUCH_DISTANCE = 7f, DRAWLINE_SIZE = 3f;
    [SerializeField] private int MAX_POINTS = 500;
    [SerializeField] private Color lineColor = Color.black;

    private int lastPositionIndex = 0;
    private bool screenTouched = false , cursorInWriteZone = false, gamePaused;
    private Vector3 prevPos;
    private PlayerController player;
    private RectTransform rectTransform;    

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        splineComputer.transform.localPosition = new Vector3(-Screen.width / 2f, - Screen.height / 2f, 0f);
        var sessionMaster = SessionMaster.current;
        player = sessionMaster.playerController;
        //
        gamePaused = sessionMaster.paused;
        sessionMaster.onPauseEnd += this.OnPauseStart;
        sessionMaster.onPauseEnd += this.OnPauseEnd;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (gamePaused) return;
        lastPositionIndex = 0;
        screenTouched = true;
        WritePosition(eventData.position);        
        lineRenderer.enabled = true;
    }
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (gamePaused) return;
        WritePosition(eventData.position);
        StopDrawing(true);
    }
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        cursorInWriteZone = true;
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        cursorInWriteZone = false;
    }

    private void WritePosition(Vector2 pos)
    {
        if (lastPositionIndex < MAX_POINTS)
        {
            var sp = new SplinePoint(linesCamera.ScreenToWorldPoint(pos));
            sp.size = DRAWLINE_SIZE;
            sp.color = Color.black;
            // splineComputer.SetPoint(lastPositionIndex++, sp, SplineComputer.Space.Local);
            splineComputer.SetPoint(lastPositionIndex++, sp, SplineComputer.Space.World);
            prevPos = pos;
        }
        //else StopDrawing(false);
    }
    private void StopDrawing(bool sendCommand)
    {
        screenTouched = false;
        lastPositionIndex = 0;
        //
        if (sendCommand)
        {
            if (splineComputer.pointCount > 1) { 
            int count = player.GetRealGuysCount();
                if (count != 0)
                {
                    var positions = new Vector2[count];
                    float p = 1f / (count + 1);

                    Transform t = splineComputer.transform;
                    Rect windowRect = rectTransform.rect;
                    // так как ориентация экрана вертикальная, y-координата будет отрицательна!
                    Vector3 v, leftDownPos = linesCamera.ScreenToWorldPoint(new Vector3(windowRect.x, windowRect.y)),
                        rightTopPos = linesCamera.ScreenToWorldPoint(new Vector3(windowRect.xMax, windowRect.yMax));
                    float deltaX = rightTopPos.x - leftDownPos.x, deltaY = rightTopPos.y - leftDownPos.y;

                    for (int i = 0; i < count; i++)
                    {
                        v = rectTransform.InverseTransformPoint(splineComputer.EvaluatePosition((i + 1) * p));
                        v.x /= rectTransform.rect.width;
                        v.x += 0.5f;
                        v.y /= rectTransform.rect.height;
                        v.y += 0.5f;
                        positions[i] = new Vector2(v.x, v.y);
                    }
                    player.ApplyFormation(positions);
                }
            }
        }
        //
        lineRenderer.enabled = false;
        splineComputer.SetPoints(new SplinePoint[0] );
    }

    private void Update()
    {
        if (gamePaused) return;

        if (screenTouched)
        {
            if (!cursorInWriteZone)
            {
                //StopDrawing(false);
            }
            else
            {
                var mpos = Input.mousePosition;
                if (Vector2.Distance(mpos, prevPos) > MIN_TOUCH_DISTANCE) WritePosition(mpos);
            }
        }
    }

    public void OnPauseStart()
    {
        gamePaused = true;
    }

    public void OnPauseEnd()
    {
        gamePaused = false;
    }
}
