# 1. User Interface & Frontend (Next.js + Zustand + React Query)

Quá trình diễn ra trên Web Client của người dùng tại trang `/kyc`.

- **Bước 1.1: Authentication & Status Check**
    - Hệ thống kiểm tra JWT token. Gọi API `GET /api/auth/me` để lấy `KycStatus` hiện tại (thông qua React Query).
    - Tùy vào kết quả `KycStatus`, giao diện sẽ tự chuyển hướng trạng thái:
        - `Pending`: Chờ duyệt.
        - `Verified`: Đã duyệt (Hiện nút Mua hàng).
        - `Rejected`: Bị từ chối (Hiện nút Quét lại).
        - `None`: Render Form uploard eKYC.
- **Bước 1.2: Thu thập thông tin Giấy tờ**
    - Người dùng Upload ảnh: Mặt trước (ID Front) & Mặt sau (ID Back). File được kiểm tra dung lượng và preview trực tiếp ở client.
- **Bước 1.3: Face Liveness Detection (Chống giả mạo)**
    - Tích hợp `face-api.js` phân tích mốc khuôn mặt (Landmarks 68 điểm) trên Local Browser realtime qua Webcam để giải quyết bài toán Liveness.
    - Áp dụng **quy trình lấy mẫu tĩnh Sequential Flow (Trái -> Phải -> Giữa)**:
        1. Yêu cầu quay nhẹ sang TRÁI (Tỷ lệ phân tán mắt trái cao -> OK)
        2. Yêu cầu quay nhẹ sang PHẢI (Tỷ lệ phân tán mắt phải cao -> OK)
        3. Yêu cầu nhìn THẲNG trung tâm (Cân bằng khoảng cách 2 mắt).
    - Ngay khi người dùng hoàn thiện đúng chuỗi hành động và đạt trạng thái nhìn thẳng trung tâm hợp lệ, hệ thống sẽ tự động (Auto-Capture) sau `500ms` delay để lấy bức ảnh mặt (Face) nét chuẩn nhất.
- **Bước 1.4: Push Data**
    - Đóng gói (Append) hình ảnh `IdCardFront`, `IdCardBack`, `FacePhoto` với định dạng `FormData` để POST sang Backend API REST.

---

# 2. Business Layer (ASP.NET Core Web API)

API Endpoint: `POST /api/auth/upload-kyc`.

- **Bước 2.1: Authentication Context**
    - Intercept request, xác thực JWT và bóc tách `Email` & `UserId` dựa trên Claims.
- **Bước 2.2: Lên mây (Cloudinary)**
    - Gọi Service xử lý ảnh bất đồng bộ (Async). Tải 3 bức ảnh nguyên bản đẩy lên hệ thống quản trị file media Cloudinary.
    - Lưu trữ Security Public Ids & URLs trả về.
- **Bước 2.3: System State Manager (MySQL)**
    - Cập nhật thông tin User: Liên kết các đường dẫn URL và tự động chuyển `KycStatus = KycStatus.Pending`.
- **Bước 2.4: Message Publisher (RabbitMQ)**
    - Khởi tạo Message DTO `SendKycStatusMessage(Email, FullName, Status)`.
    - Service đóng vai trò Publisher đẩy Event vào Direct Exchange của RabbitMQ thông qua Routing Key `kyc-email-routing-key`.
    - Đảm bảo API Response được trả về nhanh nhất cho Client (Non-blocking I/O).

---

# 3. Asynchronous Worker Service (RabbitMQ Consumer + Smtp)

Một Application Worker (.NET) hoàn toàn độc lập, listen liên tục quá trình ở Queue.

- **Bước 3.1: Message Subscription**
    - Nhận event và thực hiện Deserialize gói Event message từ RabbitMQ.
- **Bước 3.2: Dynamic Template Resolution**
    - Phân tích `KycStatus` bên trong Message payload để inject Template thích hợp:
        - `Verified`: Template màu xanh, lời chúc trải nghiệm dịch vụ.
        - `Pending`: Template màu vàng cam, thư thông báo đã ghi nhận và đang xếp hàng chờ kiểm duyệt AI/Admin.
        - `Rejected`: Template màu đỏ, báo lỗi (nguyên nhân do mờ hoặc thao tác live-ness hỏng), kèm CTA thao tác quét lại.
        - Mọi email đều Render dưới form HTML/CSS Responsive để tạo Trust cho thương hiệu Yash Gems.
- **Bước 3.3: Mail Dispatcher (MailKit)**
    - Giao tiếp SMTPS tới Provider (Gmail) và đẩy thông báo đi ra môi trường mạng lưới, ngay sau đó chốt hoàn thành ACK vào hệ thống MQ.

---

# 4. AI Core Verification & Quyết định trạng thái (Tự động hóa)

Đây là quá trình hệ thống AI Core (Ví dụ: AWS Rekognition) vào cuộc ngay sau khi file được đẩy lên đám mây thành công ở Bước 2. Hệ thống sẽ tiến hành Compare Face (Đối chiếu khuôn mặt).

- **Kiểm tra và Chấm điểm (Matching Score)**
    - AI trích xuất khuôn mặt từ `IdCardFront` (Ảnh thẻ CMND) và đem so sánh với `FacePhoto` (Ảnh Liveness vừa chụp được webcam giữ lại).
    - AI trả về chỉ số giống nhau (Similarity Score).
- **Quy tắc ra quyết định tự động chuyển đổi `KycStatus`:**
    - `Verified`: Nếu tỉ lệ khớp khuôn mặt **> 80%** và ảnh rõ nét. AI tự động đổi trạng thái DB thành `Verified`.
    - `Rejected`: Nếu tỉ lệ **< 60%** (Sai người, không khớp) hoặc phát hiện ảnh CMND bị lóa sáng/che khuất. AI đổi trạng thái thành `Rejected`.
    - `Pending`: Nếu tỉ lệ nằm ở mức lấp lửng **(60% - 80%)** hoặc AI nghi ngờ (ví dụ: ảnh cũ quá, hoặc quá mờ). AI sẽ giữ nguyên trạng thái `Pending` và tạo một cờ (Flag) đẩy hồ sơ này vào hàng đợi để **nhân viên Admin** duyệt tay thủ công.
- **Trigger Notification (Vòng lặp tương tác)**
    - Ngay khoảnh khắc AI (hoặc Admin) chuyển từ Pending sang `Verified` hoặc `Rejected`, Backend sẽ lập tức tái kích hoạt **Bước 2.4**. 
    - Một thông báo sẽ lại được đẩy vào **RabbitMQ**, Worker sẽ nhặt lấy và gửi Email ngay tức khắc thông báo kết quả cuối cùng cho người dùng (Khép kín vòng đời eKYC).
