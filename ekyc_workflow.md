# 📋 README - Online Jewellery Shopping Platform

### Tech Stack: Next.js · Bun · .NET 8.0 | Đề tài: Yash Gems & Jewelleries

---

## 🎭 ROLES TRONG HỆ THỐNG

| Role                      | Mô tả                                    |
| ------------------------- | ---------------------------------------- |
| **Super Admin**           | Toàn quyền hệ thống, cấu hình platform   |
| **Admin**                 | Quản lý vận hành hàng ngày               |
| **Vendor (Đại lý cấp 1)** | Nhà cung cấp chính thống từ Yash         |
| **Sub-Vendor cấp 2**      | Đại lý trung gian do Vendor cấp 1 tạo ra |
| **Sub-Vendor cấp 3**      | Đại lý bán lẻ cuối chuỗi                 |
| **Customer (Khách hàng)** | Người mua hàng cuối                      |
| **Shipper / Logistics**   | Đơn vị vận chuyển (tích hợp bên thứ 3)   |
| **Insurance Agent**       | Bên bảo hiểm đơn hàng (bên thứ 3)        |
| **API Partner**           | Các shop trang sức mua quyền dùng API    |

---

## 🗂️ MODULE 1: XÁC THỰC & QUẢN LÝ TÀI KHOẢN

### 1.1 Đăng ký & Đăng nhập

- Đăng ký tài khoản Customer với email, SĐT
- Xác thực OTP qua SMS/Email trước khi kích hoạt tài khoản
- Đăng nhập bằng username/password (JWT + Refresh Token)
- Đăng nhập bằng Google OAuth2
- Logout + invalidate token toàn bộ session

### 1.2 eKYC - Xác minh danh tính (Tính năng đặc thù cho hàng giá trị cao)

- Upload 2 mặt CCCD/Passport
- **Quét khuôn mặt (Face Liveness Detection)**: so khớp khuôn mặt với ảnh CCCD bằng thư viện FaceIO hoặc tích hợp service (VD: VNPTSmartCA, eKYC của FPT.AI)
- Trạng thái KYC: `PENDING` => `VERIFIED` => `REJECTED`
- **Chỉ cho phép đặt hàng khi KYC = VERIFIED** (rule áp dụng cho đơn hàng trên ngưỡng giá nhất định, ví dụ >5 triệu)
- Lý do: trang sức kim cương giá trị cao, cần định danh để tránh gian lận

### 1.3 Quản lý Profile

- Cập nhật thông tin cá nhân (map với `UserRegMst`)
- Quản lý địa chỉ giao hàng (nhiều địa chỉ)
- Lịch sử đơn hàng
- Đổi mật khẩu / Quên mật khẩu qua email

---

## 🛍️ MODULE 2: DANH MỤC & SẢN PHẨM

### 2.1 Quản lý Danh mục

- CRUD danh mục sản phẩm (`CatMst`)
- CRUD loại trang sức (`JewelTypeMst`): nhẫn, dây chuyền, bông tai, lắc tay...
- CRUD thương hiệu (`BrandMst`): Asmi, D'damas...
- CRUD loại vàng (`GoldKrtMst`): 18K, 22K...
- CRUD chứng nhận (`CertifyMst`): 918, 920...
- CRUD chất lượng kim cương (`DimQltyMst`, `DimQltySubMst`)
- CRUD chất lượng đá quý (`StoneQltyMst`)

### 2.2 Quản lý Sản phẩm (`ItemMst`)

- CRUD sản phẩm với đầy đủ trường: `Style_Code`, `Brand`, `Category`, `Certification`, `Gold_Crt`, `Gold_Wt`, `Net_Gold`, `Wastage`, `MRP`...
- Upload nhiều ảnh sản phẩm (hỗ trợ 360° view nếu có)
- Gán thông tin kim cương (`DimMst`): carat, số mảnh, trọng lượng, đơn giá
- Gán thông tin đá quý (`StoneMst`): carat, số viên, trọng lượng, đơn giá
- Quản lý tồn kho (`Quantity`) theo thời gian thực
- Trạng thái sản phẩm: `ACTIVE` / `INACTIVE` / `SOLD_OUT` / `COMING_SOON`

### 2.3 Bảng giá động - **Tự động cập nhật giá vàng**

- Tích hợp API giá vàng thời gian thực (ví dụ: SJC API hoặc Metals-API, GoldAPI.io)
- Cron job chạy mỗi 15 phút cập nhật `Gold_Rate` trong database
- **Tính toán MRP tự động** theo công thức:
    ```
    MRP = (Net_Gold × Gold_Rate) + Gold_Making + Stone_Making + Other_Making + (Wastage × Gold_Rate) + Tax
    ```
- Hiển thị chi tiết cấu thành giá trên trang sản phẩm:
    - Giá vàng tại thời điểm
    - Phí gia công (Making Charges)
    - Trọng lượng đá/kim cương
    - Hao hụt vàng (Wastage)
    - Thuế VAT
    - Phí bảo hiểm (nếu chọn)
- **Cảnh báo chênh lệch giá**: nếu giá vàng biến động >3% so với lúc khách thêm vào giỏ, thông báo và yêu cầu xác nhận lại giá trước khi checkout

### 2.4 Thông tin tra cứu kim cương (`DimInfoMst`)

- Trang tra cứu thông tin kim cương: loại, carat, giá tham chiếu, hình ảnh
- Nội dung giáo dục: lịch sử kim cương, ý nghĩa đá quý, hướng dẫn chọn mua
- **Quy trình 4C của kim cương**: Cut, Color, Clarity, Carat - hiển thị dạng interactive
- Thông tin chứng nhận kim cương (GIA, IGI...) và cách đọc chứng chỉ

---

## 🔍 MODULE 3: TÌM KIẾM & LỌC SẢN PHẨM

### 3.1 Search Engine

- Full-text search theo tên, mã sản phẩm, thương hiệu
- Combinatorial search (bám sát yêu cầu đề): lọc đồng thời nhiều tiêu chí
- Filter theo: Danh mục, Thương hiệu, Loại vàng (18K/22K), Loại đá, Chất lượng kim cương, Chứng nhận, Khoảng giá
- Sort: Giá tăng dần/giảm dần, Mới nhất, Bán chạy nhất, Đánh giá cao nhất
- Lịch sử tìm kiếm gần đây
- Gợi ý tìm kiếm (autocomplete)

### 3.2 Tính năng nâng cao

- **Tìm tỉ lệ chênh lệch**: So sánh giá sản phẩm với giá vàng thị trường, hiển thị % chênh lệch so với giá gốc nguyên liệu (minh bạch hoá chi phí gia công)
- Bộ lọc theo ngân sách (budget range slider)
- Filter theo dịp (nhẫn cưới, quà sinh nhật, lễ kỷ niệm...)

---

## 🛒 MODULE 4: GIỎ HÀNG & CHECKOUT

### 4.1 Shopping Cart (`CartList`)

- Thêm/xóa sản phẩm khỏi giỏ hàng
- Drag & drop sản phẩm vào/ra giỏ hàng (bám sát yêu cầu đề)
- Persistent cart (lưu giỏ hàng khi đăng nhập lại)
- Cảnh báo khi giá vàng thay đổi từ lúc thêm vào giỏ
- Hiển thị chi tiết cấu thành giá từng sản phẩm trong giỏ

### 4.2 Quy trình Checkout (Quan trọng - thiết kế cẩn thận vì hàng giá trị cao)

```
Bước 1: Xem lại giỏ hàng + xác nhận giá tại thời điểm
Bước 2: Chọn địa chỉ giao hàng (hoặc nhập địa chỉ mới)
Bước 3: Chọn option bảo hiểm đơn hàng (có/không)
Bước 4: Áp dụng mã giảm giá (nếu có)
Bước 5: Chọn phương thức thanh toán
Bước 6: Đặt cọc / Thanh toán toàn phần
Bước 7: ĐỢI xác nhận từ Vendor (gọi điện/liên hệ trong 2-4h)
Bước 8: Sau khi Vendor xác nhận => hệ thống chốt đơn hàng chính thức
```

### 4.3 Chống Race Condition - **Inventory Lock**

- Khi Customer bắt đầu checkout, lock sản phẩm tạm thời trong **15 phút** (dùng Redis hoặc Distributed Lock trong .NET)
- Nếu quá 15 phút chưa thanh toán => tự động release lock
- Trường hợp 2 người checkout cùng lúc 1 sản phẩm duy nhất (`Quantity = 1`): chỉ 1 người được lock, người còn lại nhận thông báo "Sản phẩm đang được người khác giữ chỗ"
- Optimistic Concurrency Control ở tầng database (.NET EF Core `RowVersion`/`Timestamp`)
- Idempotency key cho payment request: tránh double-charge khi user click nút thanh toán 2 lần

---

## 💰 MODULE 5: ĐẶT CỌC & THANH TOÁN

### 5.1 Quy trình Đặt cọc (Deposit) - Thiết kế cho hàng giá trị cao

Lý do cần đặt cọc: Kim cương và trang sức cao cấp không thể xử lý như hàng thông thường. Vendor cần thời gian verify đơn, confirm hàng thực tế có sẵn, liên hệ khách hàng.

**Quy trình đề xuất:**

```
1. Customer đặt hàng => Chọn mức đặt cọc: 30% / 50% / 100% giá trị đơn
2. Hệ thống lock tồn kho, tạo đơn hàng trạng thái: PENDING_DEPOSIT
3. Customer hoàn tất thanh toán cọc => trạng thái: DEPOSIT_PAID
4. Vendor nhận notification => Gọi điện xác nhận với khách hàng (bắt buộc)
5. Vendor xác nhận đơn => trạng thái: CONFIRMED
6. Customer thanh toán phần còn lại trước khi giao hàng (COD hoặc chuyển khoản)
7. Vendor dispatch => trạng thái: SHIPPING
8. Customer nhận hàng, xác nhận => trạng thái: COMPLETED
```

**Chính sách hoàn cọc:**

- Khách hủy trong 24h sau khi đặt cọc => hoàn 100%
- Khách hủy sau 24h => hoàn 50% (trừ phí xử lý)
- Vendor không xác nhận đơn trong 48h => hoàn 100% cọc cho khách
- Quy trình hoàn tiền phải có timeline rõ ràng (3-5 ngày làm việc)

### 5.2 Phương thức Thanh toán

- **Thẻ tín dụng/debit** (Visa, Mastercard) - bám sát yêu cầu đề
- **VNPay / MoMo / ZaloPay** (local payment gateway cho thị trường VN)
- **Chuyển khoản ngân hàng** với auto-verify qua webhook
- **COD** (chỉ áp dụng cho đơn giá trị nhỏ, ví dụ <2 triệu, và **bắt buộc phải xác nhận KYC**)

### 5.3 Chi tiết hoá đơn (Invoice) - Bám sát quy định thuế

Mỗi hoá đơn phải hiển thị đầy đủ:

- Mã hoá đơn (tự sinh, format: `INV-YYYYMMDD-XXXXX`)
- Thông tin người mua (theo KYC)
- Thông tin người bán (Vendor info)
- Chi tiết sản phẩm: mã hàng, mô tả, trọng lượng, chứng nhận
- Bảng tính giá chi tiết:
    - Giá vàng nguyên liệu × trọng lượng
    - Phí gia công (Making Charges)
    - Phí đá quý / kim cương
    - Phí hao hụt (Wastage)
    - Phí bảo hiểm đơn hàng (nếu có)
    - Thuế VAT (10%)
    - Mã giảm giá (nếu có)
    - **Tổng thanh toán**
- Mã QR để tra cứu hoá đơn
- Chữ ký điện tử Vendor
- **Tính năng yêu cầu cấp lại hoá đơn**: Customer có thể request cấp lại invoice, cung cấp đủ thông tin sản phẩm (Style Code, ngày mua, thông tin người mua) => Vendor duyệt => hệ thống tự generate PDF invoice mới

---

## 📞 MODULE 6: XÁC NHẬN ĐƠN HÀNG (CONTACT CONFIRMATION)

### 6.1 Quy trình liên hệ xác nhận - Bắt buộc trước khi chốt đơn

- Sau khi Customer đặt cọc, Vendor **bắt buộc phải gọi điện xác nhận** trong vòng **2-4 giờ làm việc**
- Trong giao diện Vendor: nút "Đã liên hệ xác nhận" - Vendor nhập note cuộc gọi
- Hệ thống log lại: thời gian gọi, kết quả (đã xác nhận / không liên lạc được / khách từ chối)
- Nếu không liên hệ được sau 3 lần: đơn chuyển trạng thái `CONTACT_FAILED` => Admin review
- SLA alert: nếu Vendor chưa liên hệ sau 4h => notification nhắc nhở, sau 8h => escalate lên Admin

### 6.2 Kênh liên hệ tích hợp

- Click-to-call từ dashboard (tích hợp Twilio hoặc VGTEL)
- Zalo OA (nếu target market VN)
- Email tự động gửi xác nhận đơn hàng kèm link tra cứu

---

## 🚚 MODULE 7: VẬN CHUYỂN & XỬ LÝ SỰ CỐ

### 7.1 Quản lý giao hàng

- Tích hợp API đơn vị vận chuyển (GHN, GHTK, ViettelPost)
- Auto tạo vận đơn khi Vendor xác nhận dispatch
- Tracking đơn hàng real-time
- **Giao hàng có chữ ký bắt buộc** cho đơn hàng trên ngưỡng giá (trang sức cao cấp)
- Ảnh xác nhận khi giao (shipper upload)

### 7.2 Xử lý sự cố giao hàng - Tính năng đề xuất của Tuấn

Khi phát sinh lỗi trong quá trình giao hàng:

**Form báo lỗi cho Shipper/Customer:**

- Chọn loại sự cố: Hàng bị hư hỏng / Mất hàng / Giao sai địa chỉ / Khách không nhận
- **Chọn lỗi do ai:**
    - 🔴 Lỗi do Shipper (đóng gói đúng nhưng shipper làm hỏng)
    - 🟡 Lỗi do Vendor (đóng gói sai, hàng không đúng mô tả)
    - 🔵 Lỗi do Customer (cung cấp sai địa chỉ, không có nhà nhận)
    - ⚪ Bất khả kháng (thiên tai, dịch bệnh...)

**Logic xử lý theo loại lỗi:**

- **Lỗi do Vendor** => Tự động generate mã giảm giá bồi thường cho Customer (% tùy cấu hình), hoàn tiền hoặc gửi hàng thay thế
- **Lỗi do Shipper** => Kích hoạt bảo hiểm vận chuyển, bên bảo hiểm xử lý bồi thường
- **Lỗi do Customer** => Phí giao lại do Customer chịu
- Upload ảnh bằng chứng bắt buộc khi report lỗi

### 7.3 Bảo hiểm đơn hàng (Order Insurance)

- Option chọn mua bảo hiểm khi checkout (tích hợp với bên thứ 3: PTI, Bảo Việt, hoặc tự xây mock)
- **Cách tính phí bảo hiểm:**
    ```
    Phí BH = Giá trị đơn hàng × Tỷ lệ BH (ví dụ: 0.5% - 1.5%)
    Tỷ lệ phụ thuộc vào: khoảng cách giao, loại hàng, lịch sử shipper
    ```
- Phạm vi bảo hiểm: mất hàng, hư hỏng trong vận chuyển
- Quy trình claim: Customer báo => upload bằng chứng => Vendor xác nhận => Insurance Agent duyệt => Bồi thường trong 3-5 ngày
- Lịch sử claim bảo hiểm trong profile Customer

---

## 🎁 MODULE 8: KHUYẾN MÃI & LOYALTY

### 8.1 Mã giảm giá (Coupon/Voucher)

- Admin/Vendor tạo mã giảm giá: theo %, theo số tiền cố định, hoặc miễn phí shipping
- Điều kiện áp dụng: giá trị đơn tối thiểu, danh mục sản phẩm cụ thể, thời hạn sử dụng, số lần dùng tối đa
- Mã hệ thống tự generate khi xử lý lỗi do Vendor
- Kiểm tra và apply mã tại trang checkout
- Lịch sử mã giảm giá trong tài khoản Customer

### 8.2 Vòng quay may mắn / Quà tặng

- Vòng quay may mắn: Customer tích điểm từ mỗi đơn hàng => đổi điểm lấy lượt quay
- Phần thưởng vòng quay: mã giảm giá, quà tặng vật lý (item trang sức nhỏ), điểm thưởng
- Popup vòng quay sau khi hoàn thành đơn hàng (event-driven)
- Chương trình ưu đãi lễ hội / dịp đặc biệt (tích hợp với trang chủ)
- Hệ thống điểm tích luỹ (Loyalty Points):
    - Mỗi 100k chi tiêu = 10 điểm
    - Điểm quy đổi thành voucher giảm giá

### 8.3 Tặng quà cho người khác - Bám sát yêu cầu đề

- Checkbox "Đây là quà tặng" trong checkout
- Nhập địa chỉ người nhận khác với địa chỉ người mua
- Tuỳ chọn viết thiệp điện tử kèm theo
- Gói hàng quà tặng cao cấp (premium wrapping, tính thêm phí)
- Ẩn thông tin giá trên phiếu giao hàng (gift mode)

---

## 🏪 MODULE 9: HỆ THỐNG ĐẠI LÝ ĐA CẤP (MULTI-TIER VENDOR)

### 9.1 Cấu trúc đại lý

```
Super Admin (Yash)
    └── Vendor cấp 1 (Nhà phân phối chính thức)
            └── Sub-Vendor cấp 2 (Đại lý khu vực)
                    └── Sub-Vendor cấp 3 (Cửa hàng bán lẻ)
```

### 9.2 Quy tắc phân cấp

- Vendor cấp 1 do Admin tạo và duyệt, phải qua KYC doanh nghiệp (MST, Giấy phép kinh doanh)
- Vendor cấp 1 có thể tạo Sub-Vendor cấp 2 (giới hạn số lượng theo gói đăng ký)
- Sub-Vendor cấp 2 tạo Sub-Vendor cấp 3
- Hoa hồng (Commission) tự động tính theo cây đại lý khi có đơn hàng:
    ```
    Cấp 1: 5% / đơn hàng cấp dưới
    Cấp 2: 3% / đơn hàng cấp dưới
    Cấp 3: trực tiếp bán hàng
    ```
- Dashboard riêng cho từng cấp, chỉ thấy dữ liệu trong cây của mình
- Report doanh thu theo cây đại lý

---

## 📊 MODULE 10: DASHBOARD & BÁO CÁO

### 10.1 Admin Dashboard

- Tổng quan: Doanh thu theo ngày/tuần/tháng/năm
- Số đơn theo trạng thái (biểu đồ)
- Top sản phẩm bán chạy
- Top Vendor doanh thu cao
- Alert: Đơn hàng quá SLA chưa xử lý, claim bảo hiểm chờ duyệt, KYC chờ verify
- Quản lý toàn bộ Users (Customer, Vendor) - CRUD, block/unblock
- Cấu hình platform: tỷ lệ bảo hiểm, hoa hồng đại lý, threshold KYC

### 10.2 Vendor Dashboard

- Đơn hàng đang chờ xử lý
- Tồn kho cần bổ sung
- Doanh thu tháng + so sánh tháng trước
- Báo cáo bán hàng xuất Excel/PDF
- Quản lý Sub-Vendor của mình
- Hoa hồng đã tích luỹ từ cây đại lý

### 10.3 Customer Dashboard

- Lịch sử đơn hàng + tracking
- Điểm tích luỹ
- Mã giảm giá đang có
- Lịch sử claim bảo hiểm
- Quản lý địa chỉ
- Thông tin KYC (xem trạng thái)

---

## 🔁 MODULE 11: THỦ TỤC SAU MUA HÀNG

### 11.1 Yêu cầu làm thủ tục (Post-Purchase Service)

Sau khi Customer nhận hàng, hỗ trợ các thủ tục:

- **Yêu cầu giấy chứng nhận chính hãng** (Certificate of Authenticity): kèm số serial kim cương/đá quý
- **Yêu cầu cấp lại hoá đơn**: điền form, cung cấp Style Code + ngày mua => Vendor verify => generate PDF
- **Đăng ký bảo hành**: kích hoạt bảo hành online, lưu thông tin vào hồ sơ khách hàng
- **Yêu cầu định giá lại (Revaluation)**: khi khách muốn bán lại hoặc thế chấp
- **Dịch vụ bảo trì / đánh bóng**: đặt lịch gửi sản phẩm về bảo dưỡng

### 11.2 Quy trình Return & Refund

- Chính sách đổi trả trong 7 ngày (nếu hàng lỗi do Vendor)
- Workflow: Customer submit return request => upload ảnh bằng chứng => Vendor review => Admin arbitrate nếu có tranh chấp
- Tiền hoàn vào ví hệ thống hoặc tài khoản ngân hàng (3-7 ngày)

---

## 📝 MODULE 12: INQUIRY & SUPPORT

### 12.1 Form liên hệ / Tư vấn (`Inquiry`)

- Map với bảng `Inquiry` trong đề bài: Name, City, Contact, Email, Comment
- Auto-reply email sau khi submit
- Vendor nhận notification và phải reply trong 24h (SLA)
- Lịch sử inquiry trong CRM của Vendor

### 12.2 Live Chat / Chatbot

- Widget chat trên website
- Bot trả lời các câu hỏi thường gặp: giá vàng hôm nay, quy trình mua, chính sách bảo hành
- Escalate sang human agent nếu bot không xử lý được

---

## 🔌 MODULE 13: API FOR PARTNERS (API Documentation & Monetization)

### 13.1 Public API cho đối tác

- RESTful API chuẩn với versioning (`/api/v1/...`)
- Endpoints chính expose cho partner:
    - `GET /products` - Lấy danh sách sản phẩm
    - `GET /products/{style_code}` - Chi tiết sản phẩm
    - `GET /gold-price` - Giá vàng hiện tại
    - `POST /orders` - Tạo đơn hàng
    - `GET /orders/{id}` - Tra cứu đơn hàng
    - `GET /diamond-info` - Thông tin tra cứu kim cương

### 13.2 API Key Management & Monetization

- Partner đăng ký tài khoản API => Chọn gói:
    - **Free**: 100 request/ngày, không có endpoint order
    - **Basic**: 10,000 request/ngày - phí cố định/tháng
    - **Pro**: Unlimited - phí cao hơn
- Dashboard theo dõi usage, rate limit (429 khi vượt quota)
- API Key rotation
- Swagger / OpenAPI 3.0 documentation tự động generate (tích hợp Swashbuckle trong .NET 8)
- Webhook: Partner nhận sự kiện khi đơn hàng thay đổi trạng thái

---

## 🛡️ MODULE 14: BẢO MẬT & COMPLIANCE

- HTTPS toàn bộ (TLS 1.3)
- JWT với expiry ngắn + Refresh Token rotation
- Rate limiting trên tất cả API endpoint
- Input validation và sanitization (chống XSS, SQL Injection)
- Audit log: mọi action quan trọng đều được log (ai làm gì, lúc nào)
- Mã hoá thông tin nhạy cảm trong DB (CCCD number, số thẻ credit card dùng tokenization - không lưu raw)
- GDPR-lite: Customer có thể yêu cầu export/xoá dữ liệu cá nhân
- 2FA option cho Vendor và Admin account

---

## 🏠 MODULE 15: TRANG CHỦ & CONTENT

### Bám sát yêu cầu đề:

- Banner hiển thị: Discount schemes, Gift offers, Festive offers, New launches
- Vòng quay may mắn popup (event-triggered)
- Section thông tin về kim cương và đá quý (edu content)
- Section hướng dẫn mua hàng lần đầu
- Widget giá vàng hôm nay (live update)
- Sản phẩm nổi bật / mới nhất / bán chạy
- Banner responsive, SEO-friendly

---

## ⚙️ MODULE 16: TECHNICAL EXCELLENCE (Điểm cộng kỹ thuật)

### Next.js (Frontend)

- App Router + Server Components cho performance
- Optimistic UI update cho cart actions
- Skeleton loading states
- Image optimization (next/image) cho ảnh sản phẩm
- SEO: dynamic metadata per product page
- PWA support (cài app trên mobile)

### .NET 8.0 (Backend)

- Clean Architecture: Domain / Application / Infrastructure / Presentation
- CQRS pattern với MediatR (tách read/write)
- EF Core với migrations
- Background Jobs (Hangfire / Quartz.NET) cho: cron cập nhật giá vàng, nhắc nhở SLA
- Distributed Lock với Redis cho race condition prevention
- Health check endpoints
- Structured logging (Serilog => ELK hoặc Seq)

### Bun (Runtime/Package manager)

- Tận dụng Bun test runner
- Bun build cho bundling nhanh hơn

---

## 📐 TỔNG KẾT - CHECKLIST TÍNH NĂNG

| #   | Tính năng                          | Nguồn         | Độ ưu tiên    |
| --- | ---------------------------------- | ------------- | ------------- |
| 1   | Auth + KYC eKYC face scan          | Tuấn đề xuất  | 🔴 Cao        |
| 2   | Quản lý sản phẩm đầy đủ            | Đề bài        | 🔴 Cao        |
| 3   | Giá vàng tự động cập nhật          | Tuấn đề xuất  | 🔴 Cao        |
| 4   | Search + Filter combinatorial      | Đề bài        | 🔴 Cao        |
| 5   | Cart + Drag & drop                 | Đề bài        | 🔴 Cao        |
| 6   | Race condition prevention          | Tuấn đề xuất  | 🔴 Cao        |
| 7   | Quy trình đặt cọc                  | Tuấn đề xuất  | 🔴 Cao        |
| 8   | Xác nhận khách hàng qua điện thoại | Tuấn đề xuất  | 🔴 Cao        |
| 9   | Thanh toán (Credit Card + local)   | Đề bài        | 🔴 Cao        |
| 10  | Invoice chi tiết + thuế VAT        | Tuấn đề xuất  | 🔴 Cao        |
| 11  | Bảo hiểm đơn hàng                  | Tuấn đề xuất  | 🟡 Trung bình |
| 12  | Xử lý sự cố giao hàng + lỗi do ai  | Tuấn đề xuất  | 🟡 Trung bình |
| 13  | Mã giảm giá                        | Tuấn đề xuất  | 🟡 Trung bình |
| 14  | Đa cấp đại lý (3 cấp)              | Tuấn đề xuất  | 🟡 Trung bình |
| 15  | API documentation + monetization   | Tuấn đề xuất  | 🟡 Trung bình |
| 16  | Cấp lại hoá đơn                    | Tuấn đề xuất  | 🟡 Trung bình |
| 17  | Vòng quay may mắn + loyalty        | Tuấn đề xuất  | 🟢 Cộng điểm  |
| 18  | Tặng quà cho người khác            | Đề bài        | 🟡 Trung bình |
| 19  | Thông tin giáo dục kim cương       | Đề bài        | 🟡 Trung bình |
| 20  | Tỉ lệ chênh lệch giá               | Tuấn đề xuất  | 🟢 Cộng điểm  |
| 21  | Thủ tục sau mua + bảo hành         | Tuấn đề xuất  | 🟢 Cộng điểm  |
| 22  | Báo cáo + xuất Excel/PDF           | Đề bài        | 🔴 Cao        |
| 23  | Dashboard đa role                  | Đề bài        | 🔴 Cao        |
| 24  | Audit log + bảo mật                | Best practice | 🟡 Trung bình |

---

**Lời khuyên từ góc độ senior**: Ưu tiên làm chắc các tính năng 🔴 trước, đảm bảo flow end-to-end hoạt động đúng. Các tính năng 🟢 là điểm cộng ấn tượng nhưng đừng để chúng làm lung lay core functionality. Race condition và quy trình đặt cọc là 2 thứ **giám khảo kỹ thuật thường hỏi** - giải thích được tại sao thiết kế như vậy sẽ ghi điểm rất mạnh.
