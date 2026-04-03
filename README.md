# 💎 Yash Gems Identity Provider - Auth & eKYC Platform

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Next.js](https://img.shields.io/badge/Next.js-14-black?style=flat-square&logo=nextdotjs&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=flat-square&logo=typescript&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-8.0-00758F?style=flat-square&logo=mysql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-7.0-DC382D?style=flat-square&logo=redis&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.3-06B6D4?style=flat-square&logo=tailwindcss&logoColor=white)

![JWT](https://img.shields.io/badge/JWT-Auth-FF6B6B?style=flat-square)
![OAuth2](https://img.shields.io/badge/OAuth2-Google-4285F4?style=flat-square)
![RBAC](https://img.shields.io/badge/RBAC-Security-27AE60?style=flat-square)
![eKYC](https://img.shields.io/badge/eKYC-Face_Detection-9B59B6?style=flat-square)
![Clean Architecture](https://img.shields.io/badge/Clean_Architecture-Design-3498DB?style=flat-square)

</div>

---

Nền tảng xác thực và định danh điện tử (eKYC) chuyên biệt cho hệ thống trang sức cao cấp Yash Gems & Jewelleries. Dự án tập trung vào tính bảo mật tuyệt đối cho các giao dịch giá trị cao, kết hợp giữa quy trình định danh hiện đại và kiến trúc phần mềm chuẩn doanh nghiệp.

---

## 🚀 Công Nghệ Sử Dụng

### Backend (.NET 8)

- **Clean Architecture**: Chia tách 4 tầng (Domain, Application, Infrastructure, Api) giúp hệ thống dễ mở rộng và bảo trì
- **Identity & Security**: JWT Authentication, Refresh Token Rotation, RBAC (Role-Based Access Control)
- **Database**: MySQL với Entity Framework Core (Code First)
- **Caching**: Redis (Upstash) xử lý Rate Limiting và Session
- **Service Integration**:
  - Cloudinary: Lưu trữ ảnh CCCD/Face
  - Resend: Gửi OTP Email
  - SendGrid/Twilio: Gửi SMS

### Frontend (Next.js 14)

- **App Router**: Tối ưu hóa SEO và hiệu năng truyền tải
- **Tailwind CSS**: Thiết kế giao diện hiện đại, responsive
- **eKYC Engine**: Sử dụng thư viện face-api.js để thực hiện quét khuôn mặt và so khớp liveness ngay tại trình duyệt (Client-side)

---

## 🔐 Tính Năng Cốt Lõi

### 1. Hệ Thống Xác Thực Hoàn Chỉnh

#### Luồng Đăng ký & Đăng nhập
- Hỗ trợ mật khẩu băm BCrypt
- Quản lý trạng thái tài khoản (Verified, Blocked)

#### Google OAuth2
- Tích hợp đăng nhập nhanh qua Google

#### Refresh Token Rotation
- Cơ chế xoay vòng token an toàn
- Lưu Refresh Token trong HttpOnly Cookie để chống tấn công XSS

#### Xác thực OTP
- Tự động gửi mã xác thực qua Email
- Kích hoạt tài khoản chính thức sau xác thực

#### Quản lý mật khẩu
- Chức năng Quên mật khẩu
- Chức năng Đổi mật khẩu bảo mật

### 2. Định Danh Điện Tử (eKYC)

Lớp bảo mật **bắt buộc** đối với các đơn hàng trang sức có giá trị cao (>5 triệu VNĐ).

#### Upload CCCD/Passport
- Hỗ trợ tải lên ảnh 2 mặt định danh
- Lưu trữ an toàn trên Cloudinary

#### Face Liveness Detection
- Quét khuôn mặt người dùng thời gian thực
- Yêu cầu thực hiện các hành động:
  - Nhìn thẳng
  - Quay trái/phải
  - Chớp mắt
- Đảm bảo người thật đang thực hiện xác thực

#### Face Matching
- Thuật toán tính toán khoảng cách Euclidean
- So khớp khuôn mặt người dùng với ảnh trên giấy từ CCCD
- Độ chính xác cao

#### KYC Lifecycle
- Quản lý trạng thái định danh: PENDING → VERIFIED → REJECTED
- Hỗ trợ Admin phê duyệt thủ công

### 3. Quản Lý Tài Khoản & Bảo Mật

#### User Profile
- Cập nhật thông tin cá nhân
- Quản lý ảnh đại diện
- Quản lý nhiều địa chỉ giao hàng

#### Rate Limiting
- Chống tấn công Brute-force
- Áp dụng cho các endpoint nhạy cảm (Login, Register)

#### Audit Logs
- Ghi lại lịch sử các hành động quan trọng
- Đổi mật khẩu
- Cập nhật KYC

---

## 🏗️ Cấu Trúc Dự Án (Clean Architecture)

### Backend Structure

```
/server (ASP.NET Core Solution)
├── YashGems.Identity.Domain
│   └── Entities, Enums, Interfaces core
│
├── YashGems.Identity.Application
│   └── Use Cases, DTOs, Business Logic
│
├── YashGems.Identity.Infrastructure
│   └── DB Context, Repositories, External Services
│
└── YashGems.Identity.Api
    └── Controllers, Middlewares, Configurations
```

### Frontend Structure

```
/client (Next.js Application)
├── /app
│   └── App Router (Pages, Layouts)
│
├── /components
│   └── UI Components (Face Scan, Auth Form)
│
├── /services
│   └── API Interaction (Axios, Auth Context)
│
└── /public
    └── face-api.js AI Models
```

---

## 🛠️ Cài Đặt & Chạy Thử

### Backend Setup

1. Cập nhật Connection String trong `appsettings.json`
2. Chạy migration database:
   ```bash
   dotnet ef database update
   ```
3. Khởi chạy server:
   ```bash
   dotnet run --project YashGems.Identity.Api
   ```

### Frontend Setup

1. Cài đặt dependencies:
   ```bash
   npm install
   # hoặc
   bun install
   ```

2. Cấu hình file `.env` với các biến:
   - `NEXT_PUBLIC_API_URL`: URL Backend
   - `NEXT_PUBLIC_CLOUDINARY_KEY`: API Key Cloudinary
   - `NEXT_PUBLIC_RESEND_KEY`: API Key Resend
   - Các biến khác liên quan đến integration

3. Chạy development server:
   ```bash
   npm run dev
   ```

---

## 👨‍💻 Thông Tin Tác Giả

| Thông tin | Chi tiết |
|-----------|---------|
| **Họ và tên** | Mai Trung Hậu |
| **Công ty** | MST Software |
| **Vị trí** | Fullstack Developer |
| **Email** | chunhau.py@gmail.com<br/>trunghau@mstsoftware.vn |
| **GitHub** | @maailtunghau |

---

## 📝 Ghi Chú

Dự án được phát triển với mục tiêu xây dựng một nền tảng định danh tin cậy cho ngành thương mại điện tử giá trị cao, đảm bảo an toàn tuyệt đối cho cả người mua và người bán.

---

© 2026 Yash Gems Identity Provider. Developed for Advanced Jewellery E-commerce Research.