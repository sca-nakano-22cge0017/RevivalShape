using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Windows[] checkPhase;
    [SerializeField] private Windows[] selectPhase;
    [SerializeField] private Windows[] playPhase;

    [SerializeField, Header("UI����s�ɂ���pImage")] GameObject obstruct;

    private int tapCount = 0;
    private bool tutorialCompleteByPhase = false;
    /// <summary>
    /// �e�t�F�[�Y�̃`���[�g���A�����I�����Ă��邩
    /// </summary>
    public bool TutorialCompleteByPhase { get { return tutorialCompleteByPhase; } private set { tutorialCompleteByPhase = value; } }

    delegate void PlayFunc();
    PlayFunc playFunc;
    private bool isFirstSelectPhase = true;
    private bool isFirstPlayPhase = true;

    private bool isTutorialComplete = false;
    public bool IsTutorialComplete { get { return isTutorialComplete; } private set { } }

    private string methodName = "";
    public string MethodName { get { return methodName; } private set { methodName = value; } }

    [SerializeField, Header("�^�b�v���Ă��玟�Ƀ^�b�v�ł���悤�ɂȂ�܂ł̎���")] float tapCoolTime = 0.5f;

    public void TutorialStart()
    {
        tutorialCanvas.SetActive(true);
        obstruct.SetActive(true);
        playFunc = CheckA;
    }

    public void TutorialUpdate()
    {
        if (playFunc != null && !isTutorialComplete)
        {
            playFunc();
            methodName = playFunc.Method.Name;
        }
    }

    // �m�F�t�F�[�Y�̐���
    void CheckA()
    {
        NextWindowDisplayByTap(checkPhase[0], CheckB, () =>{ });
    }

    // �E�����փX���C�v������
    private bool toCheckB_2 = false;
    public bool ToCheckB_2 { 
        get { return toCheckB_2;} 
        set { 
            toCheckB_2 = true;

            // ���X���C�v�w��
            if (!checkPhase[1].objects[1].activeSelf)
            {
                checkPhase[1].objects[0].SetActive(false);
                checkPhase[1].objects[1].SetActive(true);
            }
        } 
    }

    // �������փX���C�v������
    private bool toCheckC = false;
    public bool ToCheckC { 
        get { return toCheckC; } 
        set { 
            toCheckC = true;

            // ���̏�����
            checkPhase[1].order.SetActive(false);
            checkPhase[1].objects[1].SetActive(false);
            playFunc = CheckC;
        }
    }
    // �h���b�O����̐���
    void CheckB()
    {
        // �E�ړ��w��
        if (!checkPhase[1].order.activeSelf)
        {
            checkPhase[1].order.SetActive(true);
            checkPhase[1].objects[0].SetActive(true);
        }
    }

    // ���Z�b�g�{�^������������
    private bool isCheckC = false;
    public bool IsCheckC { 
        get { return isCheckC; } 
        set { 
            isCheckC = value;

            // ���Z�b�g�{�^���������ꂽ��E�B���h�E�̕\��������
            checkPhase[2].order.SetActive(false);
        }
    }

    // �J�����������ʒu�ɖ߂����o���I�������
    private bool toCheckD = false;
    public bool ToCheckD { 
        get { return toCheckD; } 
        set { 
            toCheckD = value;

            // ��]���o���I������玟��
            playFunc = CheckD;
        } 
    }

    // ���Z�b�g�{�^��������
    public void GoToCheckD()
    {
        IsCheckC = true;
    }

    // ���Z�b�g�{�^���̐���
    void CheckC()
    {
        if (!checkPhase[2].order.activeSelf && !isCheckC)
        {
            checkPhase[2].order.SetActive(true);
        }
    }

    // �_�u���^�b�v������
    private bool isCheckD = false;
    public bool IsCheckD { 
        get { return isCheckD; } 
        set { 
            isCheckD = value;

            // �_�u���^�b�v������E�B���h�E���\���ɂ���
            checkPhase[3].order.SetActive(false);
        } 
    }

    // �_�u���^�b�v�̉�]���o���I�������
    private bool toCheckE = false;
    public bool ToCheckE { 
        get { return toCheckE; } 
        set { 
            toCheckE = value;

            // ��]���o���I������玟��
            playFunc = CheckE;
        }
    }

    // �_�u���^�b�v�̐���
    void CheckD()
    {
        if (!checkPhase[3].order.activeSelf && !isCheckD)
        {
            checkPhase[3].order.SetActive(true);
        }
    }

    // ���̃t�F�[�Y�ւ̈ڍs�{�^���̐���
    void CheckE()
    {
        NextWindowDisplayByTap(checkPhase[4], () => { }, () => 
        {
            ExplainDisplaing(false);
        });
    }

    /// <summary>
    /// �I���t�F�[�Y�̐������n�߂�
    /// </summary>
    public void ToSelectA()
    {
        playFunc = SelectA;
        ExplainDisplaing(true);
    }

    // �I���t�F�[�Y�̐���
    void SelectA()
    {
        if(!isFirstSelectPhase) return;

        NextWindowDisplayByTap(selectPhase[0], SelectB, () => { });
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
            if(toSelectC) return;

            toSelectC = value;

            // ���͂��ꂽ��
            StartCoroutine(stageController.DelayCoroutine(tapCoolTime, () => 
            {
                playFunc = SelectC;
                selectPhase[1].order.SetActive(false);
            }));
        }
    }

    // �V�[�g�̐���
    void SelectB()
    {
        if (!selectPhase[1].order.activeSelf)
        {
            selectPhase[1].order.SetActive(true);
        }
    }

    private bool toSelectD = false;

    // �����S���̐���
    void SelectC()
    {
        if (!selectPhase[2].order.activeSelf)
        {
            selectPhase[2].order.SetActive(true);

            StartCoroutine(stageController.DelayCoroutine(tapCoolTime, () =>
            {
                toSelectD = true;
            }));
        }

        if(toSelectD) NextFunctionByTap(selectPhase[2], SelectD);
    }

    private bool toSelectE = false;

    // ���ዾ�̐���
    void SelectD()
    {
        if (!selectPhase[3].order.activeSelf)
        {
            selectPhase[3].order.SetActive(true);

            StartCoroutine(stageController.DelayCoroutine(tapCoolTime, () =>
            {
                toSelectE = true;
            }));
        }

        if (toSelectE) NextFunctionByTap(selectPhase[3], SelectE);
    }

    private bool toSelectF = false;

    // �^�u�̐���
    void SelectE()
    {
        if (!selectPhase[4].order.activeSelf)
        {
            selectPhase[4].order.SetActive(true);

            StartCoroutine(stageController.DelayCoroutine(tapCoolTime, () =>
            {
                toSelectF = true;
            }));
        }

        if (toSelectF) NextFunctionByTap(selectPhase[4], SelectF);
    }

    // ���̃t�F�[�Y�ւ̈ڍs�{�^���̐���
    void SelectF()
    {
        NextWindowDisplayByTap(selectPhase[5], () => { }, () => 
        {
            ExplainDisplaing(false);
            isFirstSelectPhase = false;
        });
    }

    /// <summary>
    /// ���s�t�F�[�Y�̐������n�߂�
    /// </summary>
    public void ToPlayA()
    {
        playFunc = PlayA;
        ExplainDisplaing(true);
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
            if(toPlayB) return;
            toPlayB = value;

            playFunc = PlayB;
        }
    }

    // ���s�t�F�[�Y�̐���
    void PlayA()
    {
        if (!isFirstPlayPhase) return;

        NextWindowDisplayByTap(playPhase[0], () => { }, () =>
        {
            endPlayA = true;
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

    // �~�b�V�����̐����A�`���[�g���A���̏I��
    void PlayC()
    {
        NextWindowDisplayByTap(playPhase[2], () => { }, () => 
        {
            ExplainDisplaing(false);
            isFirstPlayPhase = false;
            isTutorialComplete = true;
        });
    }

    /// <summary>
    /// �^�b�v�Ŏ��̊֐��ֈڍs����
    /// </summary>
    /// <param name="_unDispWindow">��\���ɂ���E�B���h�E</param>
    /// <param name="_nextFunc">���̊֐�</param>
    void NextFunctionByTap(Windows _unDispWindow, PlayFunc _nextFunc)
    {
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            playFunc = _nextFunc;

            if (_unDispWindow.order.activeSelf)
            {
                _unDispWindow.order.SetActive(false);
            }
        }
    }

    /// <summary>
    /// �^�b�v�ŕ\���E�B���h�E�����ԂɕύX����
    /// </summary>
    /// <param name="_windows">�ΏۃE�B���h�E</param>
    /// <param name="_nextFunc">���ɌĂԊ֐�</param>
    /// <param name="_lastFunc">�����I�����ɍs�����ꏈ��</param>
    void NextWindowDisplayByTap(Windows _windows, PlayFunc _nextFunc, Action _lastFunc)
    {
        if(!_windows.order.activeSelf) _windows.order.SetActive(true);

        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            tapCount++;

            for (int i = 0; i < _windows.objects.Length; i++)
            {
                if (i == tapCount)
                {
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
        tutorialCompleteByPhase = !_isDisplaing;
        obstruct.SetActive(_isDisplaing);
    }
}