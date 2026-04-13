using System.Linq.Expressions;
using System.Reflection;
using ExtenderApp.Buffer;
using ExtenderApp.Serialization.Abstractions;

namespace ExtenderApp.Serialization.Formatters.AutoFormatters
{
    /// <summary>
    /// 通过表达式树为类型 <typeparamref name="T"/> 自动生成序列化/反序列化/长度估算委托的通用格式化器。 使用方式：派生类在构造时收集需要处理的属性/字段并调用 <see cref="Init"/> 完成委托编译。
    /// </summary>
    /// <remarks>
    /// 工作流程：
    /// 1) 在构造函数中创建统一的参数表达式并通过 <see cref="AutoMemberDetailsStore"/> 收集成员；
    /// 2) 基于收集的成员拼装表达式树，分别编译出 Serialize/Deserialize/GetLength 委托；
    /// 3) 对于引用类型，序列化/反序列化时会处理 Nil 标记；长度估算时如果为 null，返回 1（Nil 标记）。
    /// </remarks>
    public abstract class AutoFormatter<T> : ResolverFormatter<T>
    {
        #region ParameterExpressions

        /// <summary>
        /// 用于表达式树的共享参数：ref <see cref="BinaryWriterAdapter"/>（顺序缓冲写入适配器）， 使用引用类型参数以匹配委托签名中对写入适配器的按引用传递（ref）语义。
        /// </summary>
        public static readonly ParameterExpression BufferWriterParameter = Expression.Parameter(typeof(BinaryWriterAdapter).MakeByRefType(), "writer");

        /// <summary>
        /// 用于表达式树的共享参数：ref <see cref="SpanWriter{byte}"/>。
        /// </summary>
        public static readonly ParameterExpression SpanWriterParameter = Expression.Parameter(typeof(SpanWriter<byte>).MakeByRefType(), "writer");

        /// <summary>
        /// 用于表达式树的共享参数：ref <see cref="BinaryReaderAdapter"/>（顺序缓冲读取适配器）， 使用引用类型参数以匹配委托签名中对读取适配器的按引用传递（ref）语义。
        /// </summary>
        public static readonly ParameterExpression BufferReaderParameter = Expression.Parameter(typeof(BinaryReaderAdapter).MakeByRefType(), "reader");

        /// <summary>
        /// 用于表达式树的共享参数：ref <see cref="SpanReader{byte}"/>。
        /// </summary>
        public static readonly ParameterExpression SpanReaderParameter = Expression.Parameter(typeof(SpanReader<byte>).MakeByRefType(), "reader");

        #endregion ParameterExpressions

        private readonly SerializeWriterMethod _serializeWriter = static (ref BinaryWriterAdapter _, T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法序列化:{0}", typeof(T).FullName));

        private readonly SerializeSpanMethod _serializeSpan = static (ref SpanWriter<byte> _, T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法序列化:{0}", typeof(T).FullName));

        private readonly DeserializeReaderMethod _deserializeReader = static (ref BinaryReaderAdapter _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法反序列化:{0}", typeof(T).FullName));

        private readonly DeserializeSpanMethod _deserializeSpan = static (ref SpanReader<byte> _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法反序列化:{0}", typeof(T).FullName));

        private readonly GetLengthMethod _getLength = static (T _) =>
                throw new InvalidOperationException(string.Format("未调用Init,无法获取序列化后长度:{0}", typeof(T).FullName));

        /// <summary>
        /// 指示 <typeparamref name="T"/> 是否为引用类型。用于在序列化/反序列化/长度估算时处理 Nil 语义。
        /// </summary>
        private readonly bool IsClass;

        /// <summary>
        /// 默认预估长度（字节数）。为成员默认长度之和；若 <typeparamref name="T"/> 为引用类型，会额外包含 1 字节的空值标记。
        /// </summary>
        public override int DefaultLength { get; }

        /// <summary>
        /// 使用解析器构造并编译当前类型的序列化/反序列化/长度估算委托。
        /// </summary>
        /// <param name="resolver">格式化器解析器。</param>
        public AutoFormatter(BinaryFormatterResolver resolver) : base(resolver)
        {
            Type type = typeof(T);
            ParameterExpression parameter = Expression.Parameter(typeof(T), "value");

            AutoMemberDetailsStore store = new(this, parameter);
            Init(store);

            List<AutoMemberDetails> list = store.memberDetails;
            int length = list.Count;

            Expression[] serializesBuffer = new Expression[length];
            Expression[] serializesSpan = new Expression[length];
            MemberBinding[] deserializesBuffer = new MemberBinding[length];
            MemberBinding[] deserializesSpan = new MemberBinding[length];
            Expression[] getLengths = new Expression[length];

            for (int i = 0; i < length; i++)
            {
                var details = list[i];
                serializesBuffer[i] = details.SerializeBuffer;
                serializesSpan[i] = details.SerializeSpan;
                deserializesBuffer[i] = details.DeserializeBuffer;
                deserializesSpan[i] = details.DeserializeSpan;
                getLengths[i] = details.GetLength;
                DefaultLength += details.DefaultLength;
            }

            IsClass = type.IsClass;
            if (IsClass)
                DefaultLength = NilLength;

            // 编译 Serialize 方法
            _serializeWriter = Expression.Lambda<SerializeWriterMethod>(Expression.Block(serializesBuffer), BufferWriterParameter, parameter).Compile();
            _serializeSpan = Expression.Lambda<SerializeSpanMethod>(Expression.Block(serializesSpan), SpanWriterParameter, parameter).Compile();

            // 编译 Deserialize 方法
            NewExpression newExpr = type.IsValueType
                ? Expression.New(type)
                : Expression.New(type.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException(string.Format("类型 {0} 缺少公共无参构造函数。", type.FullName)));

            _deserializeReader = Expression.Lambda<DeserializeReaderMethod>(Expression.MemberInit(newExpr, deserializesBuffer), BufferReaderParameter).Compile();
            _deserializeSpan = Expression.Lambda<DeserializeSpanMethod>(Expression.MemberInit(newExpr, deserializesSpan), SpanReaderParameter).Compile();

            // 编译 GetLength 方法
            Expression totalLen = getLengths.Length > 0 ? getLengths[0] : Expression.Constant(0L);
            for (int i = 1; i < getLengths.Length; i++)
            {
                totalLen = Expression.Add(totalLen, getLengths[i]);
            }
            _getLength = Expression.Lambda<GetLengthMethod>(totalLen, parameter).Compile();
        }

        /// <summary>
        /// 序列化到缓冲区写入器中。
        /// </summary>
        /// <typeparam name="TBufferWriter"></typeparam>
        /// <param name="writer">写入目标缓冲区。</param>
        /// <param name="value">要序列化的值。</param>
        public override sealed void Serialize(ref BinaryWriterAdapter writer, T value)
        {
            if (IsClass && value is null)
            {
                WriteNil(ref writer);
                return;
            }
            _serializeWriter(ref writer, value);
        }

        /// <summary>
        /// 序列化到写入器。
        /// </summary>
        /// <param name="writer">写入目标写入器。</param>
        /// <param name="value">要序列化的值。</param>
        public override sealed void Serialize(ref SpanWriter<byte> writer, T value)
        {
            if (IsClass && value is null)
            {
                WriteNil(ref writer);
                return;
            }
            _serializeSpan(ref writer, value);
        }

        /// <summary>
        /// 从缓冲区读取并反序列化。
        /// </summary>
        /// <param name="reader">读取源读取器。</param>
        /// <returns>反序列化后的值。</returns>
        public override sealed T Deserialize(ref BinaryReaderAdapter reader)
        {
            if (IsClass && TryReadNil(ref reader))
                return default!;
            return _deserializeReader(ref reader);
        }

        /// <summary>
        /// 从读取器读取并反序列化。
        /// </summary>
        /// <param name="reader">读取源读取器。</param>
        /// <returns>反序列化后的值。</returns>
        public override sealed T Deserialize(ref SpanReader<byte> reader)
        {
            if (IsClass && TryReadNil(ref reader))
                return default!;
            return _deserializeSpan(ref reader);
        }

        /// <summary>
        /// 获取序列化后的长度。
        /// </summary>
        /// <param name="value">要估算的值。</param>
        /// <returns>序列化后的字节长度。</returns>
        public override sealed long GetLength(T value)
        {
            if (IsClass && value is null)
                return NilLength;
            return _getLength(value);
        }

        /// <summary>
        /// 初始化并收集需要自动处理的成员。
        /// </summary>
        /// <param name="store">成员收集器。</param>
        protected abstract void Init(AutoMemberDetailsStore store);

        /// <summary>
        /// 创建用于序列化的表达式调用。
        /// </summary>
        /// <param name="member">成员表达式。</param>
        /// <param name="method">序列化方法信息。</param>
        /// <param name="param">缓冲区/写入器参数。</param>
        /// <param name="formatter">成员对应的格式化器。</param>
        /// <returns>序列化调用表达式。</returns>
        private MethodCallExpression CreateSerializeExpression(MemberExpression member, MethodInfo method, ParameterExpression param, BinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), method.DeclaringType!);
            return Expression.Call(instance, method, param, member);
        }

        /// <summary>
        /// 创建用于反序列化的成员绑定表达式。
        /// </summary>
        /// <param name="memberInfo">成员信息。</param>
        /// <param name="method">反序列化方法信息。</param>
        /// <param name="param">读取器参数。</param>
        /// <param name="formatter">成员对应的格式化器。</param>
        /// <returns>成员绑定表达式。</returns>
        private MemberAssignment CreateDeserializeExpression(MemberInfo memberInfo, MethodInfo method, ParameterExpression param, BinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), method.DeclaringType!);
            var call = Expression.Call(instance, method, param);
            var targetType = memberInfo is PropertyInfo p ? p.PropertyType : ((FieldInfo)memberInfo).FieldType;
            return Expression.Bind(memberInfo, call.Type != targetType ? Expression.Convert(call, targetType) : call);
        }

        /// <summary>
        /// 创建用于长度估算的表达式调用。
        /// </summary>
        /// <param name="member">成员表达式。</param>
        /// <param name="method">长度估算方法信息。</param>
        /// <param name="formatter">成员对应的格式化器。</param>
        /// <returns>长度估算调用表达式。</returns>
        private MethodCallExpression CreateGetLengthExpression(MemberExpression member, MethodInfo method, BinaryFormatter formatter)
        {
            var instance = Expression.Convert(Expression.Constant(formatter), method.DeclaringType!);
            return Expression.Call(instance, method, member);
        }

        /// <summary>
        /// 为指定成员创建自动成员信息。
        /// </summary>
        /// <param name="parameter">目标实例参数表达式。</param>
        /// <param name="info">成员信息。</param>
        /// <returns>自动成员信息。</returns>
        private AutoMemberDetails CreateAutoMemberInfo(Expression parameter, MemberInfo info)
        {
            Type memberType = info is PropertyInfo p ? p.PropertyType : ((FieldInfo)info).FieldType;
            MemberExpression member = Expression.MakeMemberAccess(parameter, info);
            var formatter = GetFormatter(memberType);
            var md = GetClosedFormatterMethodInfoDetails(formatter, memberType);
            int defaultLength = GetClosedFormatterDefaultLength(formatter, memberType);

            return new AutoMemberDetails(
                CreateSerializeExpression(member, md.SerializeBuffer, BufferWriterParameter, formatter),
                CreateSerializeExpression(member, md.SerializeSpan, SpanWriterParameter, formatter),
                CreateDeserializeExpression(info, md.DeserializeBuffer, BufferReaderParameter, formatter),
                CreateDeserializeExpression(info, md.DeserializeSpan, SpanReaderParameter, formatter),
                CreateGetLengthExpression(member, md.GetLength, formatter),
                info.Name,
                defaultLength);
        }

        /// <summary>
        /// 通过闭包泛型 <see cref="BinaryFormatter{T}"/> 从非泛型引用读取 <see cref="BinaryFormatterMethodInfoDetails"/>（成员类型在编译期未知）。
        /// </summary>
        private static BinaryFormatterMethodInfoDetails GetClosedFormatterMethodInfoDetails(BinaryFormatter formatter, Type valueType)
        {
            var closed = typeof(BinaryFormatter<>).MakeGenericType(valueType);
            var prop = closed.GetProperty(nameof(BinaryFormatter<int>.MethodInfoDetails), BindingFlags.Public | BindingFlags.Instance);
            return (BinaryFormatterMethodInfoDetails)prop!.GetValue(formatter)!;
        }

        /// <summary>
        /// 通过闭包泛型读取成员格式化器的 <see cref="BinaryFormatter{T}.DefaultLength"/>。
        /// </summary>
        private static int GetClosedFormatterDefaultLength(BinaryFormatter formatter, Type valueType)
        {
            var closed = typeof(BinaryFormatter<>).MakeGenericType(valueType);
            var prop = closed.GetProperty(nameof(BinaryFormatter<int>.DefaultLength), BindingFlags.Public | BindingFlags.Instance);
            return (int)prop!.GetValue(formatter)!;
        }

        /// <summary>
        /// 成员收集器，用于构建自动序列化成员列表。
        /// </summary>
        protected struct AutoMemberDetailsStore
        {
            /// <summary>
            /// 自动格式化器实例。
            /// </summary>
            private AutoFormatter<T> autoFormatter;

            /// <summary>
            /// 目标实例参数表达式。
            /// </summary>
            private ParameterExpression parameter;

            /// <summary>
            /// 收集到的成员明细列表。
            /// </summary>
            public List<AutoMemberDetails> memberDetails;

            /// <summary>
            /// 初始化成员收集器。
            /// </summary>
            /// <param name="autoFormatter">自动格式化器实例。</param>
            /// <param name="parameter">目标实例参数表达式。</param>
            public AutoMemberDetailsStore(AutoFormatter<T> autoFormatter, ParameterExpression parameter)
            {
                this.autoFormatter = autoFormatter;
                this.parameter = parameter;
                memberDetails = new();
            }

            /// <summary>
            /// 添加指定成员选择表达式。
            /// </summary>
            /// <typeparam name="TMember">成员类型。</typeparam>
            /// <param name="selector">成员选择表达式。</param>
            /// <returns>成员收集器。</returns>
            public AutoMemberDetailsStore Add<TMember>(Expression<Func<T, TMember>> selector)
            {
                Expression body = selector.Body;
                if (body is UnaryExpression u && u.NodeType == ExpressionType.Convert) body = u.Operand;
                if (body is not MemberExpression m) throw new InvalidOperationException("必须是属性或字段");
                return Add(m.Member);
            }

            /// <summary>
            /// 添加成员信息。
            /// </summary>
            /// <param name="info">成员信息。</param>
            /// <returns>成员收集器。</returns>
            public AutoMemberDetailsStore Add(MemberInfo info)
            {
                var details = autoFormatter.CreateAutoMemberInfo(parameter, info);
                for (int i = 0; i < memberDetails.Count; i++)
                    if (memberDetails[i].Name == details.Name)
                        throw new Exception($"重复添加成员：{details.Name}");
                memberDetails.Add(details);
                return this;
            }
        }

        /// <summary>
        /// 自动成员明细。
        /// </summary>
        protected readonly struct AutoMemberDetails
        {
            /// <summary>
            /// 序列化缓冲区表达式。
            /// </summary>
            public Expression SerializeBuffer { get; }

            /// <summary>
            /// 序列化写入器表达式。
            /// </summary>
            public Expression SerializeSpan { get; }

            /// <summary>
            /// 反序列化缓冲区表达式。
            /// </summary>
            public MemberBinding DeserializeBuffer { get; }

            /// <summary>
            /// 反序列化读取器表达式。
            /// </summary>
            public MemberBinding DeserializeSpan { get; }

            /// <summary>
            /// 获取长度表达式。
            /// </summary>
            public Expression GetLength { get; }

            /// <summary>
            /// 成员名称。
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// 成员默认长度。
            /// </summary>
            public int DefaultLength { get; }

            /// <summary>
            /// 初始化成员明细。
            /// </summary>
            /// <param name="serBuf">序列化缓冲区表达式。</param>
            /// <param name="serSpan">序列化写入器表达式。</param>
            /// <param name="desBuf">反序列化缓冲区表达式。</param>
            /// <param name="desSpan">反序列化读取器表达式。</param>
            /// <param name="getLen">获取长度表达式。</param>
            /// <param name="name">成员名称。</param>
            /// <param name="defLen">成员默认长度。</param>
            public AutoMemberDetails(Expression serBuf, Expression serSpan, MemberBinding desBuf, MemberBinding desSpan, Expression getLen, string name, int defLen)
            {
                SerializeBuffer = serBuf;
                SerializeSpan = serSpan;
                DeserializeBuffer = desBuf;
                DeserializeSpan = desSpan;
                GetLength = getLen;
                Name = name;
                DefaultLength = defLen;
            }
        }
    }
}