namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 二进制选项类（不可变标记，使用静态只读字段）。 将原可写属性改为静态只读字段以避免运行时被篡改并提高库的稳定性。 额外提供：按 mark 可查的名称数组和从 <see cref="System.Type"/> 到 mark 的查表以提高查找性能与可维护性。
    /// </summary>
    public static class BinaryOptions
    {
        /// <summary>
        /// 空值编码。
        /// </summary>
        public const byte Nil = 0xc0;

        /// <summary>
        /// 布尔值 False 编码。
        /// </summary>
        public const byte False = 0xc2;

        /// <summary>
        /// 布尔值 True 编码。
        /// </summary>
        public const byte True = 0xc3;

        /// <summary>
        /// 32 位浮点数数据标记。
        /// </summary>
        public const byte Float32 = 0xca;

        /// <summary>
        /// 64 位浮点数数据标记。
        /// </summary>
        public const byte Float64 = 0xcb;

        /// <summary>
        /// 8 位无符号整数数据标记。
        /// </summary>
        public const byte UInt8 = 0xcc;

        /// <summary>
        /// 16 位无符号整数数据标记。
        /// </summary>
        public const byte UInt16 = 0xcd;

        /// <summary>
        /// 32 位无符号整数数据标记。
        /// </summary>
        public const byte UInt32 = 0xce;

        /// <summary>
        /// 64 位无符号整数数据标记。
        /// </summary>
        public const byte UInt64 = 0xcf;

        /// <summary>
        /// 8 位有符号整数数据标记。
        /// </summary>
        public const byte Int8 = 0xd0;

        /// <summary>
        /// 16 位有符号整数数据标记。
        /// </summary>
        public const byte Int16 = 0xd1;

        /// <summary>
        /// 32 位有符号整数数据标记。
        /// </summary>
        public const byte Int32 = 0xd2;

        /// <summary>
        /// 64 位有符号整数数据标记。
        /// </summary>
        public const byte Int64 = 0xd3;

        /// <summary>
        /// 数组长度编码标记。
        /// </summary>
        public const byte Array = 0xdd;

        /// <summary>
        /// Map 头标记。
        /// </summary>
        public const byte MapHeader = 0xde;

        public const byte Ex4 = 0x04;

        public const byte Ex8 = 0x06;

        public const byte Ex16 = 0x0a;

        public const byte Str8 = 0xd9;
        public const byte Str16 = 0xda;
        public const byte Str32 = 0xdb;

        /// <summary>
        /// 按 mark（0-255）可索引的名称数组。未设置项会被填为占位文本。 用于高性能按 mark 查找名称（通过索引直接获取，取代大量 if/else）。
        /// </summary>
        private static readonly string[] NameByMark = CreateNameByMark();

        /// <summary>
        /// 从 CLR 类型映射到二进制标记的查表（Type -&gt; mark）。 在序列化时可用于快速把 CLR 类型映射为对应的标记字节。
        /// </summary>
        private static readonly Dictionary<Type, byte> TypeToMark = CreateTypeToMark();

        private const string EmptyName = "当前标记为空";

        private static string[] CreateNameByMark()
        {
            var arr = new string[256];
            // 仅填充当前定义的标记，其他索引保持为 EmptyName
            arr[Nil] = nameof(Nil);
            arr[False] = nameof(False);
            arr[True] = nameof(True);
            arr[Float32] = nameof(Float32);
            arr[Float64] = nameof(Float64);
            arr[UInt8] = nameof(UInt8);
            arr[UInt16] = nameof(UInt16);
            arr[UInt32] = nameof(UInt32);
            arr[UInt64] = nameof(UInt64);
            arr[Int8] = nameof(Int8);
            arr[Int16] = nameof(Int16);
            arr[Int32] = nameof(Int32);
            arr[Int64] = nameof(Int64);
            arr[Str8] = nameof(Str8);
            arr[Str16] = nameof(Str16);
            arr[Str32] = nameof(Str32);
            arr[Array] = nameof(Array);
            arr[MapHeader] = nameof(MapHeader);
            arr[Ex4] = nameof(Ex4);
            arr[Ex8] = nameof(Ex8);
            arr[Ex16] = nameof(Ex16);

            for (int i = 0; i < arr.Length; i++)
            {
                if (string.IsNullOrEmpty(arr[i]))
                {
                    arr[i] = EmptyName;
                }
            }

            return arr;
        }

        private static Dictionary<Type, byte> CreateTypeToMark()
        {
            return new Dictionary<Type, byte>(16)
            {
                [typeof(float)] = Float32,
                [typeof(double)] = Float64,
                [typeof(byte)] = UInt8,
                [typeof(sbyte)] = Int8,
                [typeof(ushort)] = UInt16,
                [typeof(short)] = Int16,
                [typeof(uint)] = UInt32,
                [typeof(int)] = Int32,
                [typeof(ulong)] = UInt64,
                [typeof(long)] = Int64
            };
        }

        /// <summary>
        /// 根据标记字节获取对应的名称（如果该标记未定义则返回占位文本）。
        /// </summary>
        /// <param name="mark">标记字节（0-255）。</param>
        /// <returns>对应的名称字符串或占位文本。</returns>
        public static string GetNameByMark(byte mark)
        {
            return NameByMark[mark];
        }

        /// <summary>
        /// 获取指定泛型类型对应的标记字节。
        /// </summary>
        /// <typeparam name="T">要查询的 CLR 类型。</typeparam>
        /// <returns>对应的标记字节。</returns>
        /// <exception cref="KeyNotFoundException">当此类型没有映射到任何标记时抛出。</exception>
        public static byte GetByteByType<T>()
        {
            return GetByteByType(typeof(T));
        }

        /// <summary>
        /// 获取指定 <see cref="Type"/> 对应的标记字节。
        /// </summary>
        /// <param name="type">要查询的 CLR 类型。</param>
        /// <returns>对应的标记字节。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> 为 null 时抛出。</exception>
        /// <exception cref="KeyNotFoundException">当此类型没有映射到任何标记时抛出。</exception>
        public static byte GetByteByType(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            return TypeToMark[type];
        }
    }
}