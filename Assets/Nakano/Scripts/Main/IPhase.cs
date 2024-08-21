using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �e�t�F�[�Y�̐���N���X�p�C���^�[�t�F�[�X
/// </summary>
public interface IPhase
{
    /// <summary>
    /// ������
    /// </summary>
    void Initialize();

    /// <summary>
    /// �t�F�[�Y�J�n���̏���
    /// </summary>
    void PhaseStart();

    /// <summary>
    /// �t�F�[�Y���̏���
    /// </summary>
    void PhaseUpdate();

    /// <summary>
    /// �t�F�[�Y�I�����̏���
    /// </summary>
    void PhaseEnd();
}
