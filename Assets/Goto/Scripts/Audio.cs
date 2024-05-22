using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

public class Audio : MonoBehaviour
{
    //Audio�~�L�T�[������Ƃ��ł�
    [SerializeField] AudioMixer audioMixer;

    //���ꂼ��̃X���C�_�[������Ƃ��ł��B�B
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SESlider;
    [SerializeField]
    Slider MasterSlider;

    private void Start()
    {
        //�~�L�T�[��volume�ɃX���C�_�[��volume�����Ă܂��B

        //BGM
        audioMixer.GetFloat("BGM", out float bgmVolume);
        BGMSlider.value = bgmVolume;
        //SE
        audioMixer.GetFloat("SE", out float seVolume);
        SESlider.value = seVolume;

        audioMixer.GetFloat("Master", out float masterVolume);
        MasterSlider.value = masterVolume;
    }
    public void SetBGM(float volume)
    {
        audioMixer.SetFloat("BGM", volume);
    }

    public void SetSE(float volume)
    {
        audioMixer.SetFloat("SE", volume);
    }
    public void SetMaster(float volume)
    {
        audioMixer.SetFloat("Master", volume);
    }
}