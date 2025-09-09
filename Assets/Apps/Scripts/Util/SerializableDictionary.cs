using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

    public SerializableDictionary() { }

    public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary ?? new Dictionary<TKey, TValue>();
    }

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var pair in _dictionary)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        _dictionary = new Dictionary<TKey, TValue>();
        for (int i = 0; i < keys.Count; i++)
        {
            if (i < values.Count)
            {
                if (!_dictionary.ContainsKey(keys[i]))
                {
                    _dictionary.Add(keys[i], values[i]);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key found during deserialization: {keys[i]}. Skipping this entry.");
                }
            }
        }
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        return _dictionary;
    }
}

[System.Serializable]
public class SerializableDictionaryIntFloatList : SerializableDictionary<int, FloatListWrapper>
{
    public SerializableDictionaryIntFloatList() : base() { }
    public SerializableDictionaryIntFloatList(Dictionary<int, FloatListWrapper> dictionary) : base(dictionary) { }
}


[System.Serializable]
public class SerializableDictionaryIntLong : SerializableDictionary<int, long>
{
    public SerializableDictionaryIntLong() : base() { }
    public SerializableDictionaryIntLong(Dictionary<int, long> dictionary) : base(dictionary) { }
}

[System.Serializable]
public class FloatListWrapper
{
    public List<float> floats;

    public FloatListWrapper()
    {
        floats = new List<float>();
    }

    public FloatListWrapper(List<float> initialList)
    {
        floats = initialList ?? new List<float>();
    }
}