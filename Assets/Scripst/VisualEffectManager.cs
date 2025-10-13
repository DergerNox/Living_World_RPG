using UnityEngine;

public class VisualEffectManager : MonoBehaviour
{
    public static VisualEffectManager Instance;

    [Header("Spell Effects")]
    public ParticleSystem fireSpellEffect;
    public ParticleSystem iceSpellEffect;
    public ParticleSystem magicSpellEffect;

    [Header("Combat Effects")]
    public ParticleSystem hitEffect;
    public ParticleSystem bloodEffect;
    public ParticleSystem explosionEffect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Spell Effects
    public void PlayFireSpellEffect(Vector3 position)
    {
        if (fireSpellEffect != null)
        {
            Instantiate(fireSpellEffect, position, Quaternion.identity).Play();
        }
    }

    public void PlayIceSpellEffect(Vector3 position)
    {
        if (iceSpellEffect != null)
        {
            Instantiate(iceSpellEffect, position, Quaternion.identity).Play();
        }
    }

    public void PlayMagicSpellEffect(Vector3 position)
    {
        if (magicSpellEffect != null)
        {
            Instantiate(magicSpellEffect, position, Quaternion.identity).Play();
        }
    }

    // Combat Effects
    public void PlayHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, position, Quaternion.identity).Play();
        }
    }

    public void PlayBloodEffect(Vector3 position)
    {
        if (bloodEffect != null)
        {
            Instantiate(bloodEffect, position, Quaternion.identity).Play();
        }
    }

    public void PlayExplosionEffect(Vector3 position)
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, position, Quaternion.identity).Play();
        }
    }
}
