# FlowCore

ระบบจัดการ Workflow และอนุมัติคำขอ (Workflow & Approval System) สร้างด้วย .NET 10 + Vue 3

---

## สารบัญ

- [ภาพรวม](#ภาพรวม)
- [Tech Stack](#tech-stack)
- [สถาปัตยกรรม](#สถาปัตยกรรม)
- [ฟีเจอร์หลัก](#ฟีเจอร์หลัก)
- [โครงสร้างโปรเจกต์](#โครงสร้างโปรเจกต์)
- [การติดตั้งและรันระบบ](#การติดตั้งและรันระบบ)
  - [วิธีที่ 1: Docker Compose (แนะนำ)](#วิธีที่-1-docker-compose-แนะนำ)
  - [วิธีที่ 2: Local Development](#วิธีที่-2-local-development)
- [Environment Variables](#environment-variables)
- [API Endpoints](#api-endpoints)
- [บัญชีผู้ใช้เริ่มต้น](#บัญชีผู้ใช้เริ่มต้น)
- [Dark Mode](#dark-mode)

---

## ภาพรวม

FlowCore เป็นระบบ Internal Approval ที่รองรับการสร้าง Workflow หลายขั้นตอน ผู้ใช้สามารถยื่นคำขออนุมัติ แนบไฟล์ และติดตามสถานะได้แบบ Real-time ผู้อนุมัติจะได้รับการแจ้งเตือนทางอีเมล และสามารถอนุมัติ/ปฏิเสธ/ส่งกลับแก้ไขได้จากระบบ

---

## Tech Stack

### Backend
| ส่วน | เทคโนโลยี |
|---|---|
| Framework | .NET 10 / ASP.NET Core |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL 16 |
| Authentication | JWT Bearer + Refresh Token Rotation |
| Password Hashing | BCrypt.Net |
| Email | System.Net.Mail (SMTP) |
| Logging | Serilog |
| API Docs | Swagger / OpenAPI |

### Frontend
| ส่วน | เทคโนโลยี |
|---|---|
| Framework | Vue 3 (Composition API) |
| State Management | Pinia |
| Router | Vue Router 4 |
| HTTP Client | Axios |
| CSS | Tailwind CSS v4 |
| Build Tool | Vite |
| Language | TypeScript |

### Infrastructure
| ส่วน | เทคโนโลยี |
|---|---|
| Containerization | Docker + Docker Compose |
| File Storage | Local disk (Docker named volume) |

---

## สถาปัตยกรรม

โปรเจกต์ Backend ใช้ **Clean Architecture** แบ่งเป็น 4 layer:

```
FlowCore/
├── src/
│   ├── FlowCore.Domain/          # Entities, Enums, Domain interfaces
│   ├── FlowCore.Application/     # Use cases, DTOs, Service interfaces
│   ├── FlowCore.Infrastructure/  # EF Core, Repositories, SMTP, Seeder
│   └── FlowCore.API/             # Controllers, Middleware, Program.cs
└── frontend/                     # Vue 3 SPA
```

**Dependency rule:** `API → Application → Domain` (Infrastructure implements Application interfaces)

### Domain Model

```
User ─── Role
 │
 └── Department

Request ─── RequestType
  │
  ├── Approval[] ─── WorkflowStep ─── Workflow
  └── Attachment[]
```

### Enum ค่าสำคัญ

| Enum | ค่า |
|---|---|
| `RequestStatus` | `Draft=0`, `Pending=1`, `Approved=2`, `Rejected=3`, `Returned=4` |
| `ApprovalAction` | `Approve=1`, `Reject=2`, `Return=3` |
| `ApproverType` | `DepartmentManager`, `Role`, `User` |

---

## ฟีเจอร์หลัก

### คำขออนุมัติ (Requests)
- สร้างคำขอในสถานะ **Draft** และแก้ไขก่อนส่ง
- ส่งคำขอเพื่อเริ่ม Approval Workflow
- แนบไฟล์ (PDF, Word, Excel, รูปภาพ, ZIP, Text) สูงสุด 10 MB/ไฟล์
- ดาวน์โหลดไฟล์แนบ
- ดูประวัติการอนุมัติทุกขั้นตอน
- กรองรายการด้วย: ชื่อ, ประเภท, สถานะ, ช่วงวันที่

### การอนุมัติ (Approvals)
- ผู้อนุมัติเห็นเฉพาะคำขอที่ถึงขั้นตอนของตน
- **อนุมัติ / ปฏิเสธ / ส่งกลับแก้ไข** พร้อมระบุเหตุผล
- อนุมัติเร็วจาก Dashboard โดยตรง
- กรองรายการเหมือน Requests

### Workflow
- Admin กำหนด Workflow ได้หลายขั้นตอน
- รองรับ Approver 3 ประเภท:
  - **DepartmentManager** — หัวหน้าแผนกของผู้ยื่นคำขอ
  - **Role** — ผู้ใช้ที่มี Role ที่กำหนด
  - **User** — ระบุ User ID โดยตรง

### User & Department Management (Admin)
- CRUD ผู้ใช้งาน: สร้าง, แก้ไข, เปิด/ปิดใช้งาน, รีเซ็ตรหัสผ่าน
- CRUD แผนก: กำหนดหัวหน้าแผนก
- Role-based Access: `Admin`, `User`, `Approver`

### Authentication
- JWT Access Token (default 60 นาที)
- Refresh Token rotation — เก็บใน DB, revoke อัตโนมัติหลังใช้งาน
- Axios interceptor ต่ออายุ token อัตโนมัติเมื่อหมดอายุ
- Rate limiting บน `/auth/login`

### Email Notification
- แจ้งเตือนผู้ยื่นคำขอเมื่อถูก **อนุมัติ / ปฏิเสธ / ส่งกลับ**
- ตั้งค่าผ่าน SMTP (ปิดได้ด้วย `Smtp__Enabled=false`)
- ไม่กระทบ Approval flow หาก SMTP ล้มเหลว

### Dark Mode
- Toggle ได้จาก Sidebar (ปุ่มรูปดวงจันทร์/ดวงอาทิตย์)
- บันทึก preference ใน `localStorage`

---

## โครงสร้างโปรเจกต์

```
FlowCore/
├── src/
│   ├── FlowCore.Domain/
│   │   └── Entities/
│   │       ├── User.cs, Role.cs, Department.cs
│   │       ├── Request.cs, RequestType.cs
│   │       ├── Workflow.cs, WorkflowStep.cs
│   │       ├── Approval.cs, Attachment.cs
│   │       ├── Comment.cs, RefreshToken.cs
│   │       └── Enums/ (RequestStatus, ApprovalAction, ApproverType)
│   │
│   ├── FlowCore.Application/
│   │   ├── DTOs/         (Auth, Request, Approval, User, Department)
│   │   ├── Interfaces/   (IAuthService, IRequestService, IApprovalService, ...)
│   │   └── Services/     (AuthService, RequestService, ApprovalService, ...)
│   │
│   ├── FlowCore.Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── DbSeeder.cs
│   │   │   ├── Migrations/
│   │   │   └── Repository.cs
│   │   └── Services/
│   │       ├── SmtpNotificationService.cs
│   │       └── SmtpSettings.cs
│   │
│   └── FlowCore.API/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── RequestController.cs
│       │   ├── ApprovalController.cs
│       │   ├── AttachmentController.cs
│       │   ├── WorkflowController.cs
│       │   ├── RequestTypeController.cs
│       │   ├── UserController.cs
│       │   ├── DepartmentController.cs
│       │   └── RoleController.cs
│       ├── appsettings.json
│       └── Program.cs
│
├── frontend/
│   └── src/
│       ├── api/          (client.ts, auth.ts)
│       ├── stores/       (auth.ts, theme.ts)
│       ├── router/       (index.ts)
│       ├── layouts/      (AppLayout.vue)
│       └── views/
│           ├── LoginView.vue
│           ├── DashboardView.vue
│           ├── ApprovalsView.vue
│           ├── WorkflowsView.vue
│           ├── requests/
│           │   ├── RequestListView.vue
│           │   └── RequestDetailView.vue
│           └── admin/
│               ├── UsersView.vue
│               └── DepartmentsView.vue
│
├── docker-compose.yml
├── Dockerfile
├── .env.example
└── FlowCore.slnx
```

---

## การติดตั้งและรันระบบ

### ข้อกำหนดเบื้องต้น
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (สำหรับ Docker Compose)
- หรือ [.NET 10 SDK](https://dotnet.microsoft.com/download) + [Node.js 20+](https://nodejs.org/) + PostgreSQL 16 (สำหรับ Local Dev)

---

### วิธีที่ 1: Docker Compose (แนะนำ)

```bash
# 1. Clone โปรเจกต์
git clone https://github.com/TaLue/FlowCore.git
cd FlowCore

# 2. สร้างไฟล์ .env จาก template
cp .env.example .env
# แก้ไข .env ตามต้องการ (อย่าลืมเปลี่ยน JWT_KEY และ POSTGRES_PASSWORD)

# 3. Build และรัน
docker compose up -d --build

# 4. ดู log
docker compose logs -f api
```

ระบบจะพร้อมใช้งานที่:
| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| PostgreSQL | localhost:5432 |

> ข้อมูลเริ่มต้น (seed) จะถูกสร้างอัตโนมัติเมื่อรันครั้งแรก

**หยุดระบบ:**
```bash
docker compose down
# หยุดและลบข้อมูลทั้งหมด (รวม DB):
docker compose down -v
```

**Rebuild หลังแก้โค้ด:**
```bash
docker compose up -d --build api
```

---

### วิธีที่ 2: Local Development

#### Backend

```bash
# 1. ตั้งค่า connection string ใน src/FlowCore.API/appsettings.json
#    หรือใช้ dotnet user-secrets

# 2. Run migrations
dotnet ef database update --project src/FlowCore.Infrastructure --startup-project src/FlowCore.API

# 3. รัน API
dotnet run --project src/FlowCore.API
# API จะรันที่ http://localhost:8080
```

#### Frontend

```bash
cd frontend

# ติดตั้ง dependencies
npm install

# รัน dev server (proxy /api → http://localhost:8080)
npm run dev
# Frontend จะรันที่ http://localhost:5173

# Build สำหรับ production
npm run build
```

---

## Environment Variables

คัดลอก `.env.example` เป็น `.env` และแก้ไขค่าต่างๆ:

```env
# PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_strong_password   # *** เปลี่ยนในทุก environment ***
POSTGRES_DB=flowcore

# JWT
JWT_KEY=your_random_secret_min_32_chars  # *** เปลี่ยนในทุก environment ***
JWT_ISSUER=FlowCore
JWT_EXPIRY_MINUTES=60
# JWT_REFRESH_EXPIRY_DAYS=7              # default 7 วัน

# CORS
CORS_ORIGIN_0=http://localhost:5173      # URL ของ Frontend

# SMTP Email (optional)
Smtp__Enabled=false                      # เปลี่ยนเป็น true เพื่อส่งอีเมลจริง
Smtp__Host=smtp.gmail.com
Smtp__Port=587
Smtp__UseSsl=true
Smtp__Username=your_email@gmail.com
Smtp__Password=your_app_password         # Gmail: ใช้ App Password
Smtp__FromAddress=noreply@flowcore.local
Smtp__FromName=FlowCore
```

> **หมายเหตุ Gmail:** ต้องเปิด 2FA และสร้าง [App Password](https://myaccount.google.com/apppasswords) แยก

---

## API Endpoints

### Authentication
| Method | Path | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/login` | เข้าสู่ระบบ | - |
| POST | `/api/auth/refresh` | ต่ออายุ token | - |

### Requests
| Method | Path | Description | Auth |
|---|---|---|---|
| GET | `/api/requests` | รายการคำขอของตัวเอง (Admin เห็นทั้งหมด) | ✓ |
| POST | `/api/requests` | สร้างคำขอใหม่ (Draft) | ✓ |
| GET | `/api/requests/{id}` | รายละเอียดคำขอ | ✓ |
| POST | `/api/requests/{id}/submit` | ส่งคำขอเข้า Workflow | ✓ |

### Approvals
| Method | Path | Description | Auth |
|---|---|---|---|
| GET | `/api/approvals/pending` | รายการรออนุมัติ | ✓ |
| POST | `/api/approvals/{id}/approve` | อนุมัติ | ✓ |
| POST | `/api/approvals/{id}/reject` | ปฏิเสธ | ✓ |
| POST | `/api/approvals/{id}/return` | ส่งกลับแก้ไข | ✓ |

### Attachments
| Method | Path | Description | Auth |
|---|---|---|---|
| GET | `/api/requests/{id}/attachments` | รายการไฟล์แนบ | ✓ |
| POST | `/api/requests/{id}/attachments` | อัปโหลดไฟล์ (max 10MB) | ✓ |
| GET | `/api/requests/{id}/attachments/{attId}/download` | ดาวน์โหลดไฟล์ | ✓ |
| DELETE | `/api/requests/{id}/attachments/{attId}` | ลบไฟล์ | ✓ |

### Workflows
| Method | Path | Description | Auth |
|---|---|---|---|
| GET | `/api/workflows` | รายการ Workflow | ✓ |
| POST | `/api/workflows` | สร้าง Workflow | Admin |
| PUT | `/api/workflows/{id}` | แก้ไข Workflow | Admin |

### Admin
| Method | Path | Description | Auth |
|---|---|---|---|
| GET | `/api/users` | รายการผู้ใช้ | Admin |
| POST | `/api/users` | สร้างผู้ใช้ | Admin |
| PUT | `/api/users/{id}` | แก้ไขผู้ใช้ | Admin |
| GET | `/api/departments` | รายการแผนก | ✓ |
| POST | `/api/departments` | สร้างแผนก | Admin |
| PUT | `/api/departments/{id}` | แก้ไขแผนก | Admin |
| GET | `/api/roles` | รายการ Role | ✓ |
| GET | `/api/request-types` | ประเภทคำขอ | ✓ |

> ดู Swagger UI ที่ `/swagger` สำหรับ schema และ request/response ครบถ้วน

---

## บัญชีผู้ใช้เริ่มต้น

บัญชีเหล่านี้ถูกสร้างอัตโนมัติเมื่อรันครั้งแรก (`DbSeeder.cs`):

| Username | Password | Role | แผนก |
|---|---|---|---|
| `admin` | `Admin@1234` | Admin | Engineering |
| `approver1` | `Approver@1234` | Approver | Engineering |
| `user1` | `User@1234` | User | Engineering |
| `user2` | `User@1234` | User | HR |

> **ควรเปลี่ยนรหัสผ่านทันทีหลัง deploy ไปยัง production**

---

## Dark Mode

ระบบรองรับ Dark Mode แบบ class-based ผ่าน Tailwind CSS v4:

- กด **ปุ่มดวงจันทร์/ดวงอาทิตย์** ใน sidebar ด้านล่างเพื่อสลับโหมด
- preference ถูกบันทึกใน `localStorage` — reload แล้วยังคง theme เดิม
- implementation: `stores/theme.ts` เพิ่ม/ลบ class `.dark` บน `<html>` element
