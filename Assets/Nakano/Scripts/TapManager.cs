using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// �^�b�v�֘A�̏���
/// </summary>
public class TapManager : MonoBehaviour
{
    [SerializeField, Header("�X���C�v�͈̔� ����")] private Vector2 dragRangeMin = new Vector2(0, 370);
    [SerializeField, Header("�X���C�v�͈̔� �E��")] private Vector2 dragRangeMax = new Vector2(1080, 1700);

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

    public Vector2 DragRangeMin()
    {
        return dragRangeMin;
    }

    public Vector2 DragRangeMax()
    {
        return dragRangeMax;
    }

    /// <summary>
    /// �^�b�v�ʒu����ʏ�̂ǂ̂����肩���㉺���E�Ŕ���
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <returns></returns>
    public string TapPosJudgement(Vector2 _pos)
    {
        string s = "up";

        _pos.y *= -1;
        _pos.y += Screen.height;

        // �㕔��
        if (InsideOrOutsideJudgement.Judge(vertex["right_up"], vertex["left_up"], vertex["center"], _pos))
            s = "up";

        // ������
        if (InsideOrOutsideJudgement.Judge(vertex["left_down"], vertex["right_down"], vertex["center"], _pos))
            s = "down";

        // �E����
        if (InsideOrOutsideJudgement.Judge(vertex["right_down"], vertex["right_up"], vertex["center"], _pos))
            s = "right";

        // ������
        if (InsideOrOutsideJudgement.Judge(vertex["left_up"], vertex["left_down"], vertex["center"], _pos))
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

                        isDoubleTapStart = false;
                        doubleTapTime = 0.0f;
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
    /// <param name="_minPos">�͈́@�ŏ�</param>
    /// <param name="_maxPos">�͈́@�ő�</param>
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

public static class InsideOrOutsideJudgement
{
    /// <summary>
    /// �O�ςɂ����O����
    /// </summary>
    /// <param name="_vertexLeft">��� �����̒��_</param>
    /// <param name="_vertexRight">��� �E���̒��_</param>
    /// <param name="_vertexTop">�㕔�̒��_</param>
    /// <param name="_judgePos">���肷��ʒu</param>
    /// <returns>�O�p�`�̓����ɂ����true</returns>
    public static bool Judge(Vector2 _vertexLeft, Vector2 _vertexRight, Vector2 _vertexTop, Vector2 _judgePos)
    {
        // �x�N�g��
        Vector3 leftToTop = _vertexTop - _vertexLeft;
        Vector3 leftToRight = _vertexRight - _vertexLeft;
        Vector3 rightToTop = _vertexTop - _vertexRight;
        Vector3 leftToJudge = _judgePos - _vertexLeft;
        Vector3 rightToJudge = _judgePos - _vertexRight;

        // �O�όv�Z
        Vector3 cross1 = Vector3.Cross(leftToJudge, leftToTop);
        Vector3 cross2 = Vector3.Cross(leftToRight, leftToJudge);
        Vector3 cross3 = Vector3.Cross(rightToTop, rightToJudge);

        // �O�ς�z�̒l�̕����������Ȃ�O�p�`�̓����ɓ_�����݂���
        if ((cross1.z > 0 && cross2.z > 0 && cross3.z > 0) || (cross1.z < 0 && cross2.z < 0 && cross3.z < 0))
            return true;

        else return false;
    }
}