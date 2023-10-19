using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformType { SimpleMove, MoveUntilPoint, Spring, Fall, Break }

public class PlatformController : MonoBehaviour
{
    [SerializeField] public PlatformType platformType;
    [SerializeField] public float _actionDelay = 0f;
    [SerializeField] public Transform[] _points;
    [SerializeField] public float _speed;
    [SerializeField] public LayerMask playerLayer;

    bool _startedAction = false;
    bool _doMove = false;
    private bool _playerIsOnPlatform;
    int i = 1;
    Vector2 targetPos;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //if (platformType == PlatformType.Fall)
        //{
        //    var pe = gameObject.AddComponent<PlatformEffector2D>();
        //    pe.useOneWay = true;
        //    GetComponent<BoxCollider2D>().usedByEffector = true;
        //    rb.constraints = RigidbodyConstraints2D.FreezePositionX;
        //}
        //else if (platformType == PlatformType.SimpleMove || platformType == PlatformType.MoveUntilPoint)
        //{ 
        //    _doNextMove = platformType == PlatformType.SimpleMove;                        
        //    rb.position = _points[0].position;
        //}
    }

    private void Update()
    {
        #region Collision Logic
        _playerIsOnPlatform = Physics2D.OverlapBox(rb.position + Vector2.up * .1f, new Vector2(5.75f, 1f), 0, playerLayer);
        switch (platformType)
        {
            case PlatformType.SimpleMove:
                /*
                 *  Starting when the instance begins, moves the platform through a cycle of points in order, without stopping 
                 */
                break;
            case PlatformType.MoveUntilPoint:
                /*  
                 *  if Player is on platform, 
                 *      wait for a delay
                 *      move the platform until the next point in the cycle
                 *      case 1:
                 *          if player is still on, keep moving without stoping
                 *      case 2:
                 *          if player got off before reaching the point, stop at that point and go back to start.
                 */
                if (!_startedAction && _playerIsOnPlatform)
                {
                    _startedAction = true;
                    StartCoroutine(HandleMoveUntilPoint());
                }
                break;
            case PlatformType.Spring:
                /*
                 *  if Player is on platform
                 *      move the platform towards a point
                 *      case 1:
                 *          player remains on the platform
                 *              do nothing, stay at the point
                 *      case 2:
                 *          player gets off the platform
                 *              as soon as player gets off, start moving the platform back to the starting position
                 */                
                break;
            case PlatformType.Fall:
                /*
                 *  as soon as player stands on platform
                 *      start a timer to drop the platform off the map, into oblivion
                 *          destroy it after a set time delay
                 */
                if (!_startedAction && _playerIsOnPlatform)
                {
                    _startedAction = true;
                    StartCoroutine(HandleFall());
                }
                break;
            case PlatformType.Break:
                /*
                 *  as soon as player stands on platform
                 *      start a timer to destroy it.
                 */
                if (!_startedAction && _playerIsOnPlatform)
                {
                    _startedAction = true;
                    StartCoroutine(HandleBreak());
                }
                break;

        }
        #endregion
    }

    private void FixedUpdate()
    {
        switch (platformType)
        {
            case PlatformType.SimpleMove:
                SimpleMove();
                break;
            case PlatformType.MoveUntilPoint:
                if (_doMove)
                {
                    MoveUntilPoint();
                }
                break;
            case PlatformType.Spring:
                MoveWhilePlayerOn();
                break;
        }
    }

    //#region COLLISION
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    switch (platformType)
    //    {
    //        case PlatformType.Fall:
    //            StartCoroutine(HandleFall());
    //            break;
    //        case PlatformType.Break:
    //            StartCoroutine(HandleBreak());
    //            break;
    //        case PlatformType.MoveUntilPoint:
    //            StartCoroutine(HandleMoveUntilPoint());
    //            break;
    //        case PlatformType.Weighted:
    //            StartCoroutine(HandleWeighted());
    //            break;
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (platformType == PlatformType.MoveUntilPoint)
    //    {
    //        _doNextMove = false;
    //    }
    //    else if (platformType == PlatformType.Weighted)
    //    {
    //        _playerIsOn = false;
    //    }
    //}
    //#endregion

    #region FALL
    private IEnumerator HandleFall()
    {
        yield return new WaitForSeconds(_actionDelay);
        rb.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject, 4f);
    }
    #endregion

    #region BREAK
    private IEnumerator HandleBreak()
    {
        yield return new WaitForSeconds(_actionDelay);
        Destroy(gameObject);
    }
    #endregion

    #region SIMPLE MOVE
    private void SimpleMove()
    {
        if (Vector2.Distance(transform.position, _points[i].position) < .02f * _speed)
        {
            rb.position = _points[i].position;
            i = (i + 1) % _points.Length;
        }
        else
        {
            targetPos = Vector2.MoveTowards(rb.position, _points[i].position, _speed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }
    }
    #endregion

    #region MOVE UNTIL POINT
    private IEnumerator HandleMoveUntilPoint()
    {
        yield return new WaitForSeconds(_actionDelay);
        _doMove = true;
    }

    private void MoveUntilPoint()
    {
        if (Vector2.Distance(rb.position, _points[i].position) < .02f * _speed)
        {
            rb.position = _points[i].position;
            i = (i + 1) % _points.Length;

            if (!_playerIsOnPlatform)
            {
                _doMove = false;
                _startedAction = false;
            }
        }
        else
        {
            targetPos = Vector2.MoveTowards(rb.position, _points[i].position, _speed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }
    }
    #endregion

    #region MOVE WHILE PLAYER ON
    private void MoveWhilePlayerOn()
    {
        int i = (_playerIsOnPlatform ? 1 : 0);

        if (Vector2.Distance(rb.position, _points[i].position) > .02f * _speed)
        {
            targetPos = Vector2.MoveTowards(rb.position, _points[i].position, _speed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }
    }
    #endregion
}
