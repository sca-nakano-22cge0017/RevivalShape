using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 確認フェーズ　カメラ制御
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField] private StageController stageController;
    [SerializeField] private Tutorial tutorial;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private Camera _camera;
    private Vector3 mapSize;
    private float wid, hei;  // スクリーンサイズ

    [SerializeField, Header("オブジェクトとの距離")] private float dir = 12;
    private Vector3 target; // 注視位置
    private float range; // カメラが移動できる球の半径

    // これらはカメラのY座標を0に補正できるようにするための変数
    [SerializeField, Header("ajustAngle度毎に補正ポイントを置く")] private float adjustAngle;
    private Vector3[] point; // カメラ位置の補正 一定距離まで近付いたらこの座標に移動させる
    const float MIN_Y = -0.5f; // y座標の下限

    // 回転
    private float sensitivity;
    [SerializeField] private SensChenz sensChenger;
    private Vector2 startPos;    // タップの始点
    private Vector2 lastTapPos;  // 前フレームのタップ位置
    private float tx, ty;
    [SerializeField, Header("スワイプ 手振れ補正値"), Tooltip("斜め方向へのスワイプをx軸/y軸に真っ直ぐな移動に補正する")]
    private float dragAdjust = 10;

    // 回転解除
    private bool didSwip; // スワイプで回転させたかどうか
    private bool isRestoring = false;
    private Tween restoreTween = null;
    private bool isRestoreStart = false;
    [SerializeField, Header("スワイプ回転解除範囲　最小")] private Vector2 rotateCancellRangeMin;
    [SerializeField, Header("スワイプ回転解除範囲　最大")] private Vector2 rotateCancellRangeMax;

    // 拡縮
    private float sDist = 0.0f, nDist = 0.0f;  //距離変数
    private float vRatio = 1.0f;               //現在倍率
    [SerializeField, Header("Field of Viewの最大・最低値")] private float vMax, vMin;
    [SerializeField, Header("Field of Viewの初期値")] private float vDefault;
    [SerializeField, Header("基本の拡縮速度")] private float vSpeed;

    // ダブルタップ
    private bool is90Rotate = false;
    [SerializeField, Header("ダブルタップ除外範囲　最小")] private Vector2 cantDoubleTapRangeMin;
    [SerializeField, Header("ダブルタップ除外範囲　最大")] private Vector2 cantDoubleTapRangeMax;
    [SerializeField, Header("ダブルタップ時の回転にかかる時間")] private float rotateTime = 1.0f;
    private bool isRotating = false;
    private Tween rotateTween = null;
    private bool isRotateStart = false;

    // 移動方向
    private enum TeleportDir { UP, DOWN, RIGHT, LEFT, NULL };
    private TeleportDir teleportDir = TeleportDir.NULL; // 移動方向
    private TeleportDir inputDir1 = TeleportDir.NULL; // 一回目に入力した移動方向
    private TeleportDir inputDir2 = TeleportDir.NULL; // 二回目に入力した移動方向

    private enum CameraPos { UP, BACK, RIGHT, FRONT, LEFT, NULL };
    private CameraPos currentCameraPos = CameraPos.UP; // 現在のカメラ位置
    private CameraPos nextCameraPos = CameraPos.NULL; // 次のカメラ位置

    // 90度毎の補正位置・回転 画面端をダブルタップでこれらの位置に移動する
    private Dictionary<CameraPos, Vector3> adjustPoint = new();
    private Dictionary<CameraPos, Quaternion> adjustQuaternion = new();

    // 実行フェーズ時のカメラ位置
    [SerializeField, Header("実行フェーズ時のカメラと水平軸のなす角度")] private float play_Angle;

    /// <summary>
    /// カメラを動かすかどうか
    /// falseのときは動かない
    /// </summary>
    public bool CanRotate { get; set; } = false;

    [SerializeField] GameObject debugobj;

    void Awake()
    {
        wid = Screen.width;
        hei = Screen.height;

        point = new Vector3[(int)(360 / adjustAngle)];

        adjustQuaternion[CameraPos.UP] = Quaternion.Euler(90, 0, 180);
        adjustQuaternion[CameraPos.BACK] = Quaternion.Euler(181.79f, 180, 180);
        adjustQuaternion[CameraPos.RIGHT] = Quaternion.Euler(181.79f, -90, 180);
        adjustQuaternion[CameraPos.FRONT] = Quaternion.Euler(181.79f, 0, 180);
        adjustQuaternion[CameraPos.LEFT] = Quaternion.Euler(181.79f, 90, 180);

        tx = 0;
        ty = 0;

        isRestoring = false;
        isRotating = false;
        is90Rotate = false;

        inputDir1 = TeleportDir.NULL;
        inputDir2 = TeleportDir.NULL;
        teleportDir = TeleportDir.NULL;
        currentCameraPos = CameraPos.UP;
        nextCameraPos = CameraPos.NULL;
    }

    void Update()
    {
        sensitivity = sensChenger.sensitivity;
        TweenPause();

        if (!CanRotate || stageController.IsPause) return;

        if (!CanRotate) tapManager.DoubleTapReset();

        if (isRestoring)
        {
            AdjustCameraToTarget();
        }

        if (!isRotating && !isRestoring)
        {
            Swip();
            Scaling();
            DoubleTap();
        }
    }

    /// <summary>
    /// スワイプ
    /// </summary>
    private void Swip()
    {
        if (stageController.IsTutorial && tutorial.MethodName != "CheckB" && !tutorial.TutorialCompleteByPhase) return;

        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                // 範囲外は無効
                if (!tapManager.TapOrDragRange(t.position)) return;

                startPos = t.position;
                lastTapPos = t.position;

                // シングルタップ時、スワイプによる回転状態をもとに戻す
                // 判定範囲内だったら処理する
                if (didSwip &&
                    tapManager.TapOrDragRange(t.position, rotateCancellRangeMin, rotateCancellRangeMax) &&
                    !stageController.IsTutorial)
                {
                    RotateRestore();
                    isRestoreStart = true;
                }
            }
            else if (t.phase == TouchPhase.Moved)
            {
                // 範囲外からスワイプしたとき用の調整
                if (!tapManager.TapOrDragRange(t.position))
                {
                    startPos = t.position;
                    lastTapPos = t.position;
                }

                // スワイプ量
                tx = t.position.x - lastTapPos.x;
                ty = t.position.y - lastTapPos.y;

                if (stageController.IsTutorial && tutorial.MethodName == "CheckB")
                {
                    ty = 0;
                }

                // 移動量から回転角度を求める
                // 始点からの移動量の絶対値が、dragAjustより小さかったら0にし、水平/垂直の移動にする
                if (Mathf.Abs(startPos.x - t.position.x) < dragAdjust) tx = 0;
                float deltaAngleLR = tx / wid * sensitivity;

                if (Mathf.Abs(startPos.y - t.position.y) < dragAdjust) ty = 0;
                float deltaAngleTB = -ty / hei * sensitivity;

                if (tx != 0 || ty != 0) didSwip = true;

                // カメラから見て上・右向きのベクトルを回転軸として回転させる
                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up); // 左右方向
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right); // 上下方向

                var pos = transform.position;
                pos -= target;
                pos = angleAxisLR * angleAxisTB * pos; // 回転移動
                pos += target;                          // 平行移動


                if (pos.y >= MIN_Y)
                {
                    transform.position = pos;
                    transform.LookAt(target, transform.up);
                }
                else
                {
                    AdjustCameraToClosestPoint();
                }

                lastTapPos = t.position;
            }

            else if (t.phase == TouchPhase.Ended)
            {
                if (stageController.IsTutorial)
                {
                    // 右ドラッグで次へ
                    if (!tutorial.ToCheckB_2 && tx > 0) tutorial.ToCheckB_2 = true;

                    // 左ドラッグで次へ
                    if (tutorial.ToCheckB_2 && !tutorial.ToCheckC && tx < 0) tutorial.ToCheckC = true;
                }
            }
        }
    }

    /// <summary>
    /// 拡縮
    /// </summary>
    private void Scaling()
    {
        if (stageController.IsTutorial && !tutorial.TutorialCompleteByPhase) return;

        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            // 範囲外は無効
            if (!tapManager.TapOrDragRange(t1.position) || !tapManager.TapOrDragRange(t2.position)) return;

            if (t2.phase == TouchPhase.Began)
            {
                // タップした二点間の距離を取得
                sDist = Vector2.Distance(t1.position, t2.position);

                vRatio = _camera.fieldOfView;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                nDist = Vector2.Distance(t1.position, t2.position);

                // 縮小
                if (sDist > nDist) vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin)) * (sDist - nDist);
                // 拡大
                if (sDist < nDist) vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin)) * (nDist - sDist);

                _camera.fieldOfView = vRatio;
                sDist = nDist;

                transform.LookAt(target, transform.up);
            }
        }
    }

    /// <summary>
    /// オブジェクトから目を離さないように注視点を設定
    /// </summary>
    private void AdjustCameraToTarget()
    {
        if (currentCameraPos == CameraPos.UP) transform.LookAt(target, Vector3.back);
        else transform.LookAt(target, Vector3.up);
    }

    /// <summary>
    /// カメラがオブジェクトの下に行かないように調整する
    /// </summary>
    private void AdjustCameraToClosestPoint()
    {
        Vector3 nearestPos = new Vector3(100, 100, 100);
        var currentPosition = transform.position;

        foreach (var p in point)
        {
            if ((currentPosition - nearestPos).magnitude > (currentPosition - p).magnitude)
            {
                nearestPos = p;
            }
        }

        transform.position = nearestPos;
        transform.LookAt(target, transform.up);
    }

    /// <summary>
    /// ダブルタップで90度回転 前後左右に移動する
    /// </summary>
    private void DoubleTap()
    {
        if (stageController.IsTutorial && tutorial.MethodName != "CheckD" && !tutorial.TutorialCompleteByPhase) return;

        tapManager.DoubleTap(
            () =>
            {
                Touch t = Input.GetTouch(0);

                // 範囲外は無効
                if (!tapManager.TapOrDragRange(t.position) ||
                    tapManager.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax))
                    return;

                // 入力方向を保存
                if (inputDir1 == TeleportDir.NULL)
                    inputDir1 = DoubleTapPosJudgement(t.position);
            },

            () =>
            {
                Touch t = Input.GetTouch(0);

                // 範囲外は無効
                if (!tapManager.TapOrDragRange(t.position) ||
                    tapManager.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax))
                    return;

                // 入力方向を保存
                if (inputDir2 == TeleportDir.NULL)
                    inputDir2 = DoubleTapPosJudgement(t.position);

                if (inputDir1 == inputDir2 && inputDir1 != TeleportDir.NULL && inputDir2 != TeleportDir.NULL)
                    teleportDir = inputDir1;
                else
                {
                    inputDir1 = TeleportDir.NULL;
                    inputDir2 = TeleportDir.NULL;
                }

                is90Rotate = true;
            },

            () =>
            {
                inputDir1 = TeleportDir.NULL;
                inputDir2 = TeleportDir.NULL;
            });

        if (is90Rotate)
        {
            if(stageController.IsTutorial)
            {
                if((tutorial.ToCheckD && !tutorial.ToCheckE && teleportDir == TeleportDir.RIGHT) || tutorial.TutorialCompleteByPhase)
                {
                    tutorial.IsCheckD = true;
                }
                else
                {
                    is90Rotate = false;
                    inputDir1 = TeleportDir.NULL;
                    inputDir2 = TeleportDir.NULL;
                    return;
                }
            }

            // 現在のカメラ位置から見た上下左右を求める
            switch (currentCameraPos)
            {
                case CameraPos.UP:
                    RotateDirSet(CameraPos.BACK, CameraPos.FRONT, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.FRONT:
                    RotateDirSet(CameraPos.UP, CameraPos.NULL, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.BACK:
                    RotateDirSet(CameraPos.UP, CameraPos.NULL, CameraPos.LEFT, CameraPos.RIGHT);
                    break;
                case CameraPos.RIGHT:
                    RotateDirSet(CameraPos.UP, CameraPos.NULL, CameraPos.BACK, CameraPos.FRONT);
                    break;
                case CameraPos.LEFT:
                    RotateDirSet(CameraPos.UP, CameraPos.NULL, CameraPos.FRONT, CameraPos.BACK);
                    break;
            }

            is90Rotate = false;
            inputDir1 = TeleportDir.NULL;
            inputDir2 = TeleportDir.NULL;
        }
    }

    /// <summary>
    /// タップ位置が上下左右の範囲のどこにあるかを判定し、移動方向を返す
    /// </summary>
    /// <param name="_pos">タップ位置</param>
    /// <returns>方向を返す</returns>
    private TeleportDir DoubleTapPosJudgement(Vector2 _pos)
    {
        string posName = tapManager.TapPosJudgement(_pos);
        TeleportDir dir;

        switch (posName)
        {
            case "up":
                dir = TeleportDir.UP;
                break;
            case "down":
                dir = TeleportDir.DOWN;
                break;
            case "right":
                dir = TeleportDir.RIGHT;
                break;
            case "left":
                dir = TeleportDir.LEFT;
                break;
            default:
                dir = TeleportDir.NULL;
                break;
        }

        return dir;
    }

    /// <summary>
    /// 現在のカメラ位置に応じて上下左右方向のカメラ位置を設定して回転させる
    /// </summary>
    /// <param name="_up">上</param>
    /// <param name="_down">下</param>
    /// <param name="_right">右</param>
    /// <param name="_left">左</param>
    private void RotateDirSet(CameraPos _up, CameraPos _down, CameraPos _right, CameraPos _left)
    {
        switch (teleportDir)
        {
            case TeleportDir.UP:
                nextCameraPos = _up;
                break;
            case TeleportDir.DOWN:
                nextCameraPos = _down;
                break;
            case TeleportDir.RIGHT:
                nextCameraPos = _right;
                break;
            case TeleportDir.LEFT:
                nextCameraPos = _left;
                break;
            default:
                nextCameraPos = CameraPos.NULL;
                break;
        }

        if (nextCameraPos == CameraPos.NULL) return;

        StartCoroutine(Rotate90Degrees());
        isRotateStart = true;
    }

    private IEnumerator Rotate90Degrees()
    {
        if (didSwip)
        {
            RotateRestore();
            isRestoreStart = true;
            yield return new WaitUntil(() => !isRestoring);
        }

        isRotating = true;
        var sequence = DOTween.Sequence();

        Vector3[] controllPoint = GetControllPoint(); // 制御点取得

        Tween move = transform.DOLocalPath(new[] { adjustPoint[nextCameraPos], controllPoint[0], controllPoint[1] }, rotateTime, PathType.CubicBezier).SetOptions(false);

        if (currentCameraPos != nextCameraPos)
        {
            if ((currentCameraPos != CameraPos.UP && nextCameraPos != CameraPos.UP) ||
                (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.FRONT) ||
                (currentCameraPos == CameraPos.FRONT && nextCameraPos == CameraPos.UP))
            {
                sequence
                    .Join(move)
                    .Join(transform.DORotateQuaternion(adjustQuaternion[nextCameraPos], rotateTime));
            }

            // 上←→左/右/後のときは回転方法を変更
            else
            {
                var angleXZ = new Vector3(0.0f, 0.0f, 0.0f); // X軸、Z軸の回転
                var angleY = new Vector3(0.0f, 0.0f, 0.0f); // Y軸の回転

                // 回転角度取得
                if (currentCameraPos == CameraPos.BACK && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(91.79f, 0.0f, 0.0f);
                    angleY = new Vector3(0.0f, -180.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.BACK)
                {
                    angleXZ = new Vector3(-91.79f, 0.0f, 0.0f);
                    angleY = new Vector3(0.0f, -180.0f, 0.0f);
                }

                else if (currentCameraPos == CameraPos.RIGHT && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, -91.79f);
                    angleY = new Vector3(0.0f, 90.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.RIGHT)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, 91.79f);
                    angleY = new Vector3(0.0f, -90.0f, 0.0f);
                }

                else if (currentCameraPos == CameraPos.LEFT && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, 91.79f);
                    angleY = new Vector3(0.0f, -90.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.LEFT)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, -91.79f);
                    angleY = new Vector3(0.0f, 90.0f, 0.0f);
                }

                // 回転
                if (nextCameraPos == CameraPos.UP)
                {
                    sequence
                        .Join(transform.DORotate(angleXZ, rotateTime, RotateMode.WorldAxisAdd))
                        .Join(move)
                        .Append(transform.DORotate(angleY, rotateTime / 2.0f, RotateMode.WorldAxisAdd));
                }
                else
                {
                    sequence
                        .Join(transform.DORotate(angleY, rotateTime / 2.0f, RotateMode.WorldAxisAdd))
                        .Append(move)
                        .Join(transform.DORotate(angleXZ, rotateTime, RotateMode.WorldAxisAdd));
                }
            }
        }

        sequence.OnComplete(() =>
        {
            currentCameraPos = nextCameraPos;
            didSwip = false;
            tx = 0;
            ty = 0;
            isRotating = false;
            isRestoring = false;
            rotateTween = null;

            if (stageController.IsTutorial && tutorial.MethodName == "CheckD")
            {
                tutorial.ToCheckE = true;
            }
        });

        rotateTween = sequence;
    }

    /// <summary>
    /// スワイプによるカメラの移動を元の位置に戻す
    /// </summary>
    /// <returns></returns>
    private void RotateRestore()
    {
        isRestoring = true;

        var sequence = DOTween.Sequence();

        sequence
            .Join(transform.DOLocalPath(new[] { transform.position, adjustPoint[currentCameraPos] }, rotateTime, PathType.CatmullRom).SetOptions(false))
            .Join(transform.DORotateQuaternion(adjustQuaternion[currentCameraPos], rotateTime))
            .OnComplete(() =>
            {
                didSwip = false;
                isRestoring = false;
                tx = 0;
                ty = 0;
                restoreTween = null;

                if (stageController.IsTutorial && tutorial.IsCheckC)
                {
                    tutorial.ToCheckD = true;
                }
            });

        restoreTween = sequence;
    }

    private Vector3[] GetControllPoint()
    {
        Vector3 relay = Vector3.zero;
        Vector3[] point = new Vector3[2]; // 制御点

        if (adjustPoint[currentCameraPos].x == adjustPoint[nextCameraPos].x) relay.x = adjustPoint[currentCameraPos].x;
        else relay.x = (Mathf.Abs(target.x - adjustPoint[currentCameraPos].x) >= 1f) ? adjustPoint[currentCameraPos].x : adjustPoint[nextCameraPos].x;

        if (adjustPoint[currentCameraPos].y == adjustPoint[nextCameraPos].y) relay.y = adjustPoint[currentCameraPos].y;
        else relay.y = (Mathf.Abs(target.y - (adjustPoint[currentCameraPos].y - MIN_Y)) >= 1f) ? adjustPoint[currentCameraPos].y : adjustPoint[nextCameraPos].y;

        if (adjustPoint[currentCameraPos].z == adjustPoint[nextCameraPos].z) relay.z = adjustPoint[currentCameraPos].z;
        else relay.z = (Mathf.Abs(target.z - adjustPoint[currentCameraPos].z) >= 1f) ? adjustPoint[currentCameraPos].z : adjustPoint[nextCameraPos].z;

        point[0] = Vector3.Lerp(adjustPoint[currentCameraPos], relay, 0.5f);
        point[1] = Vector3.Lerp(adjustPoint[nextCameraPos], relay, 0.5f);

        return point;
    }

    /// <summary>
    /// カメラの位置・回転を初期状態に戻す
    /// </summary>
    /// <param name="didGoThroughClosetPoint">最も近い補正ポイントを経由するか</param>
    public void RotateReset(bool didGoThroughClosetPoint)
    {
        if (isRotating || isRestoring) return;

        if (currentCameraPos != CameraPos.UP && didGoThroughClosetPoint)
        {
            currentCameraPos = GetCameraToClosest90Point();
        }

        nextCameraPos = CameraPos.UP;
        _camera.fieldOfView = vDefault;

        StartCoroutine(RotateResetCoroutine());
    }

    private IEnumerator RotateResetCoroutine()
    {
        if (didSwip)
        {
            RotateRestore();
            isRestoreStart = true;
            yield return new WaitUntil(() => !isRestoring);
        }

        if (currentCameraPos != CameraPos.UP)
        {
            StartCoroutine(Rotate90Degrees());
            isRotateStart = true;
        }
    }

    private CameraPos GetCameraToClosest90Point()
    {
        Vector3 nearestPos = new Vector3(100, 100, 100);
        CameraPos cameraPos = CameraPos.NULL;
        var currentPosition = transform.position;

        foreach (var p in adjustPoint)
        {
            if ((currentPosition - nearestPos).magnitude > (currentPosition - p.Value).magnitude)
            {
                nearestPos = p.Value;
                cameraPos = p.Key;
            }
        }

        return cameraPos;
    }

    /// <summary>
    /// 実行フェーズ時のカメラ位置に移動
    /// </summary>
    public void ToPlayPhase()
    {
        StopAllCoroutines();

        transform.rotation = Quaternion.Euler(0, 0, 0);

        var angle = 90 - play_Angle;

        float buttom = target.z + range * Mathf.Cos(angle * Mathf.Deg2Rad);
        float height = range * Mathf.Sin(angle * Mathf.Deg2Rad);

        transform.position = new Vector3(target.x, buttom, height);
        transform.LookAt(target, transform.up);

        currentCameraPos = CameraPos.UP;
    }

    /// <summary>
    /// 実行フェーズから確認フェーズに戻ったときにカメラを移動
    /// </summary>
    public void FromPlayPhase()
    {
        didSwip = true;
        RotateReset(false);
    }

    /// <summary>
    /// カメラの初期設定
    /// </summary>
    public void CameraSetting()
    {
        currentCameraPos = CameraPos.UP;
        nextCameraPos = CameraPos.NULL;

        // サイズ代入
        if (stageController)
            mapSize = stageController.MapSize;

        // 注視位置設定
        target = mapSize / 2;
        target.x *= -1;
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f);

        // サンプルのサイズに応じてカメラの初期位置を調整
        transform.position = new Vector3(target.x, mapSize.x + dir + MIN_Y, target.z);

        // 注視
        transform.LookAt(target, transform.up);

        // 補正ポイントの生成
        range = (target - transform.position).magnitude;

        Vector3 firstPos = new Vector3(range, MIN_Y, 0);
        for (int i = 0; i < point.Length; i++)
        {
            point[i] = Quaternion.Euler(0, i * adjustAngle, 0) * firstPos + target;
        }

        // 90度毎の補正ポイントの作成
        adjustPoint.Add(CameraPos.UP, transform.position);

        for (int i = 1; i < 5; i++)
        {
            CameraPos c = (CameraPos)Enum.ToObject(typeof(CameraPos), i);
            adjustPoint.Add(c, Quaternion.Euler(0, i * 90, 0) * firstPos + target);
        }
    }

    /// <summary>
    /// Tweenの一時停止/再開
    /// </summary>
    private void TweenPause()
    {
        if (rotateTween != null)
        {
            if (!stageController.IsPause && isRotateStart)
            {
                isRotateStart = false;
                rotateTween.Play();
            }
            else if (stageController.IsPause && !isRotateStart)
            {
                isRotateStart = true;
                rotateTween.Pause();
            }
        }

        if (restoreTween != null)
        {
            if (!stageController.IsPause && isRestoreStart)
            {
                isRestoreStart = false;
                restoreTween.Play();
            }
            else if (stageController.IsPause && !isRestoreStart)
            {
                isRestoreStart = true;
                restoreTween.Pause();
            }
        }
    }
}
