using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static MathExtensions;

/// <summary>
/// �^�b�v�֘A�̏���
/// </summary>
public class TapManager : MonoBehaviour
{
    [SerializeField, Header("�X���C�v�͈̔� ����")] private Vector2 dragRangeMin = new Vector2(0, 370);
    public Vector2 DragRangeMin
    {
        get
        {
            return dragRangeMin;
        }
        set
        {
            dragRangeMin = value;
        }
    }

    [SerializeField, Header("�X���C�v�͈̔� �E��")] private Vector2 dragRangeMax = new Vector2(1080, 1700);
    public Vector2 DragRangeMax
    {
        get
        {
            return dragRangeMax;
        }
        set
        {
            dragRangeMax = value;
        }
    }

    Dictionary<string, Vector3> vertex = new(); // ����\�͈͂̒��S��4���_

    [Header("�_�u���^�b�v")]
    private bool isDoubleTapStart = false;
    private float doubleTapTime = 0;
    [SerializeField] private float doubleTapLimit = 0.2f;

    // ������
    private bool countStart = false;
    private float holdingTime = 0;

    private void Awake()
    {
        vertex.Add("center", new Vector2((dragRangeMax.x + dragRangeMin.x) / 2, (dragRangeMax.y + dragRangeMin.y) / 2));
        vertex.Add("left_up", new Vector2(dragRangeMin.x, dragRangeMin.y));
        vertex.Add("right_up", new Vector2(dragRangeMax.x, dragRangeMin.y));
        vertex.Add("left_down", new Vector2(dragRangeMin.x, dragRangeMax.y));
        vertex.Add("right_down", new Vector2(dragRangeMax.x, dragRangeMax.y));

        isDoubleTapStart = false;
        doubleTapTime = 0;

        countStart = false;
        holdingTime = 0;
    }

    /// <summary>
    /// �^�b�v�ʒu����ʏ�̂ǂ������㉺���E�Ŕ���
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <returns></returns>
    public string TapPosJudgement(Vector2 _pos)
    {
        string s = "up";

        _pos.y *= -1;
        _pos.y += Screen.height;

        // �㕔��
        if (InsideOrOutsideJudgement(vertex["right_up"], vertex["left_up"], vertex["center"], _pos))
            s = "up";

        // ������
        if (InsideOrOutsideJudgement(vertex["left_down"], vertex["right_down"], vertex["center"], _pos))
            s = "down";

        // �E����
        if (InsideOrOutsideJudgement(vertex["right_down"], vertex["right_up"], vertex["center"], _pos))
            s = "right";

        // ������
        if (InsideOrOutsideJudgement(vertex["left_up"], vertex["left_down"], vertex["center"], _pos))
            s = "left";

        return s;
    }

    /// <summary>
    /// �_�u���^�b�v
    /// </summary>
    /// <param name="firstTap">��x�ڂ̃^�b�v�ōs������</param>
    /// <param name="secondTap">��x�ڂ̃^�b�v�ōs������</param>
    /// <param name="timeExceeded">�_�u���^�b�v�ƂȂ鎞�Ԃ𒴉߂����Ƃ��̏���</param>
    public void DoubleTap(Action firstTap, Action secondTap, Action timeExceeded)
    {
        if (isDoubleTapStart)
        {
            doubleTapTime += Time.deltaTime;
            if (doubleTapTime < doubleTapLimit)
            {
                if (Input.touchCount == 1)
                {
                    Touch t = Input.GetTouch(0);

                    if (t.phase == TouchPhase.Began)
                    {
                        secondTap?.Invoke();

                        DoubleTapReset();
                    }
                }
            }
            else
            {
                timeExceeded?.Invoke();

                isDoubleTapStart = false;
                doubleTapTime = 0.0f;
            }
        }
        else
        {
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    firstTap?.Invoke();

                    isDoubleTapStart = true;
                    doubleTapTime = 0.0f;
                }
            }
        }
    }

    public void DoubleTap(Action action)
    {
        if (isDoubleTapStart)
        {
            doubleTapTime += Time.deltaTime;
            if (doubleTapTime < doubleTapLimit)
            {
                if (Input.touchCount == 1)
                {
                    Touch t = Input.GetTouch(0);

                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        action?.Invoke();

                        isDoubleTapStart = false;
                        doubleTapTime = 0.0f;
                    }
                }
            }
            else
            {
                DoubleTapReset();
            }
        }
        else
        {
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    isDoubleTapStart = true;
                    doubleTapTime = 0.0f;
                }
            }
        }
    }

    public void DoubleTapReset()
    {
        isDoubleTapStart = false;
        doubleTapTime = 0;
    }


    public void LongTap(Action complete, Action cancel, float holdTime)
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                // �͈͊O�͖���
                if (!TapOrDragRange(t.position)) return;

                countStart = true;
                holdingTime = 0;
            }

            if (t.phase == TouchPhase.Ended)
            {
                // �͈͊O�͖���
                if (!TapOrDragRange(t.position)) return;

                countStart = false;
                holdingTime = 0;

                cancel?.Invoke();
            }
        }

        if(countStart) holdingTime += Time.deltaTime;

        if(holdingTime >= holdTime)
        {
            complete?.Invoke();
        }
    }

    public void LongTapReset()
    {
        countStart = false;
        holdingTime = 0;
    }

    /// <summary>
    /// �^�b�v�ʒu���͈͓������ׂ�
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <returns>�͈͓��ł����true</returns>
    public bool TapOrDragRange(Vector3 _pos)
    {
        _pos.y *= -1;
        _pos.y += Screen.height;

        if (_pos.x <= dragRangeMin.x || _pos.x > dragRangeMax.x || _pos.y <= dragRangeMin.y || _pos.y > dragRangeMax.y)
            return false;
        else return true;
    }

    /// <summary>
    /// �^�b�v�ʒu���͈͓������ׂ�
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <param name="_minPos">�C�ӂ͈̔́@X,Y���W���ɍő�̏ꏊ</param>
    /// <param name="_maxPos">�C�ӂ͈̔́@X,Y���W���ɍő�̏ꏊ</param>
    /// <returns>�͈͓��ł����true</returns>
    public bool TapOrDragRange(Vector3 _pos, Vector3 _minPos, Vector3 _maxPos)
    {
        _pos.y *= -1;
        _pos.y += Screen.height;

        if (_pos.x <= _minPos.x || _pos.x > _maxPos.x || _pos.y <= _minPos.y || _pos.y > _maxPos.y)
            return false;
        else return true;
    }
}