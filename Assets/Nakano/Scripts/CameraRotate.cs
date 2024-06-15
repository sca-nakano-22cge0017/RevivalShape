using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 確認フェーズ　カメラ制御
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField, Header("デバッグ用")] SampleCheck sampleCheck;

    [SerializeField] StageController stageController;
    [SerializeField] Camera _camera;
    Vector3 mapSize;

    // 注視位置
    Vector3 target;

    // 初期位置
    Vector3 defaultPos;
    Quaternion defaultRot;

    // これらはカメラのY座標を0に補正できるようにするための変数
    [SerializeField, Header("ajustAngle度毎に補正ポイントを置く")] float adjustAngle;
    Vector3[] point; // カメラ位置の補正 一定距離まで近付いたらこの座標に移動させる

    // 回転
    [SerializeField, Header("ドラッグの感度")] float sensitivity;
    Vector2 sPos;    // タップの始点
    Vector2 lastTapPos;  // 前フレームのタップ位置
    float wid, hei;  // スクリーンサイズ
    float tx, ty;
    Vector3 lastPos; // 回転開始前の座標
    Quaternion lastRot; // 回転開始前の角度

    // 拡縮
    float sDist = 0.0f, nDist = 0.0f;  //距離変数
    float vRatio = 1.0f;               //現在倍率
    [SerializeField, Header("Field of Viewの最大・最低値")] float vMax, vMin;
    [SerializeField, Header("Field of Viewの初期値")] float vDefault;
    [SerializeField, Header("基本の拡縮速度")] float vSpeed;

    [SerializeField, Header("スワイプ 手振れ補正値"), Tooltip("斜め方向へのスワイプをx軸/y軸に真っ直ぐな移動に補正する")]
    float dragAdjust = 10;

    Vector2[] dragRangeVertex = new Vector2[5]; // タップ可能範囲の中心と4頂点

    // ダブルタップ
    int tapCount = 0;
    bool canJudgement = false;
    bool isDoubleTapStart = false;
    float doubleTapTime = 0;
    bool is90Rotate = false;

    // 移動方向
    enum TeleportDir { UP, DOWN, RIGHT, LEFT, NULL };
    TeleportDir teleportDir = TeleportDir.NULL;

    TeleportDir inputDir1 = TeleportDir.NULL; // 一回目の龍力
    TeleportDir inputDir2 = TeleportDir.NULL; // 二回目の龍力

    enum CameraPos { FRONT, BACK, RIGHT, LEFT, UP, NULL };
    CameraPos nowCameraPos = CameraPos.FRONT; // 現在のカメラ位置
    CameraPos nextCameraPos = CameraPos.NULL; // 次のカメラ位置

    // 90度毎の補正位置 画面端をダブルタップでこれらの位置に移動する
    Vector3[] adjustPoint_90 = new Vector3[5];

    // タップ/スワイプ可能範囲を描画
    [SerializeField] Texture _texture;
    [SerializeField] bool isDragRangeDraw = false;

    [SerializeField, Header("スワイプの範囲 最小")] Vector2 dragRangeMin;
    [SerializeField, Header("スワイプの範囲 最大")] Vector2 dragRangeMax;

    /// <summary>
    /// カメラを動かすかどうか
    /// falseのときは動かない
    /// </summary>
    public bool CanRotate { get; set; } = false;

    void Awake()
    {
        wid = Screen.width;
        hei = Screen.height;

        point = new Vector3[(int)(360 / adjustAngle)];

        // タップ/スワイプできる範囲を取得
        Vector2 rangeMin = stageController.GetTapOrDragRange()[0];
        Vector2 rangeMax = stageController.GetTapOrDragRange()[1];
        dragRangeVertex[0] = new Vector2((rangeMax.x + rangeMin.x) / 2, (rangeMax.y + rangeMin.y) / 2);
        dragRangeVertex[1] = new Vector2(rangeMin.x, rangeMin.y);
        dragRangeVertex[2] = new Vector2(rangeMax.x, rangeMin.y);
        dragRangeVertex[3] = new Vector2(rangeMin.x, rangeMax.y);
        dragRangeVertex[4] = new Vector2(rangeMax.x, rangeMax.y);

        Application.targetFrameRate = 120;
    }

    void Update()
    {
        if (!CanRotate) return;

        DoubleTapRotate();

        // 回転
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            
            if (t.phase == TouchPhase.Began)
            {
                // 範囲外は無効
                if (!stageController.TapOrDragRange(t.position)) return;

                sPos = t.position;
                lastTapPos = t.position;

                lastPos = transform.position;
                lastRot = transform.rotation;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                // 範囲外からスワイプしたとき用の調整
                if (!stageController.TapOrDragRange(t.position))
                {
                    sPos = t.position;
                    lastTapPos = t.position;

                    RotateRestore();
                }

                // スワイプ量
                tx = t.position.x - lastTapPos.x;
                ty = t.position.y - lastTapPos.y;

                // 移動量から回転角度を求める
                // 始点からの移動量の絶対値が、dragAjustより小さかったら0にし、水平/垂直の移動にする
                if (Mathf.Abs(sPos.x - t.position.x) < dragAdjust) tx = 0;
                float deltaAngleLR = tx / wid * sensitivity;

                if (Mathf.Abs(sPos.y - t.position.y) < dragAdjust) ty = 0;
                float deltaAngleTB = -ty / hei * sensitivity;

                // カメラから見て上・右向きのベクトルを回転軸として回転させる
                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up); // 左右方向
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right); // 上下方向

                var pos = transform.position;
                pos -= target;
                pos = angleAxisLR * angleAxisTB * pos; // 回転移動
                pos += target;                          // 平行移動

                transform.position = pos;
                transform.LookAt(target, transform.up);

                // カメラがサンプルの下に回り込まないように調整
                if (pos.y <= 0.005f && pos.y != 0)
                {
                    CameraPosAdjust();
                }

                lastTapPos = t.position;
            }
            else if (t.phase == TouchPhase.Ended)
            {
                // 範囲外は無効
                if (!stageController.TapOrDragRange(t.position)) return;

                RotateRestore();
            }
        }

        // 拡縮
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            // 範囲外は無効
            if (!stageController.TapOrDragRange(t1.position)) return;
            if (!stageController.TapOrDragRange(t2.position)) return;

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
            }
        }
    }

    /// <summary>
    /// スワイプによる回転状態をもとに戻す
    /// </summary>
    void RotateRestore()
    {
        //! Todo 戻るときゆっくりと
        transform.position = lastPos;
        transform.rotation = lastRot;
    }

    /// <summary>
    /// ダブルタップで90度回転 前後左右に移動する
    /// </summary>
    void DoubleTapRotate()
    {
        if (isDoubleTapStart)
        {
            doubleTapTime += Time.deltaTime;
            if(doubleTapTime < 0.2f)
            {
                if(Input.touchCount == 1)
                {
                    Touch t = Input.GetTouch(0);

                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        // 範囲外は無効
                        if (!stageController.TapOrDragRange(t.position)) return;

                        // 中心部分は除外
                        if (stageController.TapOrDragRange(t.position, dragRangeMin, dragRangeMax)) return;

                        // 入力方向を保存
                        inputDir2 = DoubleTapPosJudgement(t.position);

                        if (inputDir1 == inputDir2 && inputDir1 != TeleportDir.NULL && inputDir2 != TeleportDir.NULL)
                            teleportDir = inputDir1;

                        is90Rotate = true;

                        isDoubleTapStart = false;
                        doubleTapTime = 0.0f;
                    }
                }
            }
            else
            {
                isDoubleTapStart = false;
                doubleTapTime = 0.0f;

                inputDir1 = TeleportDir.NULL;
                inputDir2 = TeleportDir.NULL;
            }
        }
        else
        {
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    // 範囲外は無効
                    if (!stageController.TapOrDragRange(t.position)) return;

                    // 中心部分は除外
                    if (stageController.TapOrDragRange(t.position, dragRangeMin, dragRangeMax)) return;

                    // 入力方向を保存
                    inputDir1 = DoubleTapPosJudgement(t.position);

                    isDoubleTapStart = true;
                    doubleTapTime = 0.0f;
                }
            }
        }

        if (is90Rotate)
        {
            // 現在のカメラ位置に応じて上下左右を決める
            switch (nowCameraPos)
            {
                case CameraPos.UP:
                    SetNextCameraPos(CameraPos.BACK, CameraPos.FRONT, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.FRONT:
                    SetNextCameraPos(CameraPos.UP, CameraPos.NULL, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.BACK:
                    SetNextCameraPos(CameraPos.UP, CameraPos.NULL, CameraPos.LEFT, CameraPos.RIGHT);
                    break;
                case CameraPos.RIGHT:
                    SetNextCameraPos(CameraPos.UP, CameraPos.NULL, CameraPos.BACK, CameraPos.FRONT);
                    break;
                case CameraPos.LEFT:
                    SetNextCameraPos(CameraPos.UP, CameraPos.NULL, CameraPos.FRONT, CameraPos.BACK);
                    break;
            }

            is90Rotate = false;
            tapCount = 0;
            inputDir1 = TeleportDir.NULL;
            inputDir2 = TeleportDir.NULL;
        }
    }

    /// <summary>
    /// 現在のカメラ位置に応じて上下左右方向のカメラ位置を決める
    /// </summary>
    /// <param name="_up">上</param>
    /// <param name="_down">下</param>
    /// <param name="_right">右</param>
    /// <param name="_left">左</param>
    void SetNextCameraPos(CameraPos _up, CameraPos _down, CameraPos _right, CameraPos _left)
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

        switch (nextCameraPos)
        {
            case CameraPos.UP:
                transform.position = adjustPoint_90[0];
                transform.LookAt(target);
                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.BACK:
                transform.position = adjustPoint_90[1];
                transform.LookAt(target);
                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.RIGHT:
                transform.position = adjustPoint_90[2];
                transform.LookAt(target);
                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.FRONT:
                transform.position = adjustPoint_90[3];
                transform.LookAt(target);
                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.LEFT:
                transform.position = adjustPoint_90[4];
                transform.LookAt(target);
                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.NULL:
                break;
        }
    }

    /// <summary>
    /// タップ位置が上下左右の範囲のどこにあるかを判定し、移動方向を返す
    /// </summary>
    /// <param name="_pos">タップ位置</param>
    /// <returns>方向を返す</returns>
    TeleportDir DoubleTapPosJudgement(Vector2 _pos)
    {
        // Input.mousepositionは原点が違うので調整
        _pos.y *= -1;
        _pos.y += hei;

        // 上部分
        if (InsideOrOutsideJudgement(dragRangeVertex[2], dragRangeVertex[1], dragRangeVertex[0], _pos))
            return TeleportDir.UP;

        // 下部分
        if (InsideOrOutsideJudgement(dragRangeVertex[3], dragRangeVertex[4], dragRangeVertex[0], _pos))
            return TeleportDir.DOWN;

        // 右部分
        if (InsideOrOutsideJudgement(dragRangeVertex[4], dragRangeVertex[2], dragRangeVertex[0], _pos))
            return TeleportDir.RIGHT;

        // 左部分
        if (InsideOrOutsideJudgement(dragRangeVertex[1], dragRangeVertex[3], dragRangeVertex[0], _pos))
            return TeleportDir.LEFT;

        return TeleportDir.NULL;
    }

    /// <summary>
    /// 外積による内外判定
    /// </summary>
    /// <param name="_vertexLeft">底辺 左側の頂点</param>
    /// <param name="_vertexRight">底辺 右側の頂点</param>
    /// <param name="_vertexTop">上部の頂点</param>
    /// <param name="_judgePos">判定する位置</param>
    /// <returns>三角形の内部にあればtrue</returns>
    bool InsideOrOutsideJudgement(Vector2 _vertexLeft, Vector2 _vertexRight, Vector2 _vertexTop, Vector2 _judgePos)
    {
        bool isInside = false;

        // ベクトル
        Vector3 leftToTop = _vertexTop - _vertexLeft;
        Vector3 leftToRight = _vertexRight - _vertexLeft;
        Vector3 rightToTop = _vertexTop - _vertexRight;
        Vector3 leftToJudge = _judgePos - _vertexLeft;
        Vector3 rightToJudge = _judgePos - _vertexRight;

        // 外積計算
        Vector3 cross1 = Vector3.Cross(leftToJudge, leftToTop);
        Vector3 cross2 = Vector3.Cross(leftToRight, leftToJudge);
        Vector3 cross3 = Vector3.Cross(rightToTop, rightToJudge);

        // 外積のzの値の符号が同じなら三角形の内部に点が存在する
        if ((cross1.z > 0 && cross2.z > 0 && cross3.z > 0) || (cross1.z < 0 && cross2.z < 0 && cross3.z < 0))
            isInside = true;

        return isInside;
    }

    /// <summary>
    /// カメラ位置を補正する
    /// </summary>
    void CameraPosAdjust()
    {
        Vector3 nearPos = new Vector3(100, 100, 100);

        var pos = transform.position;

        foreach (var p in point)
        {
            var nearDiff = (pos - nearPos).magnitude;
            var diff = (pos - p).magnitude;

            if (nearDiff > diff) nearPos = p;
        }

        transform.position = nearPos;
        transform.LookAt(target, transform.up);
    }

    /// <summary>
    /// カメラの位置・回転を初期状態に戻す
    /// </summary>
    public void RotateReset()
    {
        transform.position = adjustPoint_90[0];
        transform.rotation = defaultRot;

        _camera.fieldOfView = vDefault;

        transform.LookAt(target, transform.up);
    }

    /// <summary>
    /// カメラの注視位置を設定
    /// </summary>
    public void TargetSet()
    {
        // サイズ代入
        if (stageController)
            mapSize = stageController.MapSize;
        else if (sampleCheck)
            mapSize = sampleCheck.MapSize;

        // サンプルのサイズに応じてカメラ位置を調整
        transform.position = new Vector3(-mapSize.x / 2 + 0.5f, mapSize.x + 12, mapSize.z / 2);
        defaultRot = transform.rotation;

        // 注視位置設定
        target = mapSize / 2;
        target.x *= -1;
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f);
        
        transform.LookAt(target, transform.up);

        nowCameraPos = CameraPos.UP;
        nextCameraPos = CameraPos.NULL;

        // 補正ポイントの生成
        float r = (target - transform.position).magnitude;
        Vector3 firstPos = new Vector3(r, -0.5f, 0);
        for (int i = 0; i < point.Length; i++)
        {
            point[i] = Quaternion.Euler(0, i * adjustAngle, 0) * firstPos + target;
        }

        // 90度毎の補正ポイントの作成
        adjustPoint_90[0] = transform.position;
        
        for (int i = 1; i < adjustPoint_90.Length; i++)
        {
            adjustPoint_90[i] = Quaternion.Euler(0, i * 90, 0) * firstPos + target;
        }

        //! Todo サイズに応じて、Field of Viewの初期値・最大値・最小値も変更
    }

    private void OnGUI()
    {
        if (isDragRangeDraw)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.red);
            texture.Apply();
            _texture = texture;

            var rect = new Rect(dragRangeMin.x, dragRangeMin.y, dragRangeMax.x - dragRangeMin.x, dragRangeMax.y - dragRangeMin.y);
            GUI.DrawTexture(rect, _texture);
        }
    }
}
