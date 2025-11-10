/// <summary>
/// Contract for triggering game SFX/Music without referencing a concrete MonoBehaviour.
/// </summary>
public interface IAudioService
{
    void PlayAttackSound();
    void PlayHitSound();
    void PlayDeathSound();
    void PlayAmbientTrack(string trackId);
    void PlayQuestCompleteSound();
}
