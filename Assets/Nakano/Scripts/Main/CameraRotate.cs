using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MathExtensions;

/// <summary>
/// 確認フェーズ　カメラ制御
/// </summary>
public class CameraRotate : MonoBehaviour
{
    private Transform thisTransform;

    [SerializeField] private Tutorial tutorial;
    [SerializeField] private StageController stageController;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private Camera m_camera;
    private Vector3 mapSize;

    [SerializeField, Header("オブジェクトとの距離")] private float dir = 12;
    private Vector3 target; // 注視位置
    private float range;    // カメラが移動する球面の半径

    // これらはカメラのY座標を0に補正できるようにするための変数
    [SerializeField, Header("ajustAngle_Swip度毎に補正ポイントを置く")] private float adjustAngle_Swip;
    private Vector3[] adjustPoint_Swip;  // カメラ位置の補正 一定距離まで近付いたらこの座標に移動させる
    const float MIN_Y = 0.0f; // y座標の下限　これより下にカメラはいけない

    // 回転
    private float sensitivity;
    [SerializeField] private SensChenz sensChenger;
    private Vector2 startPos;    // タップの始点
    private Vector2 lastTapPos;  // 前フレームのタップ位置
    private float tx, ty;        // スワイプ量
    [SerializeField, Header("スワイプ 手振れ補正値"), Tooltip("斜め方向へのスワイプをx軸/y軸に真っ直ぐな移動に補正する")]
    private float dragAdjust = 10;

    // 回転解除
    private bool didSwip;             // スワイプで回転させたかどうか
    private bool isRestoring = false; // スワイプして回転した状態から戻っている最中か
    [SerializeField, Header("スワイプ回転解除範囲　最小")] private Vector2 rotateCancellRangeMin;
    [SerializeField, Header("スワイプ回転解除範囲　最大")] private Vector2 rotateCancellRangeMax;
    [SerializeField, Header("スワイプによる回転からもとに戻るまでの時間")] private float restoreTime = 1.0f;
    private Vector3 restoreTargetPos;            // 戻る位置
    private float restoreAngleY, restoreAngleXZ; // 戻るときの回転量

    // 拡縮
    private float sDist = 0.0f, nDist = 0.0f;  //距離変数
    private float vRatio = 1.0f;               //現在倍率
    [SerializeField, Header("Field of Viewの最大・最低値")] private float vMax, vMin;
    [SerializeField, Header("Field of Viewの初期値")] private float vDefault;
    [SerializeField, Header("基本の拡縮速度")] private float vSpeed;

    // ダブルタップ
    private bool is90Rotate = false;    // 回転できるか
    [SerializeField, Header("ダブルタップ除外範囲　最小")] private Vector2 cantDoubleTapRangeMin;
    [SerializeField, Header("ダブルタップ除外範囲　最大")] private Vector2 cantDoubleTapRangeMax;
    [SerializeField, Header("ダブルタップ時の回転にかかる時間")] private float rotateTime = 1.0f;
    private bool isRotating = false;    // 回転中か
    private Tween rotateTween = null;   // Tween 一時停止できるように保持
    private bool isRotateStart = false; // 回転開始のフラグ

    // 90度毎の補正位置・回転 画面端をダブルタップでこれらの位置に移動する
    private Dictionary<CameraPos, Vector3> adjustPoint_DoubleTap = new();
    private Dictionary<CameraPos, Quaternion> adjustQuaternion_DoubleTap = new();

    // 移動方向
    private enum TeleportDir { UP, DOWN, RIGHT, LEFT, NULL }; // 四方向 上下左右
    private TeleportDir teleportDir    = TeleportDir.NULL;    // 移動方向
    private TeleportDir firstInputDir  = TeleportDir.NULL;    // 一回目に入力した移動方向
    private TeleportDir secondInputDir = TeleportDir.NULL;    // 二回目に入力した移動方向

    private enum CameraPos { UP, BACK, RIGHT, FRONT, LEFT, NULL }; // 五方向　前後左右上
    private CameraPos currentCameraPos = CameraPos.UP;             // 現在のカメラ位置
    private CameraPos nextCameraPos    = CameraPos.NULL;          // 次のカメラ位置

    // 実行フェーズ時のカメラ位置
    [SerializeField, Header("実行フェーズ時 カメラと水平軸のなす角度")] private float play_Angle;

    WaitUntil untilRestored;

    public void Initialize()
    {
        // キャッシュ
        thisTransform = GetComponent<Transform>();
        untilRestored = new WaitUntil(() => !isRestoring);

        adjustQuaternion_DoubleTap[CameraPos.UP]    = Quaternion.Euler(90, 0, 180);
        adjustQuaternion_DoubleTap[CameraPos.BACK]  = Quaternion.Euler(180, 180, 180);
        adjustQuaternion_DoubleTap[CameraPos.RIGHT] = Quaternion.Euler(180, -90, 180);
        adjustQuaternion_DoubleTap[CameraPos.FRONT] = Quaternion.Euler(180, 0, 180);
        adjustQuaternion_DoubleTap[CameraPos.LEFT]  = Quaternion.Euler(180, 90, 180);

        tx = 0;
        ty = 0;

        isRestoring = false;
        isRotating = false;
        is90Rotate = false;

        firstInputDir  = TeleportDir.NULL;
        secondInputDir = TeleportDir.NULL;
        teleportDir    = TeleportDir.NULL;

        currentCameraPos = CameraPos.UP;
        nextCameraPos    = CameraPos.NULL;

        // 感度設定
        SetSensitivity();

        // サイズ代入
        if (stageController) mapSize = stageController.MapSize;

        // 注視位置設定
        target = mapSize / 2;
        target.x *= -1;                        // X軸の方向と反対向きにオブジェクト生成しているので-1をかける
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f); // ズレを補正

        // サンプルのサイズに応じてカメラの初期位置を調整
        thisTransform.position = new Vector3(target.x, mapSize.x + dir + MIN_Y, target.z);

        // カメラが動く球面の半径を算出
        range = (target - thisTransform.position).magnitude;

        // スワイプの補正ポイントの生成
        Vector3 firstPos = new Vector3(range, MIN_Y, 0);
        adjustPoint_Swip = new Vector3[(int)(360 / adjustAngle_Swip)];
        for (int i = 0; i < adjustPoint_Swip.Length; i++)
        {
            adjustPoint_Swip[i] = Quaternion.Euler(0, i * adjustAngle_Swip, 0) * firstPos + target;
        }

        // ダブルタップで移動するポイントの作成
        adjustPoint_DoubleTap.Add(CameraPos.UP, thisTransform.position);
        for (int i = 1; i < 5; i++)
        {
            CameraPos c = (CameraPos)Enum.ToObject(typeof(CameraPos), i);
            adjustPoint_DoubleTap.Add(c, Quaternion.Euler(0, i * 90, 0) * firstPos + target);
        }

        // 注視
        thisTransform.LookAt(target, thisTransform.up);
    }

    private void Update()
    {
        TweenPauseControll();
    }

    public void CameraUpdate()
    {
        if(!stageController.IsPause)
        {
            if (isRestoring)
            {
                Restore();
                AdjustCameraToTarget();
            }

            // 回転演出中は操作不可
            if (!isRotating && !isRestoring)
            {
                Swip();
                Scaling();
                DoubleTap();
            }
        }
    }

    bool isLeftSwip = false, isRightSwip = false;

    /// <summary>
    /// スワイプ
    /// </summary>
    private void Swip()
    {
        if (stageController.IsTutorial)
        {
            if (tutorial.MethodName != "CheckB" && tutorial.MethodName != "CheckC2" && !tutorial.TutorialCompleteByPhase && !tutorial.IsTutorialComplete)
                return;
        }
        
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
                    ((stageController.IsTutorial && (tutorial.TutorialCompleteByPhase || tutorial.MethodName == "CheckC2")) ||
                    !stageController.IsTutorial))
                {
                    restoreTargetPos = adjustPoint_DoubleTap[currentCameraPos];
                    RestoreStart();
                }
            }
            else if (t.phase == TouchPhase.Moved)
            {
                if (tutorial.MethodName == "CheckC2") return;

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

                    if (tx > 5)  isRightSwip = true;
                    if (tx < -5) isLeftSwip = true;
                }

                // 移動量から回転角度を求める
                // 始点からの移動量の絶対値が、dragAjustより小さかったら0にし、水平/垂直の移動にする
                if (Mathf.Abs(startPos.x - t.position.x) < dragAdjust) tx = 0;
                float deltaAngleLR = tx / Screen.width * sensitivity;

                if (Mathf.Abs(startPos.y - t.position.y) < dragAdjust) ty = 0;
                float deltaAngleTB = -ty / Screen.height * sensitivity;

                if (tx != 0 || ty != 0) didSwip = true;

                var up = thisTransform.up;
                var right = thisTransform.right;

                // 地面を移動する場合は回転軸を変更する
                if (thisTransform.position.y <= MIN_Y && ty == 0)
                {
                    up = Vector3.up;
                    right = thisTransform.rotation * Vector3.left;
                }

                // カメラから見て上・右向きのベクトルを回転軸として回転させる
                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, up); // 左右方向
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, right); // 上下方向

                var pos = thisTransform.position;
                pos -= target;
                pos = angleAxisTB * angleAxisLR * pos; // 回転移動
                pos += target; // 平行移動

                if (pos.y >= MIN_Y)
                {
                    thisTransform.position = pos;
                    thisTransform.LookAt(target, thisTransform.up);
                }
                else
                {
                    pos.y = MIN_Y;
                    thisTransform.position = pos;
                    thisTransform.LookAt(target, thisTransform.up);

                    AdjustCameraToClosestPoint();
                }

                lastTapPos = t.position;
            }

            else if (t.phase == TouchPhase.Ended)
            {
                if (stageController.IsTutorial)
                {
                    // 右ドラッグで次へ
                    if (tutorial.IsCheckB_1 && !tutorial.ToCheckB_2 && isRightSwip) tutorial.ToCheckB_2 = true;

                    // 左ドラッグで次へ
                    if (tutorial.IsCheckB_2 && !tutorial.ToCheckC && isLeftSwip) tutorial.ToCheckC = true;
                }
            }
        }
    }

    /// <summary>
    /// 拡縮
    /// </summary>
    private void Scaling()
    {
        if (stageController.IsTutorial && !tutorial.TutorialCompleteByPhase && !tutorial.IsTutorialComplete) return;

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

                vRatio = m_camera.fieldOfView;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                nDist = Vector2.Distance(t1.position, t2.position);

                // 縮小
                if (sDist > nDist) vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin)) * (sDist - nDist);
                // 拡大
                if (sDist < nDist) vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin)) * (nDist - sDist);

                m_camera.fieldOfView = vRatio;
                sDist = nDist;

                thisTransform.LookAt(target, thisTransform.up);
            }
        }
    }

    /// <summary>
    /// オブジェクトから目を離さないように注視点を設定
    /// </summary>
    private void AdjustCameraToTarget()
    {
        if (currentCameraPos == CameraPos.UP)
        {
            thisTransform.LookAt(target, Vector3.back);
        }
        else
        {
            thisTransform.LookAt(target, Vector3.up);
        }
    }

    /// <summary>
    /// カメラがオブジェクトの下に行かないように調整する
    /// </summary>
    private void AdjustCameraToClosestPoint()
    {
        Vector3 nearestPos = new Vector3(100, 100, 100);
        var currentPosition = thisTransform.position;

        // もっとも近いポイントを探す
        foreach (var p in adjustPoint_Swip)
        {
            if ((currentPosition - nearestPos).sqrMagnitude > (currentPosition - p).sqrMagnitude)
            {
                nearestPos = p;
            }
        }

        thisTransform.position = nearestPos;
        thisTransform.LookAt(target, thisTransform.up);
    }

    /// <summary>
    /// ダブルタップで90度回転 前後左右に移動する
    /// </summary>
    private void DoubleTap()
    {
        if (stageController.IsTutorial)
        {
            if (tutorial.MethodName != "CheckD" && !tutorial.TutorialCompleteByPhase && !tutorial.IsTutorialComplete)
                return;
        }

        tapManager.DoubleTap(
            () =>
            {
                // 一回目のタップ
                Touch t = Input.GetTouch(0);

                // 範囲外は無効
                if (!tapManager.TapOrDragRange(t.position) ||
                    tapManager.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax))
                    return;

                // 入力方向を保存
                if (firstInputDir == TeleportDir.NULL)
                    firstInputDir = DoubleTapPosJudgement(t.position);
            },

            () =>
            {
                // 二回目のタップ
                Touch t = Input.GetTouch(0);

                // 範囲外は無効
                if (!tapManager.TapOrDragRange(t.position) ||
                    tapManager.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax))
                    return;

                // 入力方向を保存
                if (secondInputDir == TeleportDir.NULL)
                    secondInputDir = DoubleTapPosJudgement(t.position);

                if (firstInputDir == secondInputDir && firstInputDir != TeleportDir.NULL && secondInputDir != TeleportDir.NULL)
                    teleportDir = firstInputDir;
                else
                {
                    firstInputDir = TeleportDir.NULL;
                    secondInputDir = TeleportDir.NULL;
                }

                is90Rotate = true;
            },

            () =>
            {
                // 時間内に二度目のタップが無かった場合
                firstInputDir = TeleportDir.NULL;
                secondInputDir = TeleportDir.NULL;
            });

        if (is90Rotate)
        {
            if (stageController.IsTutorial && !tutorial.IsTutorialComplete)
            {
                // チュートリアル 確認フェーズD
                // 右方向にのみダブルタップで移動可能
                // 移動したらチュートリアルを進める
                if ((tutorial.ToCheckD && !tutorial.ToCheckE && teleportDir == TeleportDir.RIGHT) || tutorial.TutorialCompleteByPhase)
                {
                    tutorial.IsCheckD = true;
                }
                else
                {
                    is90Rotate = false;
                    firstInputDir = TeleportDir.NULL;
                    secondInputDir = TeleportDir.NULL;
                    return;
                }
            }

            // 現在のカメラ位置から見た上下左右を求める
            // カメラがサンプルの上側にあるとき、『上』はサンプル後方、『下』はサンプル前方になる
            CameraPos up    = CameraPos.NULL, 
                      down  = CameraPos.NULL, 
                      left  = CameraPos.NULL, 
                      right = CameraPos.NULL;

            switch (currentCameraPos)
            {
                case CameraPos.UP:
                    up    = CameraPos.BACK;
                    down  = CameraPos.FRONT;
                    right = CameraPos.RIGHT;
                    left  = CameraPos.LEFT;
                    break;

                case CameraPos.FRONT:
                    up    = CameraPos.UP;
                    down  = CameraPos.NULL;
                    right = CameraPos.RIGHT;
                    left  = CameraPos.LEFT;
                    break;

                case CameraPos.BACK:
                    up    = CameraPos.UP;
                    down  = CameraPos.NULL;
                    right = CameraPos.LEFT;
                    left  = CameraPos.RIGHT;
                    break;

                case CameraPos.RIGHT:
                    up    = CameraPos.UP;
                    down  = CameraPos.NULL;
                    right = CameraPos.BACK;
                    left  = CameraPos.FRONT;
                    break;

                case CameraPos.LEFT:
                    up    = CameraPos.UP;
                    down  = CameraPos.NULL;
                    right = CameraPos.FRONT;
                    left  = CameraPos.BACK;
                    break;
            }

            if(currentCameraPos != CameraPos.UP)
            {
                if (didSwip && tx > 0)
                    right = currentCameraPos;

                else if (didSwip && tx < 0)
                    left  = currentCameraPos;

                else if (didSwip && ty > 0)
                    down  = currentCameraPos;
            }
            else
            {
                if (didSwip && ty > 0)
                    up = currentCameraPos;
                else if (didSwip && ty < 0)
                    down = currentCameraPos;
            }

            RotateDirSet(up, down, right, left);

            is90Rotate = false;
            firstInputDir = TeleportDir.NULL;
            secondInputDir = TeleportDir.NULL;
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

    /// <summary>
    /// 90度回転
    /// </summary>
    /// <returns></returns>
    private IEnumerator Rotate90Degrees()
    {
        if (didSwip)
        {
            // カメラをサンプルの前後左右に移動させるとき
            if ((adjustPoint_DoubleTap[currentCameraPos].y == MIN_Y && adjustPoint_DoubleTap[nextCameraPos].y == MIN_Y) ||
                (tx == 0 && ty < 0 && currentCameraPos == CameraPos.FRONT))
            {
                // カメラをDoTweenを使わずに移動させる
                restoreTargetPos = adjustPoint_DoubleTap[nextCameraPos];
                RestoreStart();
                yield return untilRestored;

                if(nextCameraPos == CameraPos.UP)
                    thisTransform.LookAt(target, Vector3.back);

                currentCameraPos = nextCameraPos;
                yield break;
            }

            else
            {
                restoreTargetPos = adjustPoint_DoubleTap[currentCameraPos];
                RestoreStart();
                yield return untilRestored;
            }
        }

        isRotating = true;
        var sequence = DOTween.Sequence();

        Vector3[] controllPoint = GetControllPoint(); // 制御点取得

        Tween move = thisTransform.DOLocalPath(new[] { adjustPoint_DoubleTap[nextCameraPos], controllPoint[0], controllPoint[1] }, rotateTime, PathType.CubicBezier).SetOptions(false);

        if (currentCameraPos != nextCameraPos)
        {
            if ((currentCameraPos != CameraPos.UP && nextCameraPos != CameraPos.UP) ||
                (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.FRONT) ||
                (currentCameraPos == CameraPos.FRONT && nextCameraPos == CameraPos.UP))
            {
                sequence
                    .Join(move)
                    .Join(thisTransform.DORotateQuaternion(adjustQuaternion_DoubleTap[nextCameraPos], rotateTime));
            }

            // 上←→左/右/後のときは回転角度を指定して回転させる
            else
            {
                var angleXZ = new Vector3(0.0f, 0.0f, 0.0f); // X軸、Z軸の回転
                var angleY = new Vector3(0.0f, 0.0f, 0.0f); // Y軸の回転

                // 回転角度取得
                if (currentCameraPos == CameraPos.BACK && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(90.0f, 0.0f, 0.0f);
                    angleY = new Vector3(0.0f, -180.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.BACK)
                {
                    angleXZ = new Vector3(-90.0f, 0.0f, 0.0f);
                    angleY = new Vector3(0.0f, -180.0f, 0.0f);
                }

                else if (currentCameraPos == CameraPos.RIGHT && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, -90.0f);
                    angleY = new Vector3(0.0f, 90.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.RIGHT)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, 90.0f);
                    angleY = new Vector3(0.0f, -90.0f, 0.0f);
                }

                else if (currentCameraPos == CameraPos.LEFT && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, 90.0f);
                    angleY = new Vector3(0.0f, -90.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.LEFT)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, -90.0f);
                    angleY = new Vector3(0.0f, 90.0f, 0.0f);
                }

                // 回転
                if (nextCameraPos == CameraPos.UP)
                {
                    sequence
                        .Join(thisTransform.DORotate(angleXZ, rotateTime, RotateMode.WorldAxisAdd))
                        .Join(move)
                        .Append(thisTransform.DORotate(angleY, rotateTime / 2.0f, RotateMode.WorldAxisAdd));
                }
                else
                {
                    sequence
                        .Join(thisTransform.DORotate(angleY, rotateTime / 2.0f, RotateMode.WorldAxisAdd))
                        .Append(move)
                        .Join(thisTransform.DORotate(angleXZ, rotateTime, RotateMode.WorldAxisAdd));
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
    private void RestoreStart()
    {
        isRestoring = true;

        Vector3 restoreNormalVec = CalcRestoreVec();

        var from = thisTransform.position - target;
        var to = restoreTargetPos - target;

        // 平面にベクトルを投影し、Y軸回転/XまたはZ軸回転の角度を求める
        restoreAngleY  = ProjectionAngle(from, to, Vector3.up);       // XZ平面に投影
        restoreAngleXZ = ProjectionAngle(from, to, restoreNormalVec); // XY/ZY平面に投影

        if (restoreAngleXZ > 90)
        {
            restoreAngleXZ = (180 - restoreAngleXZ) * -1;
        }
    }

    private void Restore()
    {
        // targetを中心とした反時計回りのベクトルを毎フレーム計算し直す
        Vector3 restoreNormalVec = CalcRestoreVec();

        var ratio = Time.deltaTime / restoreTime;

        // 行列の作成
        var angleAxisY  = Quaternion.AngleAxis(restoreAngleY * ratio,  Vector3.up);
        var angleAxisXZ = Quaternion.AngleAxis(restoreAngleXZ * ratio, restoreNormalVec);

        // 移動
        var pos = thisTransform.position;
        pos -= target;
        pos = angleAxisXZ * angleAxisY * pos; // 回転移動
        pos += target; // 平行移動

        // 一定以下まで近付いたら回転終了
        if ((restoreTargetPos - thisTransform.position).sqrMagnitude < 1.0f)
        {
            isRestoring = false;
            thisTransform.position = restoreTargetPos;
            tx = 0;
            ty = 0;
            didSwip = false;

            if (stageController.IsTutorial && tutorial.MethodName == "CheckC2")
            {
                tutorial.ToCheckD = true;
            }
        }
        else
        {
            if (pos.y < MIN_Y) pos.y = MIN_Y;
            thisTransform.position = pos;
        }
    }

    private Vector3 CalcRestoreVec()
    {
        Vector3 vec = Vector3.zero;

        // オブジェクト前方にある補正位置と現在位置のなす角度を求める　⇒　Y軸回転の量を算出
        var planeFrom = Vector3.ProjectOnPlane(adjustPoint_DoubleTap[CameraPos.FRONT] - target, Vector3.up);
        var planeTo = Vector3.ProjectOnPlane(thisTransform.position - target, Vector3.up);
        var angle = Vector3.SignedAngle(planeFrom, planeTo, Vector3.up);

        if (thisTransform.position.y <= MIN_Y && restoreTargetPos.y <= MIN_Y)
        {
            vec = thisTransform.rotation * Vector3.left;
        }
        else
        {
            // targetを中心とした反時計回りのベクトル
            vec = (Quaternion.Euler(0, angle, 0) * Vector3.left).normalized;
        }

        return vec;
    }

    private Vector3[] GetControllPoint()
    {
        Vector3 relay = Vector3.zero;
        Vector3[] controllPoint = new Vector3[2]; // 制御点

        if (adjustPoint_DoubleTap[currentCameraPos].x == adjustPoint_DoubleTap[nextCameraPos].x) relay.x = adjustPoint_DoubleTap[currentCameraPos].x;
        else relay.x = (Mathf.Abs(target.x - adjustPoint_DoubleTap[currentCameraPos].x) >= 1f) ? adjustPoint_DoubleTap[currentCameraPos].x : adjustPoint_DoubleTap[nextCameraPos].x;

        if (adjustPoint_DoubleTap[currentCameraPos].y == adjustPoint_DoubleTap[nextCameraPos].y) relay.y = adjustPoint_DoubleTap[currentCameraPos].y;
        else relay.y = (Mathf.Abs(target.y - (adjustPoint_DoubleTap[currentCameraPos].y - MIN_Y)) >= 1f) ? adjustPoint_DoubleTap[currentCameraPos].y : adjustPoint_DoubleTap[nextCameraPos].y;

        if (adjustPoint_DoubleTap[currentCameraPos].z == adjustPoint_DoubleTap[nextCameraPos].z) relay.z = adjustPoint_DoubleTap[currentCameraPos].z;
        else relay.z = (Mathf.Abs(target.z - adjustPoint_DoubleTap[currentCameraPos].z) >= 1f) ? adjustPoint_DoubleTap[currentCameraPos].z : adjustPoint_DoubleTap[nextCameraPos].z;

        controllPoint[0] = Vector3.Lerp(adjustPoint_DoubleTap[currentCameraPos], relay, 0.5f);
        controllPoint[1] = Vector3.Lerp(adjustPoint_DoubleTap[nextCameraPos], relay, 0.5f);

        return controllPoint;
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
        m_camera.fieldOfView = vDefault;

        StartCoroutine(RotateResetCoroutine());
    }

    /// <summary>
    /// カメラの位置・回転を初期状態に戻すコルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotateResetCoroutine()
    {
        // スワイプしていたら戻す
        if (didSwip)
        {
            restoreTargetPos = adjustPoint_DoubleTap[currentCameraPos];
            RestoreStart();
            yield return untilRestored;
        }

        // 戻り切ったら、オブジェクト上部へ移動
        if (currentCameraPos != CameraPos.UP)
        {
            StartCoroutine(Rotate90Degrees());
            isRotateStart = true;
        }
    }

    /// <summary>
    /// 現在のカメラ位置から最も近い90度毎の補正ポイントを返す
    /// </summary>
    /// <returns></returns>
    private CameraPos GetCameraToClosest90Point()
    {
        Vector3 nearestPos = new Vector3(100, 100, 100);
        CameraPos cameraPos = CameraPos.NULL;
        var currentPosition = thisTransform.position;

        foreach (var p in adjustPoint_DoubleTap)
        {
            if ((currentPosition - nearestPos).sqrMagnitude > (currentPosition - p.Value).sqrMagnitude)
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

        thisTransform.rotation = Quaternion.Euler(0, 0, 0);

        var angle = 90 - play_Angle;

        float buttom = target.z + range * Mathf.Cos(angle * Mathf.Deg2Rad);
        float height = range * Mathf.Sin(angle * Mathf.Deg2Rad);

        thisTransform.position = new Vector3(target.x, buttom, height);
        thisTransform.LookAt(target, thisTransform.up);

        didSwip = true;
        currentCameraPos = CameraPos.UP;

        TapReset();
    }

    /// <summary>
    /// 実行フェーズから確認フェーズに戻ったときにカメラを移動
    /// </summary>
    public void FromPlayPhase()
    {
        //currentCameraPos = CameraPos.UP;
        //didSwip = true;
        RotateReset(false);
        TapReset();
    }

    /// <summary>
    /// Tweenの一時停止/再開
    /// </summary>
    private void TweenPauseControll()
    {
        // 90度回転の一時停止/再開
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
    }

    /// <summary>
    /// 感度設定
    /// 設定画面を閉じたときに呼ぶ
    /// </summary>
    public void SetSensitivity()
    {
        sensitivity = sensChenger.sensitivity;
        if (sensitivity == 0) sensitivity = 80;
    }

    /// <summary>
    /// タップの状態をリセット
    /// </summary>
    public void TapReset()
    {
        tapManager.DoubleTapReset();
    }
}
