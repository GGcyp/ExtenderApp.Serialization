namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 表示各数值类型在二进制编码时的“总字节长度（含类型码）”。
    /// </summary>
    /// <remarks>
    /// 用于预估缓冲区大小与边界检查。默认值与 MessagePack 规范一致：
    /// 总长度 = 1 字节类型码 + N 字节数据（大端存储）。不包含容器（数组/映射/扩展/字符串）的额外头部开销。
    /// 注意：正/负固定整数（fixint/negative fixint）因仅 1 字节编码，不在此处体现。
    /// </remarks>
    public struct BinaryLength
    {
        /// <summary>
        /// Nil 的编码总长度（1 字节：类型码 1）。
        /// </summary>
        public int Nil { get; }

        /// <summary>
        /// Byte 的编码总长度（1 字节： 数据 1）。
        /// </summary>
        public int Byte { get; }

        /// <summary>
        /// int8 的编码总长度（2 字节：类型码 1 + 数据 1）。
        /// </summary>
        public int Int8 { get; }

        /// <summary>
        /// int16 的编码总长度（3 字节：类型码 1 + 数据 2）。
        /// </summary>
        public int Int16 { get; }

        /// <summary>
        /// int32 的编码总长度（5 字节：类型码 1 + 数据 4）。
        /// </summary>
        public int Int32 { get; }

        /// <summary>
        /// int64 的编码总长度（9 字节：类型码 1 + 数据 8）。
        /// </summary>
        public int Int64 { get; }

        /// <summary>
        /// uint8 的编码总长度（2 字节：类型码 1 + 数据 1）。
        /// </summary>
        public int UInt8 { get; }

        /// <summary>
        /// uint16 的编码总长度（3 字节：类型码 1 + 数据 2）。
        /// </summary>
        public int UInt16 { get; }

        /// <summary>
        /// uint32 的编码总长度（5 字节：类型码 1 + 数据 4）。
        /// </summary>
        public int UInt32 { get; }

        /// <summary>
        /// uint64 的编码总长度（9 字节：类型码 1 + 数据 8）。
        /// </summary>
        public int UInt64 { get; }

        /// <summary>
        /// float32 的编码总长度（5 字节：类型码 1 + IEEE754 单精度 4 字节）。
        /// </summary>
        public int Float32 { get; }

        /// <summary>
        /// float64 的编码总长度（9 字节：类型码 1 + IEEE754 双精度 8 字节）。
        /// </summary>
        public int Float64 { get; }

        /// <summary>
        /// 使用与 MessagePack 规范一致的默认长度进行初始化。
        /// </summary>
        public BinaryLength()
        {
            Nil = 1;
            Byte = 1;
            Int8 = 2;
            Int16 = 3;
            Int32 = 5;
            Int64 = 9;
            UInt8 = 2;
            UInt16 = 3;
            UInt32 = 5;
            UInt64 = 9;
            Float32 = 5;
            Float64 = 9;
        }
    }
}
