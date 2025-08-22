using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 配列に対する拡張機能を提供します。
/// </summary>
public static class ArrayExtension
{
    //
    // (1) 配列の要素に対する基本操作
    // - - - - - - - - - - - - - - - - - - - -

    /// <summary>
    /// 配列に対する操作 <see cref="Array.Exists{T}(T[], Predicate{T})"/> を標準の検索方法で拡張メソッド化します。
    /// </summary>
    public static bool Exists<T>(this T[] array, T item) => Array.Exists(array, p => p.Equals(item));

    /// <summary>
    /// 配列に対する操作 <see cref="Array.Exists{T}(T[], Predicate{T})"/> を拡張メソッド化します。
    /// </summary>
    public static bool Exists<T>(this T[] array, Predicate<T> match) => Array.Exists(array, match);

    /// <summary>
    /// 配列に対する操作 <see cref="Array.Find{T}(T[], Predicate{T})"/> を標準の検索方法で拡張メソッド化します。
    /// </summary>
    public static T Find<T>(this T[] array, T item) => Array.Find(array, p => p.Equals(item));

    /// <summary>
    /// 配列に対する操作 <see cref="Array.Find{T}(T[], Predicate{T})"/> を拡張メソッド化します。
    /// </summary>
    public static T Find<T>(this T[] array, Predicate<T> match) => Array.Find(array, match);

    /// <summary>
    /// 配列に対する操作 <see cref="Array.FindIndex{T}(T[], Predicate{T})"/> を拡張メソッド化します。
    /// （見つからない場合-1）
    /// </summary>
    public static int FindIndex<T>(this T[] array, T item) => Array.FindIndex(array, p => p.Equals(item));

    /// <summary>
    /// 配列に対する操作 <see cref="Array.FindIndex{T}(T[], Predicate{T})"/> を準の検索方法で拡張メソッド化します。
    /// （見つからない場合-1）
    /// </summary>
    public static int FindIndex<T>(this T[] array, Predicate<T> match) => Array.FindIndex(array, match);

    //
    // (2) 配列に要素を追加する
    // - - - - - - - - - - - - - - - - - - - -

    /// <summary>
    /// 配列の先頭に値を追加し、値が追加された新しい配列を取得します。
    /// </summary>
    public static T[] InsertTop<T>(this T[] array, T value)
    {
        var newArray = new T[array.Length + 1];
        newArray[0] = value;
        Array.Copy(array, 0, newArray, 1, array.Length);
        return newArray;
    }

    /// <summary>
    /// 配列の最後に値を追加し、値が追加された新しい配列を取得します。
    /// </summary>
    public static T[] InsertLast<T>(this T[] array, T value)
    {
        var newArray = new T[array.Length + 1];
        Array.Copy(array, 0, newArray, 0, array.Length);
        newArray[newArray.Length - 1] = value;
        return newArray;
    }

    /// <summary>
    /// 指定した位置に値を追加し、値が追加された新しい配列を取得します。
    /// </summary>
    public static T[] Insert<T>(this T[] array, int index, T value)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (index < 0 || index >= array.Length)
            throw new ArgumentOutOfRangeException($"index is out of range. index={index}.");

        var newArray = new T[array.Length + 1];
        Array.Copy(array, 0, newArray, 0, index); // インデックスより前
        newArray[index] = value;
        Array.Copy(array, index, newArray, index + 1, array.Length - index);

        return newArray;
    }

    /// <summary>
    /// 指定した位置に collection で指定した要素を連続で追加し、値が追加された新しい配列を取得します。
    /// </summary>
    public static T[] InsertRange<T>(this T[] array, int index, IEnumerable<T> collection)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (index < 0 || index >= array.Length)
            throw new ArgumentOutOfRangeException($"index is out of range. index={index}.");

        int len = collection.Count();
        var newArray = new T[array.Length + len];

        Array.Copy(array, 0, newArray, 0, index); // インデックスより前

        int i = 0;
        foreach (var item in collection)
        {
            newArray[index + i++] = item;
        }
        Array.Copy(array, index, newArray, index + len, array.Length - index);

        return newArray;
    }

    //
    // (3) 特定の要素を削除
    // - - - - - - - - - - - - - - - - - - - -

    // 補足:
    // 以下の削除処理は、メモリ効率と動作速度がかなり悪いため
    // 頻繁にこのような操作が発生する場合は System.Collections.Generic.List<T> の使用を検討すること。

    /// <summary>
    /// 配列から指定した位置の要素を削除した新しい配列を取得します。
    /// </summary>
    public static T[] RemoveAt<T>(this T[] array, int index)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (index < 0 || index >= array.Length)
            throw new ArgumentOutOfRangeException($"index is out of range. index={index}.");

        var newArray = new T[array.Length - 1];
        Array.Copy(array, 0, newArray, 0, index); // インデックスより前
        Array.Copy(array, index + 1, newArray, index, array.Length - index - 1); // インデックスより後

        return newArray;
    }

    /// <summary>
    /// 配列のいちばん最初に見つかった1つの要素を削除し新しい配列を取得します。削除されなかった場合null を返します。
    /// </summary>
    public static T[] RemoveFirst<T>(this T[] array, T item)
    {
        int index = array.FindIndex(item);
        if (index == -1) return null;

        return array.RemoveAt(index);
    }

    /// <summary>
    /// 指定した条件を満たす配列のいちばん最初に見つかった要素を削除し新しい配列を取得します。
    //// 削除されなかった場合null を返します。
    /// </summary>
    public static T[] RemoveFirst<T>(this T[] array, Predicate<T> match)
    {
        int index = array.FindIndex(match);
        if (index == -1) return null;

        return array.RemoveAt(index);
    }

    /// <summary>
    /// 指定した要素と同じ値を配列から全て削除します。削除されなかった場合 null を返します。
    /// </summary>
    public static T[] RemoveAll<T>(this T[] array, T item) => array.RemoveAll(elem => elem.Equals(item));

    /// <summary>
    /// 指定した条件を満たす要素を配列から全て削除します。削除されなかった場合 null を返します。
    /// </summary>
    public static T[] RemoveAll<T>(this T[] array, Predicate<T> match)
    {
        var list = new List<T>();
        for (int i = 0; i < array.Length; i++)
        {
            if (!match(array[i]))
            {
                list.Add(array[i]);
            }
        }
        return list.Count == array.Length ? null : list.ToArray();
    }


    /// <summary>
    /// 配列の指定した2つのインデックス間の値を入れ替えます。
    /// </summary>
    public static void Swap<T>(this T[] array, int i, int j)
    {
        T tmp = array[i];
        array[i] = array[j];
        array[j] = tmp;
    }

    //
    // (5) Linq風の便利な操作
    // - - - - - - - - - - - - - - - - - - - -

    /// <summary>
    /// 指定した配列をリストに変換します。
    /// </summary>
    public static List<T> ToList<T>(this T[] array)
    {
        var list = new List<T>();
        for (int i = 0; i < array.Length; i++)
        {
            list.Add(array[i]);
        }
        return list;
    }

    /// <summary>
    /// 指定した配列を述語に従って新しい型の配列に変換します。
    /// </summary>
    public static Dest[] Convert<T, Dest>(this T[] array, Func<T, Dest> func)
    {
        var newArray = new Dest[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = func(array[i]);
        }
        return newArray;
    }

    /// <summary>
    /// 配列に対する操作 <see cref="Array.ForEach{T}(T[], Action{T}))"/> を拡張メソッド化します。
    /// </summary>
    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        Array.ForEach(array, action);
    }

    //
    // (6) 配列に対するソート操作
    // - - - - - - - - - - - - - - - - - - - -

    /// <summary>
    /// <see cref="Array.Sort(Array)"/> を拡張メソッド化します。
    /// </summary>
    public static void Sort<T>(this T[] array) => Array.Sort(array);

    /// <summary>
    /// <see cref="Array.Sort{T}(T[], Comparison{T})"/> を拡張メソッド化します。
    /// </summary>
    public static void Sort<T>(this T[] array, Comparison<T> comparer) => Array.Sort(array, comparer);
}
