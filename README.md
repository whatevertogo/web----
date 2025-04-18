# 题库管理系统

这是一个现代化的在线题库管理系统，基于 Vue 3 + TypeScript 前端和 .NET 8 后端开发。系统支持多种题型管理、在线考试、AI 辅助出题等功能。

## 功能特点

### 题库管理

- 支持单选题、判断题、填空题、编程题、简答题等多种题型
- 题目导入导出（支持 Word 文档格式）
- 题目分类管理和搜索
- AI 辅助（基于 DeepSeek API）

### 考试系统(未完成)

- 在线组卷和发布考试
- 学生在线答题
- 自动评分和成绩统计
- 考试时间管理

### 用户管理

- 基于 JWT 的用户认证
- 角色权限控制（管理员/教师/学生）
- 用户信息管理

## 技术栈

### 前端

- Vue 3 + TypeScript
- Vue Router 管理路由
- Pinia 状态管理
- Element Plus UI 组件库
- Vite 构建工具

### 后端

- .NET 8 WebAPI
- Entity Framework Core ORM
- JWT Bearer 认证
- BCrypt.NET 密码加密
- MySQL/SQL Server 数据库
- Swagger API 文档

### AI 集成

- DeepSeek API 集成
- 支持多种 AI 模型
- 智能对话和题目生成

## 快速开始

### 环境要求

- Node.js 18+
- .NET SDK 8.0+
- MySQL 8.0+ 或 SQL Server
- Visual Studio Code 或 Visual Studio 2022

### 安装步骤

1. 克隆项目
\`\`\`bash
git clone [项目地址]
cd 2306053105
\`\`\`

2. 前端设置
\`\`\`bash

# 安装依赖

npm install

# 启动开发服务器

npm run dev
\`\`\`

3. 后端设置
\`\`\`bash
cd QuestionBankApi

# 还原 NuGet 包

dotnet restore

# 更新数据库

dotnet ef database update

# 运行项目

dotnet run
\`\`\`

4. 配置 DeepSeek API（可选）

- 在 \`appsettings.json\` 中设置你的 DeepSeek API 密钥

## 项目结构

\`\`\`
├── src/                   # 前端源码
│   ├── assets/           # 静态资源
│   ├── components/       # 组件
│   ├── services/         # API 服务
│   ├── stores/          # Pinia 状态管理
│   ├── views/           # 页面组件
│   └── ...
├── QuestionBankApi/      # 后端源码
│   ├── Controllers/     # API 控制器
│   ├── Models/         # 数据模型
│   ├── Services/       # 业务服务
│   ├── DeepSeek/       # AI 集成服务
│   └── ...
\`\`\`

## 部署

### 前端部署

1. 构建生产版本
\`\`\`bash
npm run build
\`\`\`
2. 将 \`dist\` 目录部署到 Web 服务器

### 后端部署

\`\`\`bash
cd QuestionBankApi
dotnet publish -c Release
\`\`\`


## 许可证

该项目使用 MIT 许可证。

## 联系方式

如有问题或建议，请通过以下方式联系：

- Email: [1879483647@qq.com]
- GitHub Issues: [项目 Issues 链接]
