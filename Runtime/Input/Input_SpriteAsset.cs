using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "InputSpriteAsset", menuName = "Koala/SO/Input Sprite Asset")]
public class InputSpriteAsset : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private List<SpriteEntry> _entries = new();
    private Dictionary<string, Sprite> _lookup;

    public Sprite this[string key] => _lookup != null && _lookup.TryGetValue(key, out var sprite) ? sprite : null;

    public bool Contains(string key) => _lookup != null && _lookup.ContainsKey(key);

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        BuildLookup();
    }

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<string, Sprite>(_entries.Count);
        foreach (var entry in _entries)
        {
            if (!string.IsNullOrEmpty(entry.Key) && !_lookup.ContainsKey(entry.Key))
                _lookup.Add(entry.Key, entry.Value);
        }
    }

    public bool TryGetSprite(string key, out Sprite sprite)
    {
        return _lookup.TryGetValue(key, out sprite);
    }
}

[Serializable]
public struct SpriteEntry
{
    public string Key;
    public Sprite Value;
}