using System.Collections;
using System.Text;

namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 表示一个值或者值的列表，可以存储单个值或值的列表。
    /// </summary>
    /// <typeparam name="T"> 值的类型。 </typeparam>
    internal class ValueOrList<T> : IList<T>
    {
        /// <summary>
        /// 存储单个值。 使用显式标志区分“未设置”（hasSingle=false）与“已设置为 default(T)”。
        /// </summary>
        private T single;

        private bool hasSingle;

        /// <summary>
        /// 存储值的列表。
        /// </summary>
        private ValueList<T> list;

        /// <summary>
        /// 获取集合中的元素数量。
        /// </summary>
        public int Count
        {
            get
            {
                if (!list.IsEmpty) return list.Count;
                return hasSingle ? 1 : 0;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示集合是只读的。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <param name="index"> 要获取或设置的元素的从零开始的索引。 </param>
        /// <returns> 指定索引处的元素。 </returns>
        public T this[int index]
        {
            get
            {
                if (!list.IsEmpty) return list[index];
                if (index == 0 && hasSingle) return single;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (!list.IsEmpty)
                {
                    list[index] = value;
                }
                else if (index == 0 && hasSingle)
                {
                    single = value;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public ValueOrList() : this(0)
        {
        }

        public ValueOrList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();

            if (capacity > 1)
                list = new(capacity);

            single = default!;
            hasSingle = false;
        }

        /// <summary>
        /// 将对象添加到集合的末尾。
        /// </summary>
        /// <param name="item"> 要添加到集合末尾的对象。 </param>
        public void Add(T item)
        {
            if (!list.IsEmpty)
            {
                list.Add(item);
            }
            else if (!hasSingle)
            {
                single = item;
                hasSingle = true;
            }
            else
            {
                list = new(2) { single, item };
                single = default!;
                hasSingle = false;
            }
        }

        /// <summary>
        /// 从集合中移除所有元素。
        /// </summary>
        public void Clear()
        {
            single = default!;
            hasSingle = false;
            list.Clear();
        }

        /// <summary>
        /// 确定集合中是否包含特定元素。
        /// </summary>
        /// <param name="item"> 要在集合中定位的对象。 </param>
        /// <returns> 如果集合包含指定的元素，则为 true；否则为 false。 </returns>
        public bool Contains(T item)
        {
            if (!list.IsEmpty) return list.Contains(item);
            if (hasSingle) return EqualityComparer<T>.Default.Equals(single, item);
            return false;
        }

        /// <summary>
        /// 将整个集合复制到兼容的一维数组中，从指定索引处开始复制。
        /// </summary>
        /// <param name="array"> 一维 System.TArray，它是目标数组的起始位置。 </param>
        /// <param name="arrayIndex"> array 中从零开始的索引，从此处开始复制集合中的元素。 </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (!list.IsEmpty)
            {
                list.CopyTo(array, arrayIndex);
            }
            else if (hasSingle)
            {
                array[arrayIndex] = single;
            }
        }

        /// <summary>
        /// 返回循环访问集合的枚举器。
        /// </summary>
        /// <returns> 一个可用于循环访问集合的枚举器。 </returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (!list.IsEmpty)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i];
                }
            }
            else if (hasSingle)
            {
                yield return single;
            }
        }

        /// <summary>
        /// 返回循环访问 System.Collections 的枚举器。
        /// </summary>
        /// <returns> 一个可用于循环访问集合的枚举器。 </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 在集合中搜索指定的对象，并返回其从零开始的索引。
        /// </summary>
        /// <param name="item"> 要在集合中定位的对象。 </param>
        /// <returns> 在集合中对象第一次出现的从零开始的索引；如果未找到对象，则为 -1。 </returns>
        public int IndexOf(T item)
        {
            if (!list.IsEmpty) return list.IndexOf(item);
            if (hasSingle && EqualityComparer<T>.Default.Equals(single, item)) return 0;
            return -1;
        }

        /// <summary>
        /// 在集合的指定索引处插入一个元素。
        /// </summary>
        /// <param name="index"> 在集合中插入元素的位置的索引。 </param>
        /// <param name="item"> 要插入集合的元素。 </param>
        public void Insert(int index, T item)
        {
            if (!list.IsEmpty)
            {
                list.Insert(index, item);
            }
            else if (!hasSingle && index == 0)
            {
                single = item;
                hasSingle = true;
            }
            else if (hasSingle && index == 0)
            {
                list = new(2) { item, single };
                single = default!;
                hasSingle = false;
            }
            else if (hasSingle && index == 1)
            {
                list = new(2) { single, item };
                single = default!;
                hasSingle = false;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// 从集合中移除特定对象的第一个匹配项。
        /// </summary>
        /// <param name="item"> 要从集合中移除的对象。 </param>
        /// <returns> 如果已从集合中成功移除 item，则为 true；否则为 false。如果在集合中未找到 item，该方法也返回 false。 </returns>
        public bool Remove(T item)
        {
            if (!list.IsEmpty)
            {
                return list.Remove(item);
            }
            else if (hasSingle && EqualityComparer<T>.Default.Equals(single, item))
            {
                single = default!;
                hasSingle = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除集合中指定索引处的元素。
        /// </summary>
        /// <param name="index"> 要移除的元素的从零开始的索引。 </param>
        public void RemoveAt(int index)
        {
            if (!list.IsEmpty)
            {
                list.RemoveAt(index);
                if (list.Count == 1)
                {
                    single = list[0];
                    hasSingle = true;
                    list.Dispose();
                }
            }
            else if (hasSingle && index == 0)
            {
                single = default!;
                hasSingle = false;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// 在集合中查找满足指定条件的元素。
        /// </summary>
        /// <param name="predicate"> 一个用于测试每个元素的条件。 </param>
        /// <returns> 如果找到满足条件的元素，则返回该元素；否则返回默认值。 </returns>
        public T? Find(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                if (predicate(this[i]))
                {
                    return this[i];
                }
            }
            return default;
        }

        public override string ToString()
        {
            if (Count == 0)
                return "<empty>";
            StringBuilder sb = new();

            for (int i = 0; i < Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(this[i]?.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将一组值添加到集合中。
        /// </summary>
        /// <param name="values"> 一组值 </param>
        public void AddRange(IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                Add(item);
            }
        }
    }
}