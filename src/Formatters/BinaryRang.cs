namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 定义用于二进制编码（兼容 MessagePack 规范）的“固定段（fix）”范围集合。
    /// </summary>
    /// <remarks>
    /// 这些范围值用于编码决策：当数值或长度落入指定区间时，优先使用更紧凑的固定格式（fixint/fixstr/fixarray/fixmap）；
    /// 超出范围则回退到变长格式（如 int8/16/32、str8/16/32、array16/32、map16/32）。
    /// <para>默认值对齐 MessagePack 规范：</para>
    /// <list type="bullet">
    ///   <item><description>负固定整数：[-32, -1]</description></item>
    ///   <item><description>正固定整数：[0, 127]</description></item>
    ///   <item><description>固定字符串长度：[0, 31]</description></item>
    ///   <item><description>固定数组元素个数：[0, 15]</description></item>
    ///   <item><description>固定映射项个数：[0, 15]</description></item>
    /// </list>
    /// 如需自定义，仅当你在自定义协议中明确改变了“何时使用固定格式”的边界时再调整这些值；
    /// 读写双方必须保持一致。请确保各属性遵守各自的有效范围（见属性注释）。
    /// </remarks>
    public struct BinaryRang
    {
        /// <summary>
        /// 固定负整数（negative fixint）的最小值（含）。
        /// </summary>
        /// <remarks>
        /// 默认值为 -32。有效范围通常为 [-32, -1] 且应 ≤ <see cref="MaxFixNegativeInt"/>。
        /// 当一个有符号整数值 ≥ <see cref="MinFixNegativeInt"/> 且 ≤ <see cref="MaxFixNegativeInt"/> 时，
        /// 编码器可使用负固定整数单字节编码（0xE0..0xFF）。
        /// </remarks>
        public int MinFixNegativeInt { get; set; }

        /// <summary>
        /// 固定负整数（negative fixint）的最大值（含）。
        /// </summary>
        /// <remarks>
        /// 默认值为 -1。有效范围通常为 [-32, -1] 且应 ≥ <see cref="MinFixNegativeInt"/>。
        /// </remarks>
        public int MaxFixNegativeInt { get; set; }

        /// <summary>
        /// 固定正整数（positive fixint）的最大值（含）。
        /// </summary>
        /// <remarks>
        /// 默认值为 127。有效范围通常为 [0, 127]。
        /// 当无符号或非负整数值 ≤ 本值时，编码器可使用正固定整数单字节编码（0x00..0x7F）。
        /// </remarks>
        public int MaxFixPositiveInt { get; set; }

        /// <summary>
        /// 固定字符串（fixstr）的最小字节长度（含）。
        /// </summary>
        /// <remarks>
        /// 默认值为 0。有效范围通常为 [0, <see cref="MaxFixStringLength"/>]。
        /// </remarks>
        public int MinFixStringLength { get; set; }

        /// <summary>
        /// 固定字符串（fixstr）的最大字节长度（含）。
        /// </summary>
        /// <remarks>
        /// 默认值为 31。有效范围通常为 [<see cref="MinFixStringLength"/>, 31]。
        /// 当字符串字节长度 ≤ 本值时，可使用 fixstr 单字节头（0xA0..0xBF）。
        /// </remarks>
        public int MaxFixStringLength { get; set; }

        /// <summary>
        /// 固定映射（fixmap）的最大项个数（含）。
        /// </summary>
        /// <remarks>
        /// 默认值为 15。有效范围通常为 [0, 15]。
        /// 当映射项个数 ≤ 本值时，可使用 fixmap 单字节头（0x80..0x8F）。
        /// </remarks>
        public int MaxFixMapCount { get; set; }

        /// <summary>
        /// 固定数组（fixarray）的最大元素个数（含）。
        /// </summary>
        /// <remarks>
        /// 默认值为 15。有效范围通常为 [0, 15]。
        /// 当数组元素个数 ≤ 本值时，可使用 fixarray 单字节头（0x90..0x9F）。
        /// </remarks>
        public int MaxFixArrayCount { get; set; }

        /// <summary>
        /// 使用与 MessagePack 规范一致的默认边界初始化各属性。
        /// </summary>
        /// <remarks>
        /// 默认设置：
        /// <list type="bullet">
        ///   <item><description><see cref="MinFixNegativeInt"/> = -32</description></item>
        ///   <item><description><see cref="MaxFixNegativeInt"/> = -1</description></item>
        ///   <item><description><see cref="MaxFixPositiveInt"/> = 127</description></item>
        ///   <item><description><see cref="MinFixStringLength"/> = 0</description></item>
        ///   <item><description><see cref="MaxFixStringLength"/> = 31</description></item>
        ///   <item><description><see cref="MaxFixMapCount"/> = 15</description></item>
        ///   <item><description><see cref="MaxFixArrayCount"/> = 15</description></item>
        /// </list>
        /// 修改这些边界会直接影响编码选择（是否使用 fix* 形式）。确保读写双方配置一致。
        /// </remarks>
        public BinaryRang()
        {
            MinFixNegativeInt = -32;
            MaxFixNegativeInt = -1;
            MaxFixPositiveInt = 127;
            MinFixStringLength = 0;
            MaxFixStringLength = 31;
            MaxFixMapCount = 15;
            MaxFixArrayCount = 15;
        }
    }
}
