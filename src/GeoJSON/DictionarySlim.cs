// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace GeoJSON;

internal sealed class DictionarySlim : IDictionary<string, object?>
{
    private string[] _keys = [];
    private object?[] _values = [];
    private int _size;

    public int Count => _size;

    public int Capacity
    {
        get => _keys.Length;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, _size);

            if (value != _keys.Length)
            {
                if (value > 0)
                {
                    string[] newKeys = new string[value];
                    object?[] newValues = new object?[value];
                    if (_size > 0)
                    {
                        Array.Copy(_keys, newKeys, _size);
                        Array.Copy(_values, newValues, _size);
                    }
                    _keys = newKeys;
                    _values = newValues;
                }
                else
                {
                    _keys = [];
                    _values = [];
                }
            }
        }
    }

    public object? this[string key]
    {
        get
        {
            int index = _keys.IndexOf(key);
            return index == -1 ? throw new KeyNotFoundException() : _values[index];
        }
        set => GetValueRefOrAddDefault(key) = value;
    }

    public ICollection<string> Keys => _keys[.._size];

    public ICollection<object?> Values => _values[.._size];

    public bool IsReadOnly => false;

    public void Add(string key, object? value) => GetValueRefOrAddDefault(key, throwIfExists: true) = value;

    public void Add(KeyValuePair<string, object?> item) => GetValueRefOrAddDefault(item.Key, throwIfExists: true) = item.Value;

    public void Clear()
    {
        _keys = [];
        _values = [];
        _size = 0;
    }

    public bool Contains(KeyValuePair<string, object?> item)
    {
        int index = _keys.IndexOf(item.Key);
        if (index == -1) return false;
        object? value = _values[index];
        return value is null ? item.Value is null : value.Equals(item.Value);
    }

    public bool ContainsKey(string key) => _keys.Contains(key);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _keys.Zip(_values).Take(_size).Select(item => KeyValuePair.Create(item.First, item.Second)).GetEnumerator();

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
    {
        int index = _keys.IndexOf(key);
        if (index == -1)
        {
            value = default;
            return false;
        }
        value = _values[index];
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private ref object? GetValueRefOrAddDefault(string key, bool throwIfExists = false)
    {
        int index = _keys.IndexOf(key);
        if (index == -1)
        {
            index = _size;
            if ((uint)index >= (uint)_keys.Length)
            {
                Grow(index + 1);
            }
            _size = index + 1;
            _keys[index] = key;
        }
        else if (throwIfExists)
        {
            throw new ArgumentException(nameof(key));
        }

        return ref _values[index];
    }

    private void Grow(int capacity)
    {
        int newCapacity = _keys.Length == 0 ? 4 : 2 * _keys.Length;

        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

        if (newCapacity < capacity) newCapacity = capacity;

        Capacity = newCapacity;
    }

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(string key) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<string, object?> item) => throw new NotImplementedException();
}
