using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : Singleton<VFXManager>
{
    public List<VFXData> vfxList = new();
    private Dictionary<string, GameObject> vfxDictionary = new();

    protected override void Awake()
    {
        base.Awake();
        foreach (var vfx in vfxList)
        {
            vfxDictionary[vfx.key] = vfx.effectPrefab;
        }

    }


    public void PlayVFX(string key, Vector3 position, Quaternion rotation, float destroyTime = 2f)
    {
        if (vfxDictionary.TryGetValue(key, out GameObject prefab))
        {
            var vfxInstance = ObjectPool.Instance.GetObject(prefab);
            vfxInstance.transform.SetPositionAndRotation(position, rotation);
            StartCoroutine(DeactiveAfterTime(vfxInstance, destroyTime));
            vfxInstance.SetActive(true);
        }
        else
        {
            Debug.LogWarning("VFX is not founded: " + key);
        }
    }


    private IEnumerator DeactiveAfterTime(GameObject prefab, float time)
    {
        yield return new WaitForSeconds(time);

        if(prefab != null)
            prefab.SetActive(false);
    }
}

[System.Serializable]
public class VFXData
{
    public string key;
    public GameObject effectPrefab;
}
