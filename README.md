# 智能题库管理系统

## 项目概述

这是一个现代化的智能题库管理系统，基于 Vue 3 + TypeScript 前端和 .NET 8 后端开发。系统支持多种题型管理、在线考试组织、AI 辅助出题等功能，为教育教学提供了数字化解决方案。

## 📋 功能特点

### 题库管理

- ✅ 支持多种题型：单选题、判断题、填空题、编程题、简答题等
- ✅ 题目分类管理和高级搜索
- ✅ 题目批量导入导出（支持 Word、Excel 格式）
- ✅ 支持代码高亮显示和 Markdown 格式

### AI 智能辅助

- ✅ 集成 DeepSeek AI API，实现智能出题
- ✅ 题目质量评估
- ✅ 题目生成与智能推荐

### 考试管理

- 🔄 在线组卷和发布考试
- 🔄 学生在线答题系统
- 🔄 自动评分和成绩统计
- 🔄 考试时间

### 用户管理

- ✅ 基于 JWT 的安全认证体系
- ✅ 角色权限控制（管理员/教师/学生）
- ✅ 用户信息和权限管理
- ✅ 批量导入学生账户

## 🛠️ 技术栈

### 前端

- **核心框架**: Vue 3 + TypeScript
- **状态管理**: Pinia
- **路由管理**: Vue Router 4
- **UI组件库**: Element Plus
- **构建工具**: Vite 6
- **HTTP客户端**: Axios
- **文档处理**: DocxTemplater, XLSX
- **代码高亮**: Highlight.js
- **Markdown渲染**: Marked + DOMPurify

### 后端

- **框架**: .NET 8 WebAPI
- **ORM**: Entity Framework Core 8
- **认证**: JWT Bearer Authentication
- **加密**: BCrypt.NET
- **数据库**: Microsoft SQL Server / MySQL (使用 Pomelo.EntityFrameworkCore.MySql)
- **API文档**: Swagger / OpenAPI
- **依赖注入**: 内置 .NET 依赖注入容器

### AI 集成

- **API集成**: DeepSeek API
- **智能对话**: 基于大型语言模型
- **题目生成**: 提示工程和上下文控制

## 🚀 快速开始

### 环境要求

- **前端**: Node.js 18+
- **后端**: .NET SDK 8.0+
- **数据库**: MySQL 8.0+ 或 SQL Server
- **IDE**: Visual Studio Code 或 Visual Studio 2022

### 安装步骤

#### 1. 克隆项目

```bash
git clone https://github.com/你的用户名/2306053105.git
cd 2306053105
```

#### 2. 前端设置

```bash
# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

#### 3. 后端设置

```bash
cd QuestionBankApi

# 还原 NuGet 包
dotnet restore

# 更新数据库 (确保已正确配置数据库连接字符串)
dotnet ef database update

# 运行项目
dotnet run
```

#### 4. 配置 DeepSeek API (可选)

在 `QuestionBankApi/appsettings.json` 中配置：

```json
{
  "DeepSeek": {
    "ApiKey": "你的API密钥",
    "Model": "deepseek-chat"
  }
}
```

## 📁 项目结构

```
2306053105/
├─src/                    # 前端源码
│  ├─assets/             # 静态资源
│  ├─components/         # Vue组件
│  ├─router/             # 路由定义
│  ├─services/           # API服务
│  ├─stores/             # Pinia状态管理
│  ├─types/              # TypeScript类型定义
│  ├─utils/              # 工具函数
│  └─views/              # 页面组件
├─public/                # 静态资源
│  └─templates/          # 导出模板
└─QuestionBankApi/       # 后端源码
   ├─Controllers/        # API控制器
   ├─Data/               # 数据访问层
   ├─DeepSeek/           # AI集成服务
   ├─DTOs/               # 数据传输对象
   ├─LoginSystemApi/     # 用户认证系统
   ├─Migrations/         # EF Core迁移
   ├─Models/             # 数据模型
   └─Services/           # 业务逻辑服务
```

## 📦 部署指南

### 前端部署

1. 构建生产版本:

```bash
npm run build
```

2. 将 `dist` 目录部署到 Web 服务器 (Nginx, Apache, IIS等)

### 后端部署

```bash
cd QuestionBankApi
dotnet publish -c Release
```

部署到 IIS, Azure App Service, Docker容器或其他支持 .NET 的托管服务

## 📈 未来计划

- 完善考试系统功能
- 增加数据分析和可视化模块
- 优化AI出题算法
- 开发移动端应用
- 添加更多题型支持
- 集成第三方学习管理系统

## 📋 更新日志

### 2025-04-29

- 优化题库管理界面
- 改进用户体验
- 修复已知问题

### 2025-04-18

- 完成题库管理基本功能
- 集成 DeepSeek API
- 实现用户认证和权限控制

## 前端不要学我，这个项目准备弃坑了，第一次写项目规范差

## 前端不要学我，这个项目准备弃坑了，第一次写项目规范差

## 前端不要学我，这个项目准备弃坑了，第一次写项目规范差

## 后端自认为还勉强像人类