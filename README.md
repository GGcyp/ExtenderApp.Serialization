# ExtenderApp.Serialization

> 是而精致序列化：一个面向 .NET 8 的高性能二进制序列化库，强调简洁 API、可扩展设计与稳定行为，适合在业务系统中统一对象与二进制数据的转换流程。

## 项目简介

`ExtenderApp.Serialization` 是一个专注于二进制序列化场景的类库，目标是在保证可读性与可维护性的前提下，提供稳定、清晰、便于集成的序列化能力。

## 适用场景

- 服务间高频数据传输
- 本地缓存对象持久化
- 对序列化性能和可控性有要求的业务模块

## 环境要求

- .NET SDK 8.0 或更高版本
- Windows / Linux / macOS（支持 .NET 8 的环境）

## 项目结构

- `src/ExtenderApp.Serialization.csproj`：核心序列化库
- `test/ExtenderApp.Serialization.Tests/`：单元测试项目
- `external/ExtenderApp.Buffer/`：依赖的缓冲区组件项目

## 快速开始

### 1) 克隆仓库

```bash
git clone https://github.com/<你的用户名>/<你的仓库名>.git
cd <你的仓库名>
```

### 2) 还原并构建

```bash
dotnet restore
dotnet build -c Release
```

### 3) 运行测试

```bash
dotnet test
```

## 在你的项目中使用

如果在同一解决方案中使用，可先添加项目引用：

```bash
dotnet add <你的业务项目>.csproj reference src/ExtenderApp.Serialization.csproj
```

然后在代码中按你公开的 API 进行序列化/反序列化调用。

### 在宿主仓库复用现有 Buffer 项目

如果你的宿主仓库已经维护了 `ExtenderApp.Buffer`（例如放在 `external/ExtenderApp.Buffer/`），可以让 `ExtenderApp.Serialization` 直接引用宿主仓库中的 Buffer 工程，而不是在当前子仓库内再维护一份。

建议做法：

- 在宿主解决方案中同时纳入 `ExtenderApp.Buffer` 与 `ExtenderApp.Serialization`
- 将 `ExtenderApp.Serialization.csproj` 中的 `ProjectReference` 调整为指向宿主仓库的 Buffer 路径
- 本地验证 `dotnet build` 与 `dotnet test` 后再决定是否需要提交该路径调整

该方式适合多仓库聚合场景，可避免重复维护 Buffer 依赖目录。

## 开发与提交建议

- 提交前建议执行：
  - `dotnet build`
  - `dotnet test`
- 请确保测试临时文件（如 `bin/`、`obj/`、`TestResults/`）不会进入版本控制

## 许可证

如需开源发布，请在仓库根目录添加 `LICENSE` 文件，并在此处标注许可证类型（例如 MIT）。

