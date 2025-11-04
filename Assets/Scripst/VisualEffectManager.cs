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

    [Header("Pooling")]
    [Range(0, 50)]
    public int poolPrewarm = 5;
    private System.Collections.Generic.Dictionary<ParticleSystem, System.Collections.Generic.Stack<ParticleSystem>> _pools = new System.Collections.Generic.Dictionary<ParticleSystem, System.Collections.Generic.Stack<ParticleSystem>>();

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

        PrewarmPool(fireSpellEffect);
        PrewarmPool(iceSpellEffect);
        PrewarmPool(magicSpellEffect);
        PrewarmPool(hitEffect);
        PrewarmPool(bloodEffect);
        PrewarmPool(explosionEffect);
    }

    // Spell Effects
    public void PlayFireSpellEffect(Vector3 position)
    {
        SpawnAndPlay(fireSpellEffect, position);
    }

    public void PlayIceSpellEffect(Vector3 position)
    {
        SpawnAndPlay(iceSpellEffect, position);
    }

    public void PlayMagicSpellEffect(Vector3 position)
    {
        SpawnAndPlay(magicSpellEffect, position);
    }

    // Combat Effects
    public void PlayHitEffect(Vector3 position)
    {
        SpawnAndPlay(hitEffect, position);
    }

    public void PlayBloodEffect(Vector3 position)
    {
        SpawnAndPlay(bloodEffect, position);
    }

    public void PlayExplosionEffect(Vector3 position)
    {
        SpawnAndPlay(explosionEffect, position);
    }

    private void PrewarmPool(ParticleSystem prefab)
    {
        if (prefab == null || poolPrewarm <= 0) return;
        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new System.Collections.Generic.Stack<ParticleSystem>(poolPrewarm);
        for (int i = 0; i < poolPrewarm; i++)
        {
            var ps = Instantiate(prefab, transform);
            ps.gameObject.SetActive(false);
            _pools[prefab].Push(ps);
        }
    }

    private void SpawnAndPlay(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return;
        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new System.Collections.Generic.Stack<ParticleSystem>();
        ParticleSystem ps = _pools[prefab].Count > 0 ? _pools[prefab].Pop() : Instantiate(prefab);
        var go = ps.gameObject;
        go.transform.position = position;
        go.transform.rotation = Quaternion.identity;
        go.SetActive(true);
        ps.Play();
        StartCoroutine(ReleaseWhenDone(prefab, ps));
    }

    private System.Collections.IEnumerator ReleaseWhenDone(ParticleSystem prefab, ParticleSystem instance)
    {
        var main = instance.main;
        float duration = main.duration + main.startLifetime.constantMax;
        yield return new WaitForSeconds(duration);
        instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        instance.gameObject.SetActive(false);
        _pools[prefab].Push(instance);
    }
}
