

namespace ExtenderApp.Serialization.Contracts
{
    /// <summary>
    /// 二进制编码定义。
    /// </summary>
    public struct BinaryCode
    {
        /// <summary>
        /// 最小固定整数编码值。
        /// </summary>
        public byte MinFixInt { get; set; } = 0x00;

        /// <summary>
        /// 最大固定整数编码值。
        /// </summary>
        public byte MaxFixInt { get; set; } = 0x7f;

        /// <summary>
        /// 最小固定映射编码值。
        /// </summary>
        public byte MinFixMap { get; set; } = 0x80;

        /// <summary>
        /// 最大固定映射编码值。
        /// </summary>
        public byte MaxFixMap { get; set; } = 0x8f;

        /// <summary>
        /// 最小固定数组编码值。
        /// </summary>
        public byte MinFixArray { get; set; } = 0x90;

        /// <summary>
        /// 最大固定数组编码值。
        /// </summary>
        public byte MaxFixArray { get; set; } = 0x9f;

        /// <summary>
        /// 最小固定字符串编码值。
        /// </summary>
        public byte MinFixStr { get; set; } = 0xa0;

        /// <summary>
        /// 最大固定字符串编码值。
        /// </summary>
        public byte MaxFixStr { get; set; } = 0xbf;

        /// <summary>
        /// 空值编码。
        /// </summary>
        public byte Nil { get; set; } = 0xc0;

        /// <summary>
        /// 未使用编码。
        /// </summary>
        public byte NeverUsed { get; set; } = 0xc1;

        /// <summary>
        /// 布尔值False编码。
        /// </summary>
        public byte False { get; set; } = 0xc2;

        /// <summary>
        /// 布尔值True编码。
        /// </summary>
        public byte True { get; set; } = 0xc3;

        /// <summary>
        /// 8位二进制数据编码。
        /// </summary>
        public byte Bin8 { get; set; } = 0xc4;

        /// <summary>
        /// 16位二进制数据编码。
        /// </summary>
        public byte Bin16 { get; set; } = 0xc5;

        /// <summary>
        /// 32位二进制数据编码标记。
        /// </summary>
        public byte Bin32 { get; set; } = 0xc6;

        /// <summary>
        /// 1字节扩展格式数据开头标记。
        /// </summary>
        public byte FixExt1 { get; set; } = 0xd4;

        /// <summary>
        /// 2字节扩展格式数据开头标记。
        /// </summary>
        public byte FixExt2 { get; set; } = 0xd5;

        /// <summary>
        /// 4字节扩展格式数据开头标记。
        /// </summary>
        public byte FixExt4 { get; set; } = 0xd6;

        /// <summary>
        /// 8字节扩展格式数据开头标记。
        /// </summary>
        public byte FixExt8 { get; set; } = 0xd7;

        /// <summary>
        /// 16字节扩展格式数据开头标记。
        /// </summary>
        public byte FixExt16 { get; set; } = 0xd8;

        /// <summary>
        /// 8位扩展类型数据标记。
        /// </summary>
        public byte Ext8 { get; set; } = 0xc7;

        /// <summary>
        /// 16位扩展类型数据标记。
        /// </summary>
        public byte Ext16 { get; set; } = 0xc8;

        /// <summary>
        /// 32位扩展类型数据标记。
        /// </summary>
        public byte Ext32 { get; set; } = 0xc9;

        /// <summary>
        /// 32位浮点数数据标记。
        /// </summary>
        public byte Float32 { get; set; } = 0xca;

        /// <summary>
        /// 64位浮点数数据标记。
        /// </summary>
        public byte Float64 { get; set; } = 0xcb;

        /// <summary>
        /// 8位无符号整数数据标记。
        /// </summary>
        public byte UInt8 { get; set; } = 0xcc;

        /// <summary>
        /// 16位无符号整数数据标记。
        /// </summary>
        public byte UInt16 { get; set; } = 0xcd;

        /// <summary>
        /// 32位无符号整数数据标记。
        /// </summary>
        public byte UInt32 { get; set; } = 0xce;

        /// <summary>
        /// 64位无符号整数数据标记。
        /// </summary>
        public byte UInt64 { get; set; } = 0xcf;

        /// <summary>
        /// 8位有符号整数数据标记。
        /// </summary>
        public byte Int8 { get; set; } = 0xd0;

        /// <summary>
        /// 16位有符号整数数据标记。
        /// </summary>
        public byte Int16 { get; set; } = 0xd1;

        /// <summary>
        /// 32位有符号整数数据标记。
        /// </summary>
        public byte Int32 { get; set; } = 0xd2;

        /// <summary>
        /// 64位有符号整数数据标记。
        /// </summary>
        public byte Int64 { get; set; } = 0xd3;

        /// <summary>
        /// 8位字符串长度编码标记。
        /// </summary>
        public byte Str8 { get; set; } = 0xd9;

        /// <summary>
        /// 16位字符串长度编码标记。
        /// </summary>
        public byte Str16 { get; set; } = 0xda;

        /// <summary>
        /// 32位字符串长度编码标记。
        /// </summary>
        public byte Str32 { get; set; } = 0xdb;

        /// <summary>
        /// 16位数组长度编码标记。
        /// </summary>
        public byte Array16 { get; set; } = 0xdc;

        /// <summary>
        /// 32位数组长度编码标记。
        /// </summary>
        public byte Array32 { get; set; } = 0xdd;

        /// <summary>
        /// 16位映射长度编码标记。
        /// </summary>
        public byte Map16 { get; set; } = 0xde;

        /// <summary>
        /// 32位映射长度编码标记。
        /// </summary>
        public byte Map32 { get; set; } = 0xdf;

        /// <summary>
        /// 最小固定负整数标记值。
        /// </summary>
        public byte MinNegativeFixInt { get; set; } = 0xe0;

        /// <summary>
        /// 最大固定负整数标记值。
        /// </summary>
        public byte MaxNegativeFixInt { get; set; } = 0xff;

        public BinaryCode()
        {
        }
    }
}
