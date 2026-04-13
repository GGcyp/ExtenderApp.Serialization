

namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 日期时间常量结构体
    /// </summary>
    public struct BinaryDateTime
    {
        /// <summary>
        /// 从.NET BCL（基础类库）角度来看，对应Unix纪元（1970年1月1日 00:00:00 UTC）的秒数。
        /// </summary>
        public long BclSecondsAtUnixEpoch { get; set; }

        /// <summary>
        /// 每一个时间刻度（Tick）对应的纳秒数。
        /// </summary>
        public int NanosecondsPerTick { get; set; }

        /// <summary>
        /// 表示Unix纪元（1970年1月1日 00:00:00 UTC）的 <see cref="DateTime"/> 实例，其 <see cref="DateTime.Kind"/> 为 <see cref="DateTimeKind.Utc"/>。
        /// </summary>
        public DateTime UnixEpoch { get; set; }

        /// <summary>
        /// 日期时间值。
        /// </summary>
        public sbyte DateTime { get; set; }

        public BinaryDateTime()
        {
            BclSecondsAtUnixEpoch = 62135596800;
            NanosecondsPerTick = 100;
            UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime = -1;
        }
    }
}
