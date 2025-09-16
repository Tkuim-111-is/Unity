using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class ItemStateManager : MonoBehaviour
{
    public static ItemStateManager Instance { get; private set; }
    private const string Key = "CollectedItems_v1"; // PlayerPrefs Key
    private HashSet<string> collected = new HashSet<string>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �������J��A���y�Ҧ� Collectible�A�̾ڪ��A�M��
        ApplySceneState();
    }

    public void MarkCollected(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        if (collected.Add(itemId)) Save();
    }

    public bool IsCollected(string itemId) => !string.IsNullOrEmpty(itemId) && collected.Contains(itemId);

    public void ApplySceneState()
    {
        foreach (var c in FindObjectsOfType<Collectible>(true))
        {
            ApplyToCollectible(c);
        }
    }

    public void ApplyToCollectible(Collectible c)
    {
        if (!c) return;
        bool done = IsCollected(c.itemId);
        if (done)
        {
            if (c.setUntaggedWhenCollected) c.gameObject.tag = "Untagged";
            if (c.hideWhenCollected) c.gameObject.SetActive(false);
        }
        else
        {
            // �|�������G�T�O���Ҧ^��w�]�]�קK�W������ɴݯd�^
            if (c.resetTagIfNotCollected && !string.IsNullOrEmpty(c.defaultTag))
                c.gameObject.tag = c.defaultTag;
        }
    }

    private void Save()
    {
        var s = string.Join(",", collected);
        PlayerPrefs.SetString(Key, s);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        collected.Clear();
        var s = PlayerPrefs.GetString(Key, "");
        if (!string.IsNullOrEmpty(s))
        {
            foreach (var id in s.Split(','))
            {
                var trimmed = id.Trim();
                if (!string.IsNullOrEmpty(trimmed)) collected.Add(trimmed);
            }
        }
    }
}
