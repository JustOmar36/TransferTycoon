using UnityEngine;

public class UIEventSfx : MonoBehaviour
{
    public enum SfxType
    {
        CommonClick,
        MenuClick,
        TextPopup,
        DialogueReminder
    }

    [Header("SFX Settings")]
    public SfxType sfxType = SfxType.CommonClick;
    [Range(0f, 10f)]
    public float volume = 1f;
    public float delay = 0f;

    // Call this from any UI event (OnClick, OnValueChanged, etc.)
    public void PlaySfx()
    {
        if (AudioUIManager.Instance == null) return;

        switch (sfxType)
        {
            case SfxType.CommonClick:
                AudioUIManager.Instance.PlayCommonButtonClick(volume, delay);
                break;
            case SfxType.MenuClick:
                AudioUIManager.Instance.PlayMenuButtonClick(volume, delay);
                break;
            case SfxType.TextPopup:
                AudioUIManager.Instance.PlayTextPopup(volume, delay);
                break;
            case SfxType.DialogueReminder:
                AudioUIManager.Instance.PlayDialogueReminder(volume, delay);
                break;
        }
    }
}
