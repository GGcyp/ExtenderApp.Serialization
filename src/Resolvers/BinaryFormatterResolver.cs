using System.Reflection;
using ExtenderApp.Serialization.Formatters;

namespace ExtenderApp.Serialization
{
    /// <summary>
    /// 二进制格式化器解析器：根据类型从 <see cref="BinaryFormatterStore" /> 创建或获取 <see cref="BinaryFormatter" /> 实例。
    /// </summary>
    public sealed class BinaryFormatterResolver
    {
        private readonly Dictionary<Type, BinaryFormatter> _formmaterDict;

        private readonly BinaryFormatterStore _store;

        private readonly BinaryFormatterCreator _formatCreator;

        /// <summary>
        /// 使用指定的格式化器存储创建解析器。
        /// </summary>
        /// <param name="store"> 格式化器注册表。 </param>
        public BinaryFormatterResolver(BinaryFormatterStore store)
        {
            _formmaterDict = new();
            _store = store;
            _formatCreator = new(store);
        }

        /// <summary>
        /// 获取指定类型的泛型格式化器。
        /// </summary>
        public BinaryFormatter<T> GetFormatter<T>()
        {
            return (BinaryFormatter<T>)GetFormatter(typeof(T));
        }

        /// <summary>
        /// 按运行时类型获取格式化器。
        /// </summary>
        public BinaryFormatter GetFormatter(Type type)
        {
            if (!_formmaterDict.TryGetValue(type, out var formatter))
            {
                lock (_formmaterDict)
                {
                    if (_formmaterDict.TryGetValue(type, out formatter))
                    {
                        return formatter;
                    }

                    BinaryFormatter? resolved = null;
                    var skipFinalAdd = false;

                    if (!_store.TryGetValue(type, out var details))
                    {
                        var formatterType = _formatCreator.CreatFormatter(type);
                        if (formatterType is null)
                            throw new InvalidOperationException($"未找到转换器类型：{type.FullName}.");

                        resolved = CreateFormatterInstance(formatterType, this);
                    }
                    else
                    {
                        if (details!.FormatterTypes.Count == 0)
                        {
                            throw new ArgumentNullException($"转换器类型为空：{type}");
                        }

                        if (details.IsVersionDataFormatter)
                        {
                            var managerType = typeof(VersionDataFormatterManager<>).MakeGenericType(details.BinaryType);
                            var managerObj = Activator.CreateInstance(managerType, this)
                                ?? throw new InvalidOperationException($"无法创建版本数据转换器管理器：{managerType.FullName}");
                            dynamic dynManager = managerObj;
                            for (int i = 0; i < details.FormatterTypes.Count; i++)
                            {
                                var vdFormatter = CreateFormatterInstance(details.FormatterTypes[i], this);
                                dynManager.AddFormatter(vdFormatter);
                            }

                            _formmaterDict.Add(details.VersionDataBinaryType!, (BinaryFormatter)managerObj);
                            var proxyType = typeof(VersionDataPayloadFormatterProxy<>).MakeGenericType(details.BinaryType);
                            var proxyObj = Activator.CreateInstance(proxyType, managerObj)
                                ?? throw new InvalidOperationException($"无法创建版本载荷代理格式化器：{proxyType.FullName}");
                            _formmaterDict.Add(details.BinaryType, (BinaryFormatter)proxyObj);

                            if (type == details.VersionDataBinaryType)
                                resolved = (BinaryFormatter)managerObj;
                            else if (type == details.BinaryType)
                                resolved = (BinaryFormatter)proxyObj;
                            else
                                throw new InvalidOperationException($"版本化注册与请求类型不一致：{type.FullName}");

                            skipFinalAdd = true;
                        }
                        else
                        {
                            var formatterType = details.FormatterTypes[0];
                            resolved = CreateFormatterInstance(formatterType, this);
                        }
                    }

                    formatter = resolved;
                    if (formatter == null)
                        throw new ArgumentNullException($"未找到：{type.FullName} 的格式转换器");

                    if (!skipFinalAdd)
                        _formmaterDict.Add(type, formatter);
                }
            }
            return formatter;
        }

        /// <summary>
        /// 使用公共构造函数创建格式化器：优先选用 <c> (BinaryFormatterResolver) </c>，否则使用无参构造函数。
        /// </summary>
        private static BinaryFormatter CreateFormatterInstance(Type formatterType, BinaryFormatterResolver resolver)
        {
            ConstructorInfo? ctor = formatterType.GetConstructor(new[] { typeof(BinaryFormatterResolver) });
            if (ctor is not null)
            {
                object? o = Activator.CreateInstance(formatterType, resolver);
                if (o is BinaryFormatter bf)
                    return bf;
                throw new InvalidOperationException($"格式化器实例类型无效：{formatterType.FullName}");
            }

            ctor = formatterType.GetConstructor(Type.EmptyTypes);
            if (ctor is not null)
            {
                object? o = Activator.CreateInstance(formatterType);
                if (o is BinaryFormatter bf)
                    return bf;
                throw new InvalidOperationException($"格式化器实例类型无效：{formatterType.FullName}");
            }

            throw new InvalidOperationException($"格式化器缺少可用的公共构造函数：{formatterType.FullName}");
        }
    }
}