using System.Buffers;
using System.Collections;

namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 使用 <see cref="ArrayPool{T}" /> 进行内存租赁的轻量 List 结构体实现，避免频繁堆分配。 非线程安全；适合短生命周期或热点路径。
    /// </summary>
    /// <typeparam name="T"> 元素类型。 </typeparam>
    internal struct ValueList<T> : IList<T>, IEquatable<ValueList<T>>, IDisposable
    {
        /// <summary>
        /// 数组池，用于优化数组分配与回收，减少 GC 压力。
        /// </summary>
        private readonly ArrayPool<T> _pool;

        /// <summary>
        /// 元素比较器缓存，避免频繁调用 <see cref="EqualityComparer{T}.Default" />。
        /// </summary>
        private readonly EqualityComparer<T> _eComparer;

        /// <summary>
        /// 实际存储元素的池租赁数组。已使用区间为 [0, Count)。 可能为 null（尚未分配）。
        /// </summary>
        private T[]? array;

        /// <summary>
        /// 用于外部锁定的对象引用（返回内部数组）。数组未初始化时抛出异常。
        /// </summary>
        public object LockObject => array ?? throw new NullReferenceException(nameof(ValueList<T>));

        /// <summary>
        /// 指示是否尚未分配底层数组。
        /// </summary>
        public bool IsEmpty => array == null;

        /// <summary>
        /// 始终返回 false；该结构可写。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 返回底层数组的 Span 视图。注意：如果 <c> array </c> 为 null 将得到默认 <see cref="Span{T}" />。 有效数据仅前 <see cref="Count" /> 个元素。
        /// </summary>
        public Span<T> SpanArray => array;

        /// <summary>
        /// 当前元素数量（有效长度）。范围：[0, Capacity]。
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 当前数组容量（底层数组长度）。为 0 表示尚未分配。
        /// </summary>
        public int Capacity => array?.Length ?? 0;

        /// <summary>
        /// 访问或设置指定索引处的元素。
        /// </summary>
        /// <param name="index"> 索引（0 ≤ index &lt; Count）。 </param>
        /// <exception cref="IndexOutOfRangeException"> 索引越界。 </exception>
        /// <remarks> TODO: 当前 <see cref="CheckIndex(int)" /> 允许 index == Count，会导致读取时潜在越界风险，应考虑修正。  </remarks>
        public T this[int index]
        {
            get
            {
                CheckIndex(index);
                return SpanArray[index];
            }
            set
            {
                CheckIndex(index);
                SpanArray[index] = value;
            }
        }

        /// <summary>
        /// 使用共享池初始化（延迟分配，首次添加时才真正租赁）。
        /// </summary>
        public ValueList() : this(ArrayPool<T>.Shared)
        {
        }

        /// <summary>
        /// 指定初始容量构造。容量为 0 时强制为 1。
        /// </summary>
        /// <param name="capacity"> 期望初始容量。 </param>
        /// <param name="pool"> 可选指定数组池。 </param>
        public ValueList(int capacity, ArrayPool<T>? pool = null)
            : this(pool ?? ArrayPool<T>.Shared)
        {
            Count = 0;
            Ensure(capacity == 0 ? 1 : capacity);
        }

        /// <summary>
        /// 通过现有 <see cref="Span{T}" /> 初始化；数据复制到新租赁数组。
        /// </summary>
        /// <param name="span"> 源数据。 </param>
        /// <param name="pool"> 数组池。 </param>
        public ValueList(Span<T> span, ArrayPool<T>? pool = null)
            : this(span.Length, pool)
        {
            Count = span.Length;
            span.CopyTo(array);
        }

        /// <summary>
        /// 通过枚举源集合依次添加元素（O(n)）。
        /// </summary>
        /// <param name="array"> 源集合。 </param>
        /// <param name="pool"> 数组池。 </param>
        public ValueList(IEnumerable<T> array, ArrayPool<T>? pool = null)
            : this(pool ?? ArrayPool<T>.Shared)
        {
            foreach (T item in array)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 使用指定数组池的基础构造函数（不分配数组）。
        /// </summary>
        public ValueList(ArrayPool<T> pool)
        {
            _pool = pool;
            _eComparer = EqualityComparer<T>.Default;
        }

        /// <summary>
        /// 检查索引有效性。当前实现允许 index == Count，被访问时可能越界。
        /// </summary>
        /// <param name="index"> 要检查的索引。 </param>
        /// <exception cref="IndexOutOfRangeException"> 非法索引。 </exception>
        private void CheckIndex(int index)
        {
            if (index < 0 || index > Count)
            {
                throw new IndexOutOfRangeException(string.Format("数据位置超过数据界限:{0}", index));
            }
        }

        /// <summary>
        /// 根据需要扩容。新容量 = Count + sizeHint。若已有容量足够则不操作。
        /// </summary>
        /// <param name="sizeHint"> 附加需求量（不是最终容量）。 </param>
        /// <remarks> 当前调用处多以 <c> Count + 1 </c> 传入，导致实际新容量计算为 <c> Count + (Count + 1) </c>，可能过度扩容。 建议调用位置只传递需要新增的元素数量（通常为 1）。 </remarks>
        private void Ensure(int sizeHint = 1)
        {
            int newCapacity = Count + sizeHint;
            if (newCapacity <= Capacity)
            {
                return;
            }

            var oldArray = array;
            array = _pool.Rent(newCapacity);
            if (oldArray is null)
                return;
            if (Count > 0)
            {
                Array.Copy(oldArray, 0, array, 0, Count);
            }
            _pool.Return(oldArray);
        }

        /// <summary>
        /// 在尾部添加元素。均摊 O(1)，扩容时 O(n)。
        /// </summary>
        /// <param name="item"> 待添加元素。 </param>
        public void Add(T item)
        {
            Ensure(Count + 1); // 参见 Ensure 备注：可优化为 Ensure(1)
            SpanArray[Count] = item;
            Count++;
        }

        /// <summary>
        /// 将多个元素添加到尾部。O(m)，m 为添加元素数量。
        /// </summary>
        /// <param name="items"> </param>
        public void AddRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 按谓词移除首个匹配元素。O(n)。
        /// </summary>
        /// <param name="predicate"> 匹配条件。 </param>
        /// <returns> 被移除的元素；未找到则返回 default。 </returns>
        public T? Remove(Predicate<T> predicate)
        {
            if (array == null)
                return default;
            for (int i = 0; i < Count; i++)
            {
                T temp = array[i];
                if (predicate(temp))
                {
                    RemoveAt(i);
                    return temp;
                }
            }

            return default(T);
        }

        /// <summary>
        /// 移除首个等于指定值的元素。O(n)。
        /// </summary>
        /// <param name="item"> 目标值。 </param>
        /// <returns> 是否成功移除。 </returns>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 删除指定索引处元素，并向前移动后续元素。O(n)。
        /// </summary>
        /// <param name="index"> 元素索引。 </param>
        /// <exception cref="NullReferenceException"> 数组未分配。 </exception>
        /// <exception cref="IndexOutOfRangeException"> 索引越界。 </exception>
        public void RemoveAt(int index)
        {
            if (array is null)
                return;
            CheckIndex(index);

            for (int i = index; i < Count - 1; i++)
            {
                SpanArray[i] = SpanArray[i + 1];
            }
            Count--;
        }

        /// <summary>
        /// 清空所有元素（将前 Count 个位置写为 default；不归还数组）。O(n)。
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                SpanArray[i] = default;
            }
            Count = 0;
        }

        /// <summary>
        /// 查找元素首次出现的索引。O(n)。
        /// </summary>
        public int IndexOf(T item)
        {
            return IndexOf(item, null);
        }

        /// <summary>
        /// 使用指定比较器查找元素索引。O(n)。
        /// </summary>
        /// <param name="item"> 目标元素。 </param>
        /// <param name="comparer"> 比较器（null 使用默认）。 </param>
        /// <returns> 索引或 -1。 </returns>
        public int IndexOf(T item, EqualityComparer<T>? comparer = null)
        {
            if (Count == 0 || array is null) return -1;

            comparer = comparer ?? _eComparer;

            for (int i = 0; i < Count; i++)
            {
                if (comparer.Equals(item, SpanArray[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 在指定位置插入元素，后续元素后移。O(n)。
        /// </summary>
        /// <param name="index"> 插入位置。 </param>
        /// <param name="item"> 元素。 </param>
        public void Insert(int index, T item)
        {
            Ensure(Count + 1); // 建议改为 Ensure(1) 以避免过度扩容
            for (int i = Count; i > index; i--)
            {
                SpanArray[i] = SpanArray[i - 1];
            }

            SpanArray[index] = item;
            Count++;
        }

        /// <summary>
        /// 判断是否包含元素。O(n)。
        /// </summary>
        public bool Contains(T item)
        {
            return Contains(item, null);
        }

        /// <summary>
        /// 使用比较器判断是否包含元素。O(n)。
        /// </summary>
        public bool Contains(T item, EqualityComparer<T>? comparer = null)
        {
            return IndexOf(item, comparer) >= 0;
        }

        /// <summary>
        /// 判断是否存在满足谓词的元素。O(n)。
        /// </summary>
        public bool Contains(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = SpanArray[i];
                if (predicate(temp))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 返回首个满足条件的元素，找不到返回 default。O(n)。
        /// </summary>
        public T? FirstOrDefault(Func<T, bool> func)
        {
            if (array is null)
                return default;

            T? item = default;
            for (int i = 0; i < Count; i++)
            {
                if (func(SpanArray[i]))
                {
                    item = SpanArray[i];
                    break;
                }
            }
            return item;
        }

        /// <summary>
        /// 返回首个满足带额外参数对比条件的元素。O(n)。
        /// </summary>
        public T? FirstOrDefault<T1>(Func<T, T1, bool> func, T1 contrast)
        {
            if (array is null)
                return default;

            T? item = default;
            for (int i = 0; i < Count; i++)
            {
                if (func(SpanArray[i], contrast))
                {
                    item = SpanArray[i];
                    break;
                }
            }
            return item;
        }

        /// <summary>
        /// 判断与另一 <see cref="ValueList{T}" /> 是否引用同一底层数组（浅比较）。
        /// </summary>
        public bool Equals(ValueList<T> list)
        {
            if (array is null && list.array is null)
                return true;
            if (array is null || list.array is null)
                return false;
            return array.Equals(list.array);
        }

        /// <summary>
        /// 复制当前元素到新数组（长度 = Count）。O(n)。
        /// </summary>
        public T[] ToArray()
        {
            T[] result = new T[Count];
            CopyTo(result);
            return result;
        }

        /// <summary>
        /// 将元素复制到目标 Span。长度不足时抛出异常。O(n)。
        /// </summary>
        /// <param name="span"> 目标 Span（长度 ≥ Count）。 </param>
        /// <exception cref="ArgumentOutOfRangeException"> span 太小。 </exception>
        public void CopyTo(Span<T> span)
        {
            // 无法装下全部数据
            if (span.Length < Count)
                throw new ArgumentOutOfRangeException(nameof(span));

            array.AsSpan(0, Count).CopyTo(span);
        }

        /// <summary>
        /// 将元素复制到数组的指定起始位置。O(n)。
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array.AsSpan(arrayIndex));
        }

        /// <summary>
        /// 返回迭代器（按索引顺序）。不分配。O(n)。
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return SpanArray[i];
            }
        }

        /// <summary>
        /// 非泛型枚举器实现。
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 查找首个匹配谓词的元素。O(n)。
        /// </summary>
        public T? Find(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = SpanArray[i];
                if (predicate(temp))
                {
                    return temp;
                }
            }
            return default;
        }

        /// <summary>
        /// 查找首个匹配带外部值条件的元素。O(n)。
        /// </summary>
        public T? Find<TValue>(Func<T, TValue, bool> predicate, TValue value)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = SpanArray[i];
                if (predicate(temp, value))
                {
                    return temp;
                }
            }
            return default;
        }

        /// <summary>
        /// 返回首个匹配带外部值条件的元素索引。O(n)。
        /// </summary>
        public int FindIndex<TValue>(Func<T, TValue, bool> predicate, TValue value)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = SpanArray[i];
                if (predicate(temp, value))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取最后一个元素，若为空返回 default。O(1)。
        /// </summary>
        public T? GetLast()
        {
            if (Count == 0)
                return default;
            return SpanArray[Count - 1];
        }

        public override bool Equals(object? obj)
            => obj is ValueList<T> other && Equals(other);

        public override string ToString()
        {
            if (Count == 0)
                return "[]";
            var span = array.AsSpan(0, Count);
            return "[" + string.Join(", ", span.ToArray()) + "]";
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Count);
            var span = array.AsSpan(0, Count);
            for (int i = 0; i < span.Length; i++)
            {
                hash.Add(span[i], _eComparer);
            }
            return hash.ToHashCode();
        }

        public void Dispose()
        {
            if (array != null)
            {
                _pool.Return(array);
                array = null;
                Count = 0;
            }
        }
    }
}