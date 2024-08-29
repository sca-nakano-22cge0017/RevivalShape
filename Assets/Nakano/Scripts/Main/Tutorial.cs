using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Extensions;

[System.Serializable]
public class Windows
{
    public GameObject order;
    public GameObject[] objects;
}

/// <summary>
/// �`���[�g���A������
/// </summary>
public class Tutorial : MonoBehaviour
{
    [SerializeField] private StageController stageController;
    [SerializeField] private TimeManager timeManager;

    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Windows[] checkPhase;
    [SerializeField] private Windows[] selectPhase;
    [SerializeField] private Windows[] playPhase;

    [SerializeField, Header("UI����s�ɂ���pImage")] GameObject obstruct;
    [SerializeField, Header("�^�b�v���Ă��玟�Ƀ^�b�v�ł���悤�ɂȂ�܂ł̎���")] float tapCoolTime = 0.5f;

    private int tapCount = 0;
    private bool tutorialCompleteByPhase = false;
    /// <summary>
    /// �e�t�F�[�Y�̃`���[�g���A�����I�����Ă��邩
    /// </summary>
    public bool TutorialCompleteByPhase { get { return tutorialCompleteByPhase; } private set { tutorialCompleteByPhase = value; } }

    delegate void PlayFunc();
    PlayFunc playFunc;

    private bool isTutorialComplete = false;
    /// <summary>
    ///  �`���[�g���A�����S�ďI�����Ă��邩
    /// </summary>
    public bool IsTutorialComplete { get { return isTutorialComplete; } private set { } }

    private string methodName = "";
    public string MethodName { get { return methodName; } private set { methodName = value; } }

    // SE
    private SoundManager soundManager;

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
    }

    public void TutorialStart()
    {
        tutorialCanvas.SetActive(true);
        obstruct.SetActive(true);
        playFunc = CheckA;

        timeManager.OnStop();

        SEPlay();
    }

    public void TutorialUpdate()
    {
        if (playFunc != null && !isTutorialComplete)
        {
            playFunc();
            methodName = playFunc.Method.Name;
        }

        if(isTutorialComplete)
        {
            obstruct.SetActive(false);
        }
    }

    // �m�F�t�F�[�Y�̐���
    void CheckA()
    {
        NextWindowDisplayByTap(checkPhase[0], CheckB, () => 
        {
            StartCoroutine(DelayCoroutine(0.1f, () => 
            {
                // �E�ړ��w��
                if (!checkPhase[1].order.activeSelf)
                {
                    checkPhase[1].order.SetActive(true);
                    checkPhase[1].objects[0].SetActive(true);

                    SEPlay();
                }
            }));
        });
    }

    // �E�����փX���C�v������
    private bool toCheckB_2 = false;
    public bool ToCheckB_2 { 
        get { return toCheckB_2;} 
        set {
            if(toCheckB_2) return;
            toCheckB_2 = value;

            // ���X���C�v�w��
            if (!checkPhase[1].objects[1].activeSelf)
            {
                checkPhase[1].objects[0].SetActive(false);
                checkPhase[1].objects[1].SetActive(true);
            }
        } 
    }
    // �h���b�O����̐���
    void CheckB()
    {
    }

    // �������փX���C�v������
    private bool toCheckC = false;
    public bool ToCheckC
    {
        get { return toCheckC; }
        set
        {
            if (toCheckC) return;
            toCheckC = true;

            // �X���C�v�w����\��
            checkPhase[1].order.SetActive(false);
            checkPhase[1].objects[1].SetActive(false);

            playFunc = CheckC;

            if (!checkPhase[2].order.activeSelf)
            {
                checkPhase[2].order.SetActive(true);
                SEPlay();
            }

            obstruct.SetActive(false);
        }
    }

    // ���Z�b�g�{�^������������
    private bool isCheckC = false;
    public bool IsCheckC
    {
        get { return isCheckC; }
        set
        {
            if (isCheckC) return;
            isCheckC = value;

            // ���Z�b�g�{�^���������ꂽ��E�B���h�E�̕\��������
            checkPhase[2].order.SetActive(false);

            obstruct.SetActive(true);
        }
    }

    // ���Z�b�g�{�^���̐���
    void CheckC()
    {
    }

    // ���Z�b�g�{�^��������
    public void GoToCheckD()
    {
        IsCheckC = true;
    }

    // �J�����������ʒu�ɖ߂����o���I�������
    private bool toCheckD = false;
    public bool ToCheckD
    {
        get { return toCheckD; }
        set
        {
            if(toCheckD) return;
            toCheckD = value;

            // ��]���o���I������玟��
            playFunc = CheckD;

            if (!checkPhase[3].order.activeSelf)
            {
                checkPhase[3].order.SetActive(true);
                SEPlay();
            }
        }
    }

    // �_�u���^�b�v������
    private bool isCheckD = false;
    public bool IsCheckD { 
        get { return isCheckD; } 
        set {
            if (isCheckD) return;
            isCheckD = value;

            // �_�u���^�b�v������E�B���h�E���\���ɂ���
            checkPhase[3].order.SetActive(false);
        } 
    }

    // �_�u���^�b�v�̐���
    void CheckD()
    {
    }

    // �_�u���^�b�v�̉�]���o���I�������
    private bool toCheckE = false;
    public bool ToCheckE
    {
        get { return toCheckE; }
        set
        {
            if(toCheckE) return;
            toCheckE = value;

            // ��]���o���I������玟��
            playFunc = CheckE;

            if (!checkPhase[4].order.activeSelf)
            {
                checkPhase[4].order.SetActive(true);
                SEPlay();
            }
        }
    }

    // ���̃t�F�[�Y�ւ̈ڍs�{�^���̐���
    void CheckE()
    {
        NextWindowDisplayByTap(checkPhase[4], () => { }, () => 
        {
            ExplainDisplaing(false);
            timeManager.OnStart();
        });
    }

    private bool toSelectA = false;
    public bool ToSelectA
    {
        get
        {
            return toSelectA;
        }
        set
        {
            if(toSelectA) return;
            toSelectA = value;

            playFunc = SelectA;
            ExplainDisplaing(true);

            obstruct.SetActive(false);
            timeManager.OnStop();

            SEPlay();
        }
    }

    // �I���t�F�[�Y�̐���
    void SelectA()
    {
        NextWindowDisplayByTap(selectPhase[0], SelectB, () => 
        {
            if (!selectPhase[1].order.activeSelf)
            {
                selectPhase[1].order.SetActive(true);
                SEPlay();
            }
        });
    }

    // �V�[�g�̐���
    void SelectB()
    {
    }

    // �V�[�g�ɐ�������͂�����
    private bool toSelectC = false;
    public bool ToSelectC
    {
        get
        {
            return toSelectC;
        }
        set
        {
            if (toSelectC) return;
            toSelectC = value;

            // ���͂��ꂽ��
            StartCoroutine(DelayCoroutine(tapCoolTime, () =>
            {
                selectPhase[1].order.SetActive(false);

                playFunc = SelectC;

                if (!selectPhase[2].order.activeSelf)
                {
                    selectPhase[2].order.SetActive(true);
                    obstruct.SetActive(true);

                    SEPlay();

                    StartCoroutine(DelayCoroutine(tapCoolTime, () =>
                    {
                        toSelectD = true;
                    }));
                }
            }));
        }
    }
    // �����S���̐���
    void SelectC()
    {
        if(toSelectD) NextFunctionByTap(selectPhase[2], SelectD, () => 
        {
            if (!selectPhase[3].order.activeSelf)
            {
                selectPhase[3].order.SetActive(true);
                SEPlay();

                StartCoroutine(DelayCoroutine(tapCoolTime, () =>
                {
                    toSelectE = true;
                }));
            }
        });
    }

    private bool toSelectD = false;
    // ���ዾ�̐���
    void SelectD()
    {
        if (toSelectE) NextFunctionByTap(selectPhase[3], SelectE, () => 
        {
            if (!selectPhase[4].order.activeSelf)
            {
                selectPhase[4].order.SetActive(true);
                SEPlay();

                StartCoroutine(DelayCoroutine(tapCoolTime, () =>
                {
                    toSelectF = true;
                }));
            }
        });
    }

    private bool toSelectE = false;
    // �^�u�̐���
    void SelectE()
    {
        if (toSelectF) NextFunctionByTap(selectPhase[4], SelectF, () => 
        {
            if (!selectPhase[5].order.activeSelf) selectPhase[5].order.SetActive(true);
            SEPlay();
        });
    }

    private bool toSelectF = false;
    // ���̃t�F�[�Y�ւ̈ڍs�{�^���̐���
    void SelectF()
    {
        NextWindowDisplayByTap(selectPhase[5], () => { }, () => 
        {
            ExplainDisplaing(false);
            timeManager.OnStart();
        });
    }

    /// <summary>
    /// ���s�t�F�[�Y�̐������n�߂�
    /// </summary>
    public void ToPlayA()
    {
        playFunc = PlayA;
        ExplainDisplaing(true);
        timeManager.OnStop();

        SEPlay();
    }

    // ���s�t�F�[�Y�̐���
    void PlayA()
    {
        NextWindowDisplayByTap(playPhase[0], () => { }, () =>
        {
            endPlayA = true;
        });
    }

    // ���s�t�F�[�Y�̐������I�������
    private bool endPlayA = false;
    public bool EndPlayA { get { return endPlayA; } private set { } }

    // �������o���I�������
    private bool toPlayB = false;
    public bool ToPlayB
    {
        get
        {
            return toPlayB;
        }
        set
        {
            if (toPlayB) return;
            toPlayB = value;

            playFunc = PlayB;

            SEPlay();
        }
    }

    // �N���A�����̐���
    void PlayB()
    {
        NextWindowDisplayByTap(playPhase[1], () => { }, () => 
        {
            endPlayB = true;
        });
    }

    // �N���A�����̐������I�������
    private bool endPlayB = false;
    public bool EndPlayB { get { return endPlayB; } private set { } }

    // ���U���g�\�����Ă��邩
    private bool toPlayC = false;
    public bool ToPlayC
    {
        get
        {
            return toPlayC;
        }
        set
        {
            if (toPlayC) return;
            toPlayC = value;

            playFunc = PlayC;
            SEPlay();
        }
    }

    private bool endPlayC = false;
    public bool EndPlayC { get { return endPlayC;} }

    // �~�b�V�����̐����A�`���[�g���A���̏I��
    void PlayC()
    {
        NextWindowDisplayByTap(playPhase[2], () => { }, () => 
        {
            ExplainDisplaing(false);
            endPlayC = true;
            isTutorialComplete = true;
            timeManager.OnStart();
        });
    }

    /// <summary>
    /// �^�b�v�Ŏ��̊֐��ֈڍs����
    /// </summary>
    /// <param name="_unDispWindow">��\���ɂ���E�B���h�E</param>
    /// <param name="_nextFunc">���̊֐�</param>
    void NextFunctionByTap(Windows _unDispWindow, PlayFunc _nextFunc, Action _lastFunc)
    {
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (_unDispWindow.order.activeSelf)
            {
                _unDispWindow.order.SetActive(false);
            }

            playFunc = _nextFunc;
            _lastFunc?.Invoke();
        }
    }

    /// <summary>
    /// �^�b�v�ŕ\���E�B���h�E�����ԂɕύX����
    /// </summary>
    /// <param name="_windows">�ΏۃE�B���h�E</param>
    /// <param name="_nextFunc">���ɌĂԊ֐�</param>
    /// <param name="_lastFunc">�E�B���h�E�����Ƃ��ɍs������</param>
    void NextWindowDisplayByTap(Windows _windows, PlayFunc _nextFunc, Action _lastFunc)
    {
        // �E�B���h�E�̐e��\��
        if (!_windows.order.activeSelf) _windows.order.SetActive(true);

        // �^�b�v�Ŏ��̐�����
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            tapCount++;

            for (int i = 0; i < _windows.objects.Length; i++)
            {
                if (i == tapCount)
                {
                    SEPlay();
                    _windows.objects[i].SetActive(true);
                }
                else _windows.objects[i].SetActive(false);
            }

            if (tapCount >= _windows.objects.Length)
            {
                _windows.order.SetActive(false);
                tapCount = 0;

                playFunc = _nextFunc;

                _lastFunc?.Invoke();
            }
        }
    }

    /// <summary>
    /// �����E�B���h�E�\�������ǂ�����ݒ�
    /// </summary>
    /// <param name="_isDisplaing">true�̂Ƃ��A�������@����ɐ������|���� + �ݒ�{�^���g�p�s��</param>
    void ExplainDisplaing(bool _isDisplaing)
    {
        // ���R�ɑ���ł��邩��ݒ�
        tutorialCompleteByPhase = !_isDisplaing;

        // �ݒ�{�^���������邩��ݒ�
        obstruct.SetActive(_isDisplaing);
    }

    void SEPlay()
    {
        if (soundManager != null)
        {
            soundManager.SEPlay7();
        }
    }
}