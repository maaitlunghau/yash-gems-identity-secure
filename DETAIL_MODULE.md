# 🔬 PHÂN TÍCH SÂU TỪNG MODULE

### + Đề xuất API miễn phí cho môi trường test/học

---

## 🌐 BẢN ĐỒ API BÊN THỨ 3 - FREE TIER

Trước khi đi vào từng module, tổng hợp hết các API cần dùng và lựa chọn miễn phí:

| Nhu cầu                   | API đề xuất                                                     | Free tier                  | Ghi chú                               |
| ------------------------- | --------------------------------------------------------------- | -------------------------- | ------------------------------------- |
| **Giá vàng**              | [GoldAPI.io](https://www.goldapi.io)                            | 100 req/tháng              | Hoặc dùng backup dưới                 |
| **Giá vàng (backup)**     | Tự scrape SJC/DOJI                                              | Miễn phí                   | Dùng HtmlAgilityPack trong .NET       |
| **Payment**               | [Stripe Test Mode](https://stripe.com)                          | Hoàn toàn miễn phí         | Card test `4242 4242 4242 4242`       |
| **Payment local**         | [PayOS](https://payos.vn)                                       | Sandbox miễn phí           | Của người Việt, dễ tích hợp           |
| **Email**                 | [Resend.com](https://resend.com)                                | 3,000 email/tháng free     | SDK .NET + Next.js cực ngon           |
| **SMS OTP**               | [Twilio](https://twilio.com)                                    | $15 credit free khi signup | Đủ để test hàng trăm OTP              |
| **Face Detection / eKYC** | [face-api.js](https://github.com/justadudewhohacks/face-api.js) | Hoàn toàn miễn phí         | Chạy client-side, không cần server    |
| **Vận chuyển**            | [GHN Sandbox](https://dev.ghn.vn)                               | Miễn phí                   | Có sandbox env đầy đủ                 |
| **Vận chuyển (backup)**   | [GHTK API](https://khachhang.giaohangtietkiem.vn)               | Miễn phí sandbox           | Cần đăng ký tài khoản                 |
| **File Storage**          | [Cloudinary](https://cloudinary.com)                            | 25GB free                  | Upload ảnh sản phẩm, CCCD             |
| **Distributed Lock**      | Redis via [Upstash](https://upstash.com)                        | 10,000 req/ngày free       | Serverless Redis, không cần self-host |
| **Background Job**        | Hangfire (self-host)                                            | Hoàn toàn miễn phí         | Tích hợp thẳng .NET 8                 |
| **Logging**               | [Seq](https://datalust.co/seq)                                  | Free cho dev local         | Hoặc dùng Grafana + Loki              |
| **Swagger/Docs**          | Swashbuckle .NET                                                | Hoàn toàn miễn phí         | Built-in .NET 8                       |

---

## 🔐 MODULE 1 - XÁC THỰC & eKYC (PHÂN TÍCH SÂU)

### 1.1 Auth Flow chi tiết

```
ĐĂNG KÝ:
POST /api/v1/auth/register
  → Validate input (email format, phone VN format, password strength)
  → Hash password (BCrypt, cost factor 12)
  → Tạo user trạng thái: UNVERIFIED
  → Gửi email OTP (6 chữ số, TTL 10 phút) qua Resend
  → Return: { message: "OTP sent" }

POST /api/v1/auth/verify-email
  → Verify OTP
  → User trạng thái → VERIFIED (nhưng chưa KYC)
  → Return: JWT access token (15 phút) + Refresh token (7 ngày, httpOnly cookie)

ĐĂNG NHẬP:
POST /api/v1/auth/login
  → Verify credentials
  → Check user status (VERIFIED / BLOCKED / BANNED)
  → Return: access token + refresh token

REFRESH TOKEN:
POST /api/v1/auth/refresh
  → Lấy refresh token từ httpOnly cookie
  → Rotate refresh token (old token bị invalidate)
  → Return: access token mới
```

**Lý do dùng httpOnly cookie cho refresh token thay vì localStorage**: Tránh XSS attack lấy cắp token.

**JWT Payload nên chứa:**

```json
{
    "sub": "user_id",
    "role": "CUSTOMER | VENDOR_L1 | VENDOR_L2 | VENDOR_L3 | ADMIN | SUPER_ADMIN",
    "kyc_status": "NONE | PENDING | VERIFIED | REJECTED",
    "vendor_id": "nullable",
    "parent_vendor_id": "nullable",
    "iat": 1234567890,
    "exp": 1234568790
}
```

**Guards trong .NET 8 cần implement:**

- `[Authorize(Roles = "ADMIN, SUPER_ADMIN")]`
- `[RequireKYC]` - custom attribute, check `kyc_status == VERIFIED` trong JWT
- `[RequireVendorLevel(1)]` - check vendor tier

---

### 1.2 eKYC với face-api.js - Thiết kế chi tiết

**Tại sao chọn face-api.js thay vì service trả phí?**

- Chạy hoàn toàn trên browser (WebGL), không gửi ảnh lên server bên thứ 3
- Model size nhỏ (~6MB), load 1 lần rồi cache
- Đủ accurate cho môi trường học

**Flow eKYC step by step:**

```
BƯỚC 1 - Upload CCCD 2 mặt:
- Frontend: Cloudinary upload widget
- Validate: file type (jpg/png/webp), max 5MB
- Backend: lưu path vào DB, trạng thái KYC = PENDING_FACE_SCAN
- KHÔNG xử lý OCR (quá phức tạp cho đồ án - nói rõ trong report)

BƯỚC 2 - Face Liveness Detection (face-api.js):
- Load models: face_detection + face_landmark + face_recognition
- Yêu cầu người dùng:
    ✓ Nhìn thẳng 3 giây
    ✓ Quay trái
    ✓ Quay phải
    ✓ Chớp mắt
- Liveness check: detect xem có phải ảnh tĩnh hay người thật (Anti-spoofing cơ bản)

BƯỚC 3 - So khớp khuôn mặt với ảnh CCCD:
- Extract face descriptor từ ảnh CCCD (1 lần)
- Extract face descriptor từ webcam (realtime)
- Tính Euclidean distance giữa 2 descriptor
- Threshold: distance < 0.6 → MATCH (tùy chỉnh theo test)

BƯỚC 4 - Submit kết quả lên backend:
POST /api/v1/kyc/submit
{
  "cccd_front_url": "...",
  "cccd_back_url": "...",
  "face_match_score": 0.42,
  "liveness_passed": true
}
→ Backend lưu, KYC status = VERIFIED (auto nếu score ok) hoặc cần Admin review
```

**Database tables bổ sung cho KYC:**

```sql
KYCVerification:
  - user_id (FK)
  - cccd_front_url
  - cccd_back_url
  - face_match_score (decimal)
  - liveness_passed (bool)
  - status: PENDING | VERIFIED | REJECTED | MANUAL_REVIEW
  - reviewed_by (Admin user_id, nullable)
  - reviewed_at
  - reject_reason (nullable)
  - created_at
```

**Rule áp dụng KYC:**

- Đơn hàng < 5 triệu: Không cần KYC, chỉ cần verify email/SĐT
- Đơn hàng 5-20 triệu: Cần KYC VERIFIED
- Đơn hàng > 20 triệu: Cần KYC + Admin manual approve

---

## 💎 MODULE 2 - SẢN PHẨM & GIÁ VÀNG ĐỘNG (PHÂN TÍCH SÂU)

### 2.1 Kiến trúc tính giá tự động

**Vấn đề cốt lõi:** Giá vàng thay đổi liên tục. MRP trong `ItemMst` không thể là con số cứng - phải tính động.

**Giải pháp - Tách thành 2 lớp:**

```
STATIC FIELDS (lưu trong DB, không đổi):
  - Net_Gold (trọng lượng vàng thực tế)
  - Gold_Making (phí gia công vàng)
  - Stone_Making (phí gia công đá)
  - Other_Making (phí khác)
  - Wastage_Per (% hao hụt)
  - Stone/Diamond info (carat, weight, rate)
  - VAT_rate (% thuế)

DYNAMIC FIELDS (tính runtime, không lưu DB):
  - Gold_Rate (lấy từ cache Redis)
  - Gold_Amt = Net_Gold × Gold_Rate
  - Wastage_Amt = Net_Gold × Wastage_Per × Gold_Rate
  - Total_Making = Gold_Making + Stone_Making + Other_Making
  - Stone_Amt = sum từ StoneMst + DimMst
  - Subtotal = Gold_Amt + Wastage_Amt + Total_Making + Stone_Amt
  - VAT_Amt = Subtotal × VAT_rate
  - MRP = Subtotal + VAT_Amt
  - Insurance_Fee = MRP × insurance_rate (nếu chọn)
  - Final_Price = MRP + Insurance_Fee
```

**Cron job cập nhật giá vàng:**

```csharp
// Hangfire job, chạy mỗi 15 phút
public class GoldPriceUpdateJob
{
    // Gọi GoldAPI.io hoặc scrape SJC
    // Lưu vào Redis với key: "gold_rate:XAU_VND"
    // TTL: 20 phút (nếu job fail, giá cũ vẫn dùng được)
    // Lưu lịch sử giá vào GoldPriceHistory table
}
```

**Scrape SJC (backup miễn phí 100%):**

```csharp
// SJC expose JSON endpoint không cần key
// GET https://sjc.com.vn/GoldPrice/Index
// Parse response lấy giá bán ra (gia_ban)
// Đây là giá thị trường VN thực tế, phù hợp cho đồ án
```

**API endpoint trả về giá sản phẩm:**

```
GET /api/v1/products/{style_code}/price
Response:
{
  "style_code": "YG-001",
  "gold_rate": 8500000,  // VND/chỉ, cập nhật lúc 14:30
  "gold_rate_updated_at": "2025-06-01T14:30:00Z",
  "breakdown": {
    "gold_material": 12750000,
    "wastage": 382500,
    "making_charges": 2500000,
    "stone_charges": 5000000,
    "subtotal": 20632500,
    "vat_10_percent": 2063250,
    "mrp": 22695750
  },
  "price_valid_until": "2025-06-01T14:45:00Z"  // 15 phút
}
```

**Cảnh báo biến động giá trong cart:**

- Lưu `gold_rate_at_add_time` vào CartItem
- Mỗi khi Customer vào trang cart, so sánh với giá hiện tại
- Nếu chênh > 3%: hiển thị banner warning màu vàng
- Nếu chênh > 10%: force re-confirm trước khi checkout

---

### 2.2 Data Model mở rộng (so với đề bài)

Đề bài thiếu một số trường quan trọng cần bổ sung:

```sql
-- Bổ sung vào ItemMst:
  is_active BOOLEAN DEFAULT TRUE
  stock_status ENUM('IN_STOCK', 'LOW_STOCK', 'OUT_OF_STOCK', 'COMING_SOON')
  view_count INT DEFAULT 0
  sold_count INT DEFAULT 0
  created_at TIMESTAMP
  updated_at TIMESTAMP
  vendor_id VARCHAR(50) FK  -- ai đăng sản phẩm này

-- Bổ sung bảng ProductImages:
  image_id (PK)
  style_code (FK)
  image_url VARCHAR(500)
  is_primary BOOLEAN
  sort_order INT

-- Bổ sung bảng GoldPriceHistory:
  id (PK)
  gold_rate DECIMAL(15,2)
  source ENUM('GOLDAPI', 'SJC', 'MANUAL')
  recorded_at TIMESTAMP

-- Bổ sung bảng ProductReviews:
  review_id (PK)
  style_code (FK)
  user_id (FK)
  rating INT (1-5)
  comment TEXT
  verified_purchase BOOLEAN
  created_at TIMESTAMP
```

---

## 🛒 MODULE 3 - CART & CHECKOUT (PHÂN TÍCH SÂU)

### 3.1 Race Condition - Thiết kế kỹ thuật chi tiết

Đây là phần **giám khảo kỹ thuật sẽ hỏi** - cần hiểu rõ.

**Scenario nguy hiểm:**

```
T=0: ProductA có Quantity = 1
T=1: CustomerA thêm ProductA vào cart
T=1: CustomerB thêm ProductA vào cart
T=2: CustomerA bắt đầu checkout
T=2: CustomerB bắt đầu checkout  ← RACE CONDITION
T=3: Cả 2 đều thanh toán thành công?? ← BUG
```

**Giải pháp - 3 lớp bảo vệ:**

**Lớp 1: Inventory Soft Lock (Redis)**

```csharp
// Khi bắt đầu checkout session:
public async Task<bool> TryLockInventory(string styleCode, string userId)
{
    var lockKey = $"inventory_lock:{styleCode}";
    var lockValue = userId;
    var ttl = TimeSpan.FromMinutes(15);

    // SET NX EX - Atomic operation, chỉ set nếu key chưa tồn tại
    var acquired = await _redis.StringSetAsync(
        lockKey, lockValue, ttl, When.NotExists
    );

    return acquired; // false = người khác đang giữ lock
}
```

**Lớp 2: Optimistic Concurrency (Database)**

```csharp
// Trong ItemMst thêm cột:
// RowVersion ROWVERSION (SQL Server) hoặc xmin (PostgreSQL)

// Khi update quantity:
context.Database.ExecuteSqlRaw(
    "UPDATE ItemMst SET Quantity = Quantity - 1 " +
    "WHERE Style_Code = {0} AND Quantity > 0 AND RowVersion = {1}",
    styleCode, originalRowVersion
);

// Nếu affected rows = 0 → ai đó đã update trước → throw ConcurrencyException
```

**Lớp 3: Idempotency Key (Payment)**

```
POST /api/v1/orders/checkout
Headers:
  X-Idempotency-Key: {uuid-generated-on-frontend}

Backend:
  - Check Redis: key "idempotency:{uuid}" đã tồn tại chưa?
  - Nếu có → trả về kết quả đã lưu (không process lại)
  - Nếu không → process, lưu kết quả vào Redis TTL 24h
```

---

### 3.2 Checkout Flow State Machine

```
STATES:
CART_ACTIVE
    → CHECKOUT_INITIATED (lock inventory)
        → DEPOSIT_PENDING (chờ thanh toán cọc)
            → DEPOSIT_PAID (đã cọc)
                → AWAITING_VENDOR_CONFIRMATION (đợi vendor gọi confirm)
                    → CONFIRMED (vendor xác nhận)
                        → AWAITING_FULL_PAYMENT (thanh toán phần còn lại)
                            → FULLY_PAID
                                → PREPARING (vendor chuẩn bị hàng)
                                    → SHIPPED (đã gửi hàng)
                                        → DELIVERED (đã nhận)
                                            → COMPLETED
                                        → DELIVERY_FAILED (giao thất bại)
                    → VENDOR_REJECTED (vendor không có hàng)
                        → REFUNDING → REFUNDED
        → CHECKOUT_EXPIRED (quá 15 phút không thanh toán → release lock)
```

**Tại sao cần state machine thay vì enum đơn giản?**

- Ngăn transition bất hợp lệ (VD: không thể từ COMPLETED về CART_ACTIVE)
- Mỗi transition có thể trigger side effects (gửi email, notification)
- Dễ audit và debug

---

### 3.3 Quy trình đặt cọc - Logic chi tiết

```
TÍNH SỐ TIỀN CỌC:
  Giá trị đơn < 10 triệu  → Cọc tối thiểu 30% hoặc full
  Giá trị đơn 10-50 triệu → Cọc tối thiểu 50%
  Giá trị đơn > 50 triệu  → Cọc 100% (quá cao cấp, không chấp nhận rủi ro)

THỜI HẠN THANH TOÁN CỌC:
  Sau khi tạo đơn → 2 giờ để hoàn tất thanh toán cọc
  Quá 2 giờ → đơn tự huỷ, release inventory lock

THANH TOÁN PHẦN CÒN LẠI:
  Vendor xác nhận đơn + hàng ready → gửi link thanh toán phần còn lại
  Customer có 24h để thanh toán nốt
  Sau đó vendor mới dispatch

DATABASE - OrderDeposit table:
  order_id (FK)
  deposit_amount DECIMAL
  deposit_percentage INT
  deposit_paid_at TIMESTAMP
  deposit_payment_method
  remaining_amount DECIMAL
  remaining_paid_at TIMESTAMP (nullable)
```

---

## 📞 MODULE 4 - XÁC NHẬN VENDOR (PHÂN TÍCH SÂU)

### 4.1 SLA Engine

```
Sau khi order vào trạng thái DEPOSIT_PAID:

T+0h:   Notification đến Vendor (email + in-app)
T+2h:   Nếu chưa liên hệ → reminder notification
T+4h:   Nếu chưa liên hệ → ESCALATE lên Admin
T+8h:   Admin nhận alert "Vendor chưa xử lý đơn"
T+24h:  Hệ thống tự động chuyển trạng thái CONTACT_FAILED
        → Trigger hoàn cọc 100% cho Customer
        → Ghi nhận vi phạm SLA vào Vendor profile
        → Nếu Vendor vi phạm 3 lần → Admin review account
```

**Database - OrderContactLog:**

```sql
contact_log_id (PK)
order_id (FK)
vendor_id (FK)
attempt_number INT  -- lần gọi thứ mấy
contact_method ENUM('PHONE', 'EMAIL', 'ZALO')
contact_at TIMESTAMP
result ENUM('CONFIRMED', 'NO_ANSWER', 'REJECTED', 'CALLBACK_REQUESTED')
note TEXT
```

**UI cho Vendor:**

- Badge số đơn cần liên hệ ngay trên nav
- Nút "Đã liên hệ - Khách xác nhận" → mở modal điền note
- Nút "Không liên lạc được" → đặt lịch retry sau 30 phút
- Nút "Khách từ chối" → trigger hoàn cọc flow

---

## 💳 MODULE 5 - THANH TOÁN (PHÂN TÍCH SÂU)

### 5.1 Stripe Test Mode - Setup chi tiết

```typescript
// .env
STRIPE_SECRET_KEY=sk_test_...  // Test key, không tốn tiền
STRIPE_PUBLISHABLE_KEY=pk_test_...
STRIPE_WEBHOOK_SECRET=whsec_...

// Backend .NET 8 - Tạo Payment Intent
var options = new PaymentIntentCreateOptions {
    Amount = (long)(orderAmount * 100), // Stripe dùng cents/xu
    Currency = "vnd", // hoặc "usd" cho test
    Metadata = new Dictionary<string, string> {
        { "order_id", orderId },
        { "idempotency_key", idempotencyKey }
    }
};

// Frontend Next.js - Stripe Elements
import { loadStripe } from '@stripe/stripe-js'
import { Elements, PaymentElement } from '@stripe/react-stripe-js'
// Card test: 4242 4242 4242 4242, exp: any future, CVC: any 3 digits
```

**Webhook xử lý kết quả thanh toán:**

```csharp
// POST /api/v1/webhooks/stripe
// Stripe gọi về endpoint này sau khi payment hoàn tất

[HttpPost("stripe")]
public async Task<IActionResult> StripeWebhook()
{
    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
    var stripeEvent = EventUtility.ConstructEvent(
        json,
        Request.Headers["Stripe-Signature"],
        _webhookSecret
    );

    switch (stripeEvent.Type)
    {
        case "payment_intent.succeeded":
            // → Update order status, gửi email xác nhận
            break;
        case "payment_intent.payment_failed":
            // → Release inventory lock, notify customer
            break;
    }
}
```

### 5.2 PayOS (Payment local VN) - Setup

```typescript
// PayOS đơn giản hơn Stripe, phù hợp VN market
// Docs: https://payos.vn/docs

const payOS = new PayOS(process.env.PAYOS_CLIENT_ID, process.env.PAYOS_API_KEY, process.env.PAYOS_CHECKSUM_KEY);

const paymentLink = await payOS.createPaymentLink({
    orderCode: Number(orderId),
    amount: totalAmount,
    description: `Thanh toan don hang #${orderId}`,
    returnUrl: `${BASE_URL}/orders/${orderId}/success`,
    cancelUrl: `${BASE_URL}/orders/${orderId}/cancel`,
});
// → Redirect customer đến paymentLink.checkoutUrl
```

---

## 🚛 MODULE 6 - VẬN CHUYỂN GHN (PHÂN TÍCH SÂU)

### 6.1 GHN Sandbox Integration

```
Đăng ký: https://dev.ghn.vn
→ Tạo tài khoản sandbox
→ Lấy Token + ShopID (test)
→ Mọi API call đều free trong sandbox
```

**Các API GHN cần dùng:**

```csharp
// 1. Tính phí vận chuyển (hiển thị khi checkout)
POST https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee
{
  "service_type_id": 2,  // Express
  "from_district_id": 1442,
  "to_district_id": 1820,
  "to_ward_code": "020408",
  "weight": 500,  // gram
  "insurance_value": 22000000  // Giá trị bảo hiểm GHN
}

// 2. Tạo vận đơn (khi vendor dispatch)
POST https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create
{
  "to_name": "Nguyen Van A",
  "to_phone": "0912345678",
  "to_address": "123 Nguyen Trai",
  "to_ward_name": "Phuong 1",
  "to_district_name": "Quan 5",
  "to_province_name": "Ho Chi Minh",
  "weight": 500,
  "cod_amount": 0,  // = 0 vì đã thanh toán online
  "insurance_value": 22000000,
  "items": [{ "name": "Nhan kim cuong", "quantity": 1 }]
}

// 3. Tracking đơn hàng
GET https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail
?order_code=GHN_TRACKING_CODE
```

**Lưu ý quan trọng về bảo hiểm GHN:**

- GHN tự có bảo hiểm vận chuyển theo `insurance_value`
- Phí bảo hiểm GHN: khoảng 0.5% giá trị hàng
- Đây là lớp bảo hiểm **vận chuyển** (khác với order insurance của hệ thống)
- Cần phân biệt rõ 2 lớp này trong UI để tránh nhầm lẫn

---

## 🛡️ MODULE 7 - BẢO HIỂM ĐƠN HÀNG (PHÂN TÍCH SÂU)

### 7.1 Thiết kế hệ thống bảo hiểm (Mock internal)

Vì không có đối tác bảo hiểm thực, xây dựng module bảo hiểm nội bộ (mock business logic):

```
PHÂN LOẠI BẢO HIỂM:

1. BẢO HIỂM VẬN CHUYỂN (Shipping Insurance):
   - Phạm vi: hư hỏng/mất mát trong quá trình ship
   - Tự động tích hợp qua GHN insurance_value
   - Phí: 0.5% giá trị đơn
   - Claim: trong 48h sau khi phát sinh sự cố

2. BẢO HIỂM SẢN PHẨM (Product Insurance):
   - Phạm vi: hư hỏng sau khi nhận hàng (trong 30 ngày đầu)
   - Phí: 1% giá trị đơn
   - Claim: upload ảnh + mô tả sự cố

BẢNG TÍNH PHÍ:
  Giá trị đơn × tỷ lệ bảo hiểm = Phí BH

  Ví dụ:
  22,695,750 VND × 0.5% = 113,478 VND (ship insurance)
  22,695,750 VND × 1.0% = 226,957 VND (product insurance)

  Option 1: Không mua BH          +0
  Option 2: BH vận chuyển only    +113,478
  Option 3: BH đầy đủ (ship + SP) +340,435
```

**Database - InsuranceClaim table:**

```sql
claim_id (PK)
order_id (FK)
claim_type ENUM('SHIPPING', 'PRODUCT')
incident_date DATE
description TEXT
evidence_urls JSON  -- array of image URLs
status ENUM('SUBMITTED', 'UNDER_REVIEW', 'APPROVED', 'REJECTED', 'PAID')
approved_amount DECIMAL (nullable)
reviewed_by (nullable FK → admin)
review_note TEXT
created_at TIMESTAMP
```

---

## 🧾 MODULE 8 - HÓA ĐƠN & THUẾ (PHÂN TÍCH SÂU)

### 8.1 Cấu trúc Invoice chuẩn

```
Invoice phải đủ các yếu tố pháp lý cơ bản:

HEADER:
  - Tên công ty bán (Vendor info)
  - Mã số thuế (MST) Vendor - mock trong test
  - Địa chỉ Vendor
  - Số hóa đơn: INV-{YYYYMMDD}-{5 chữ số tự tăng}
  - Ngày xuất hóa đơn

THÔNG TIN MUA HÀNG:
  - Tên người mua (từ KYC)
  - CCCD/CMND người mua
  - Địa chỉ người mua

CHI TIẾT SẢN PHẨM:
  - Style Code + Tên sản phẩm
  - Chứng nhận (Certify_Type)
  - Vàng: loại + trọng lượng + đơn giá nguyên liệu
  - Kim cương: carat + số viên + đơn giá
  - Đá quý: loại + carat + đơn giá

BẢNG TÍNH GIÁ (HIỂN THỊ ĐẦY ĐỦ):
  Giá vàng nguyên liệu:     xx,xxx,xxx VND
  Phí hao hụt (x%):         xx,xxx VND
  Phí gia công vàng:         xx,xxx VND
  Phí gia công đá:           xx,xxx VND
  Phí khác:                  xx,xxx VND
  ─────────────────────────────────────
  Tổng trước thuế:           xx,xxx,xxx VND
  VAT (10%):                 x,xxx,xxx VND
  ─────────────────────────────────────
  Giảm giá (nếu có):        -x,xxx VND
  Phí bảo hiểm (nếu có):   +x,xxx VND
  ─────────────────────────────────────
  TỔNG THANH TOÁN:           xx,xxx,xxx VND

  Đã đặt cọc:               -xx,xxx,xxx VND
  CÒN LẠI CẦN THANH TOÁN:   xx,xxx,xxx VND
```

**Generate PDF Invoice:**

```csharp
// .NET 8 dùng QuestPDF (free, open source, cực ngon)
// Install: dotnet add package QuestPDF

Document.Create(container => {
    container.Page(page => {
        page.Content().Column(col => {
            col.Item().Text("HÓA ĐƠN BÁN HÀNG").Bold().FontSize(20);
            // ... render từng section
        });
    });
}).GeneratePdf("invoice.pdf");
```

### 8.2 Yêu cầu cấp lại hóa đơn

```
Flow:
1. Customer vào Orders → chọn đơn đã COMPLETED
2. Click "Yêu cầu cấp lại hóa đơn"
3. Điền form:
   - Lý do cấp lại (dropdown: Mất hóa đơn gốc / Sai thông tin / Phục vụ kế toán)
   - Thông tin cần thay đổi (nếu sai): tên người mua, địa chỉ
   - Upload ảnh CCCD để verify
4. Vendor nhận request → review trong 48h
5. Nếu duyệt → QuestPDF generate PDF mới
6. Email PDF đến Customer + lưu trong order history
7. Invoice mới có watermark "BẢN SAO" + số lần cấp lại

Database - InvoiceReissueRequest:
  request_id (PK)
  order_id (FK)
  user_id (FK)
  reason ENUM
  change_note TEXT
  status ENUM('PENDING', 'APPROVED', 'REJECTED')
  reissued_invoice_url VARCHAR (nullable)
  created_at TIMESTAMP
```

---

## 🏪 MODULE 9 - ĐA CẤP ĐẠI LÝ (PHÂN TÍCH SÂU)

### 9.1 Database Schema cho cây đại lý

```sql
-- Dùng Adjacency List Model (đủ cho 3 cấp, đơn giản hơn Closure Table)
Vendor:
  vendor_id (PK)
  user_id (FK → UserRegMst)  -- tài khoản đăng nhập
  parent_vendor_id (FK → Vendor, nullable) -- NULL = cấp 1
  vendor_level INT  -- 1, 2, 3
  business_name VARCHAR(200)
  business_license_url VARCHAR  -- Giấy phép kinh doanh
  tax_code VARCHAR(20)
  commission_rate DECIMAL(5,2)  -- % nhận từ cấp dưới
  status ENUM('PENDING', 'ACTIVE', 'SUSPENDED')
  approved_by (FK → Admin, nullable)
  created_at TIMESTAMP

-- Lấy toàn bộ cây của 1 vendor (đệ quy CTE):
WITH RECURSIVE vendor_tree AS (
  SELECT vendor_id, parent_vendor_id, vendor_level, business_name
  FROM Vendor WHERE vendor_id = :root_vendor_id
  UNION ALL
  SELECT v.vendor_id, v.parent_vendor_id, v.vendor_level, v.business_name
  FROM Vendor v
  INNER JOIN vendor_tree vt ON v.parent_vendor_id = vt.vendor_id
)
SELECT * FROM vendor_tree;
```

### 9.2 Hoa hồng (Commission) - Logic tự động

```
Khi đơn hàng của cấp 3 COMPLETED:
  Order total = 22,000,000 VND

  Cấp 3 Vendor doanh thu: 22,000,000
  Trả hoa hồng cấp 2 (3%): 660,000
  Trả hoa hồng cấp 1 (2%): 440,000

  → Tạo CommissionLedger records:
    | from_vendor | to_vendor | amount  | order_id | status   |
    | vendor_L3   | vendor_L2 | 660,000 | #001     | PENDING  |
    | vendor_L2   | vendor_L1 | 440,000 | #001     | PENDING  |

  → Tổng hợp cuối tháng → PAID khi Admin duyệt thanh toán
```

---

## 🎁 MODULE 10 - LOYALTY & GAMIFICATION (PHÂN TÍCH SÂU)

### 10.1 Điểm tích lũy

```
EARN POINTS:
  - Mỗi 100,000 VND chi tiêu = 10 điểm
  - Đánh giá sản phẩm sau mua = +5 điểm
  - Giới thiệu bạn bè mua hàng = +50 điểm/người
  - Sinh nhật = +100 điểm (cron job check dob hàng ngày)

REDEEM POINTS:
  100 điểm = 10,000 VND giảm giá
  Tối đa dùng 20% giá trị đơn bằng điểm

DATABASE - PointsLedger:
  entry_id (PK)
  user_id (FK)
  points INT  -- dương = earn, âm = redeem
  type ENUM('PURCHASE', 'REVIEW', 'REFERRAL', 'BIRTHDAY', 'REDEEM', 'EXPIRE')
  ref_id VARCHAR  -- order_id hoặc review_id
  created_at TIMESTAMP

  -- Tổng điểm hiện tại = SUM(points) WHERE user_id = x
  -- (Không lưu total riêng để tránh inconsistency)
```

### 10.2 Vòng quay may mắn

```
TRIGGER: Sau khi order COMPLETED → popup vòng quay

PHẦN THƯỞNG CONFIG (Admin cài):
  - 40%: Mã giảm giá 2%
  - 25%: Mã giảm giá 5%
  - 15%: Mã giảm giá 10%
  - 10%: +50 điểm thưởng
  - 5%:  Miễn phí bảo hiểm đơn hàng sau
  - 4%:  Voucher quà tặng 500k
  - 1%:  Jackpot: Giảm 20%

ANTI-CHEAT:
  - 1 đơn hàng = 1 lượt quay (link spin_id với order_id)
  - Kết quả được xác định server-side, không phải animation client
  - Client chỉ nhận kết quả và play animation tương ứng

  // Backend generate kết quả:
  var random = new SecureRandom(); // dùng cryptographic random, không phải Math.random
  var result = WeightedRandom(prizes); // dựa trên probability config
  // Lưu kết quả vào DB trước, rồi trả về client
```

---

## 📋 THỨ TỰ PHÁT TRIỂN ĐỀ XUẤT

```
SPRINT 1 (Core):
  ✅ Auth + JWT + Role system
  ✅ Product CRUD + Image upload
  ✅ Search + Filter
  ✅ Cart cơ bản

SPRINT 2 (Business Logic):
  ✅ Gold price cron + Dynamic pricing
  ✅ Checkout flow + State machine
  ✅ Race condition prevention (Redis lock)
  ✅ Deposit flow

SPRINT 3 (Integration):
  ✅ Stripe/PayOS payment
  ✅ GHN shipping
  ✅ Email (Resend)
  ✅ eKYC (face-api.js)

SPRINT 4 (Advanced):
  ✅ Multi-tier vendor + Commission
  ✅ Insurance module
  ✅ Invoice PDF (QuestPDF)
  ✅ Incident/Dispute handling

SPRINT 5 (Polish):
  ✅ Loyalty + Spin wheel
  ✅ Dashboard + Reports
  ✅ SLA engine + Admin tools
  ✅ Audit logging
```

---

**Điểm mấu chốt để đồ án nổi bật:** Race condition + State machine order + Dynamic gold pricing là 3 thứ cho thấy tư duy hệ thống thực sự. Giải thích được _tại sao_ thiết kế như vậy (không chỉ _cái gì_) sẽ là điểm phân hoá so với đồ án CRUD thông thường.
