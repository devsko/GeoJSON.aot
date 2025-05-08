// Copyright © devsko 2025. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace GeoJson.Test;

public static class DictionarySlimTests
{
    [Fact]
    public static void Add()
    {
        DictionarySlim dict = new();
        dict.Add("a", 1);

        Assert.Single(dict);
        Assert.Equal("a", dict.Keys.First());
        Assert.Equal(1, dict.Values.First());
    }

    [Fact]
    public static void AddExistingKeyThrows()
    {
        DictionarySlim dict = new();
        dict.Add("a", 1);
        Assert.Throws<ArgumentException>(() => dict.Add("a", 2));
    }

    [Fact]
    public static void IndexGets()
    {
        DictionarySlim dict = new();
        dict.Add("a", 1);
        Assert.Equal(1, dict["a"]);
    }

    [Fact]
    public static void IndexAdds()
    {
        DictionarySlim dict = new();
        dict["a"] = 2;
        Assert.Single(dict);
        Assert.Equal(2, dict["a"]);
    }

    [Fact]
    public static void IndexSets()
    {
        DictionarySlim dict = new();
        dict.Add("a", 1);
        dict["a"] = 2;
        Assert.Single(dict);
        Assert.Equal(2, dict["a"]);
    }

    [Fact]
    public static void IndexThrows()
    {
        DictionarySlim dict = new();
        Assert.Throws<KeyNotFoundException>(() => dict["a"]);
    }

    [Fact]
    public static void Enumerate()
    {
        DictionarySlim dict = new();
        dict.Add("a", 1);
        dict.Add("b", 2);
        dict.Add("c", 3);
        dict.Add("d", 4);
        dict.Add("e", 5);
        var enumerator = dict.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Equal(KeyValuePair.Create("a", (object?)1), enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(KeyValuePair.Create("b", (object?)2), enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(KeyValuePair.Create("c", (object?)3), enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(KeyValuePair.Create("d", (object?)4), enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(KeyValuePair.Create("e", (object?)5), enumerator.Current);
        Assert.False(enumerator.MoveNext());
    }
}
