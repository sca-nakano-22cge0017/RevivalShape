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
    Vector2 lastPos;  // 前フレームのタップ位置
    float wid, hei;  // スクリーンサイズ
    float tx, ty;

    // 拡縮
    float sDist = 0.0f, nDist = 0.0f;  //距離変数
    float vRatio = 1.0f;               //現在倍率
    [SerializeField, Header("Field of Viewの最大・最低値")] float vMax, vMin;
    [SerializeField, Header("Field of Viewの初期値")] float vDefault;
    [SerializeField, Header("基本の拡縮速度")] float vSpeed;

    [SerializeField, Header("スワイプ 手振れ補正値"), Tooltip("斜め方向へのスワイプをx軸/y軸に真っ直ぐな移動に補正する")]
    float dragAdjust = 10;

    [SerializeField, Header("回転・縮小ができる範囲 最小")] Vector2 dragRangeMin;
    [SerializeField, Header("回転・縮小ができる範囲 最大")] Vector2 dragRangeMax;

    Vector2[] dragRangeVertex = new Vector2[5]; // タップ可能範囲の中心と4頂点

    // ダブルタップ
    int tapCount = 0;
    bool canJudgement = false;
    float doubleTapTime = 0;
    bool is90Rotate = false;

    DoubleTap_TeleportDir inputDir1 = DoubleTap_TeleportDir.NULL; // 一回目の龍力
    DoubleTap_TeleportDir inputDir2 = DoubleTap_TeleportDir.NULL; // 二回目の龍力

    // 移動方向
    enum DoubleTap_TeleportDir { UP, DOWN, RIGHT, LEFT, NULL };
    DoubleTap_TeleportDir teleportDir = DoubleTap_TeleportDir.NULL;

    // 90度毎の補正位置 画面端をダブルタップでこれらの位置に移動する
    Vector3[] adjustPoint_90 = new Vector3[5];

    // タップ/スワイプ可能範囲を描画
    [SerializeField] Texture _texture;
    [SerializeField] bool isDragRangeDraw = false;

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
    }

    void Update()
    {
        if (!CanRotate) return;

        DoubleTapRotate();

        // 回転
        if (Input.touchCount == 1)
        {
            Touch t1 = Input.GetTouch(0);

            if (t1.phase == TouchPhase.Began)
            {
                // 範囲外は無効
                if (!stageController.TapOrDragRange(t1.position, dragRangeMin, dragRangeMax)) return;

                sPos = t1.position;
                lastPos = t1.position;
            }
            else if (t1.phase == TouchPhase.Moved)
            {
                // 範囲外からスワイプしたとき用の調整
                if (!stageController.TapOrDragRange(t1.position, dragRangeMin, dragRangeMax))
                {
                    sPos = t1.position;
                    lastPos = t1.position;
                }

                // スワイプ量
                tx = t1.position.x - lastPos.x;
                ty = t1.position.y - lastPos.y;

                // 移動量から回転角度を求める
                // 始点からの移動量の絶対値が、dragAjustより小さかったら0にし、水平/垂直の移動にする
                if (Mathf.Abs(sPos.x - t1.position.x) < dragAdjust) tx = 0;
                float deltaAngleLR = tx / wid * sensitivity;

                if (Mathf.Abs(sPos.y - t1.position.y) < dragAdjust) ty = 0;
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
                if (pos.y <= 0.005f)
                {
                    CameraPosAdjust();
                }

                lastPos = t1.position;
            }
        }

        // 拡縮
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            // 範囲外は無効
            if (!stageController.TapOrDragRange(t1.position, dragRangeMin, dragRangeMax)) return;
            if (!stageController.TapOrDragRange(t2.position, dragRangeMin, dragRangeMax)) return;

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
    /// ダブルタップで90度回転 前後左右に移動する
    /// </summary>
    void DoubleTapRotate()
    {
        // 1回目のタップ
        if (Input.GetMouseButtonDown(0) && tapCount == 0)
        {
            // 範囲外は無効
            if (!stageController.TapOrDragRange(Input.mousePosition)) return;
            if (stageController.TapOrDragRange(Input.mousePosition, dragRangeMin, dragRangeMax)) return;

            // 入力方向を保存
            inputDir1 = DoubleTapPosJudgement(Input.mousePosition);

            tapCount++;
            canJudgement = true;
        }

        if (canJudgement) doubleTapTime += Time.deltaTime;

        // 2回目のタップ
        if (doubleTapTime <= 0.2f && doubleTapTime >= 0.05f && tapCount == 1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 範囲外は無効
                if (!stageController.TapOrDragRange(Input.mousePosition)) return;
                if (stageController.TapOrDragRange(Input.mousePosition, dragRangeMin, dragRangeMax)) return;

                // 入力方向を保存
                inputDir2 = DoubleTapPosJudgement(Input.mousePosition);

                tapCount++;
            }
        }
        else if (doubleTapTime > 0.2f)
        {
            canJudgement = false;
            tapCount = 0;
            doubleTapTime = 0f;
            inputDir1 = DoubleTap_TeleportDir.NULL;
            inputDir2 = DoubleTap_TeleportDir.NULL;
        }

        if (tapCount >= 2)
        {
            if(inputDir1 == inputDir2 && inputDir1 != DoubleTap_TeleportDir.NULL && inputDir2 != DoubleTap_TeleportDir.NULL)
                teleportDir = inputDir1;

            canJudgement = false;
            doubleTapTime = 0f;
            is90Rotate = true;
        }

        if(is90Rotate)
        {
            Camera90Rotate();
            is90Rotate = false;
            tapCount = 0;
            inputDir1 = DoubleTap_TeleportDir.NULL;
            inputDir2 = DoubleTap_TeleportDir.NULL;
        }
    }

    void Camera90Rotate()
    {
        Vector3 crossMin_right = new Vector3(0, 0, -100);
        Vector3 crossMin_left = new Vector3(0, 0, 100);
        Vector3 crossMin_up = new Vector3(100, 0, 0);
        Vector3 crossMin_down = new Vector3(-100, 0, 0);

        Vector3 near_right = Vector3.zero;
        Vector3 near_left = Vector3.zero;
        Vector3 near_up = Vector3.zero;
        Vector3 near_down = Vector3.zero;

        for (int i = 0; i < adjustPoint_90.Length; i++)
        {
            var f = transform.forward;
            var v = adjustPoint_90[i] - transform.position;
            var cross = Vector3.Cross(f, v);

            Debug.Log(i + " : " + cross);

            // 左右判定
            if(cross.z < 0 && Mathf.Abs(cross.z) < Mathf.Abs(crossMin_right.z))
            {
                crossMin_right = cross;
                near_right = adjustPoint_90[i];
            }
            else if (cross.z > 0 && Mathf.Abs(cross.z) < Mathf.Abs(crossMin_left.z))
            {
                crossMin_left = cross;
                near_left = adjustPoint_90[i];
            }

            // 上下判定
            if (cross.x > 0 && Mathf.Abs(cross.x) < Mathf.Abs(crossMin_up.x))
            {
                crossMin_up = cross;
                near_up = adjustPoint_90[i];
            }
            else if (cross.x < 0 && Mathf.Abs(cross.x) < Mathf.Abs(crossMin_down.x))
            {
                crossMin_down = cross;
                near_down = adjustPoint_90[i];
            }
        }

        Vector3 cPos = transform.position;
        switch(teleportDir)
        {
            case DoubleTap_TeleportDir.UP:
                cPos = near_up;
                break;
            case DoubleTap_TeleportDir.DOWN:
                cPos = near_down;
                break;
            case DoubleTap_TeleportDir.RIGHT:
                cPos = near_right;
                break;
            case DoubleTap_TeleportDir.LEFT:
                cPos = near_left;
                break;
            default:
                cPos = transform.position;
                break;
        }

        transform.position = cPos;

        if (cPos.y > 0) transform.LookAt(target, Vector3.back);
        else transform.LookAt(target, Vector3.up);
    }

    /// <summary>
    /// タップ位置が上下左右の範囲のどこにあるかを判定し、移動方向を返す
    /// </summary>
    /// <param name="_pos">タップ位置</param>
    /// <returns>方向を返す</returns>
    DoubleTap_TeleportDir DoubleTapPosJudgement(Vector2 _pos)
    {
        // Input.mousepositionは原点が違うので調整
        _pos.y *= -1;
        _pos.y += hei;

        // 上部分
        if (InsideOrOutsideJudgement(dragRangeVertex[2], dragRangeVertex[1], dragRangeVertex[0], _pos))
            return DoubleTap_TeleportDir.UP;

        // 下部分
        if (InsideOrOutsideJudgement(dragRangeVertex[3], dragRangeVertex[4], dragRangeVertex[0], _pos))
            return DoubleTap_TeleportDir.DOWN;

        // 右部分
        if (InsideOrOutsideJudgement(dragRangeVertex[4], dragRangeVertex[2], dragRangeVertex[0], _pos))
            return DoubleTap_TeleportDir.RIGHT;

        // 左部分
        if (InsideOrOutsideJudgement(dragRangeVertex[1], dragRangeVertex[3], dragRangeVertex[0], _pos))
            return DoubleTap_TeleportDir.LEFT;

        return DoubleTap_TeleportDir.NULL;
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
        transform.position = defaultPos;
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
        transform.position = new Vector3(-mapSize.x / 2 + 0.5f, mapSize.x + 10, mapSize.z + 2);
        //_camera.orthographicSize = mapSize.x + 2;
        defaultPos = transform.position;
        defaultRot = transform.rotation;

        // 注視位置設定
        target = mapSize / 2;
        target.x *= -1;
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f);
        
        transform.LookAt(target, transform.up);

        // 補正ポイントの生成
        float r = (target - defaultPos).magnitude;
        Vector3 firstPos = new Vector3(r, 0, 0);
        for (int i = 0; i < point.Length; i++)
        {
            point[i] = Quaternion.Euler(0, i * adjustAngle, 0) * firstPos + target;
        }

        // 90度毎の補正ポイントの作成
        adjustPoint_90[0] = new Vector3(target.x, r, target.z);
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
