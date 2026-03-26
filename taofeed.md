> **Your request in English:**
> "Currently, I have created a new user who has the following room and device. For the Postman request: `POST http://localhost:5189/api/rooms/9d6cda06-9154-4894-a4cf-848d727e5983/devices` with body `{"deviceName": "light-1", "type": "OUTPUT", "thresholdMin": 10, "thresholdMax": 100}`. The output returned a device with `"feedKey": "phong-khach-2-light-1"`. What do I need to do to control the `light-1` feed on Adafruit? I don't see anything in that feed; the link is `https://io.adafruit.com/DMH1805/feeds/light-1` and it displays as shown in the image."

Chào Nhật, nhìn vào hình ảnh và dữ liệu bạn gửi, tôi đã phát hiện ra **2 điểm mấu chốt** khiến bạn chưa điều khiển được và thấy màn hình trống trơn:

**1. Lệch Feed Key (Rất quan trọng):**
Hệ thống Backend của bạn đã tự động sinh ra mã `feedKey` là `"phong-khach-2-light-1"`. Điều này có nghĩa là khi gọi API điều khiển, Backend sẽ gửi lệnh MQTT vào topic: `DMH1805/feeds/phong-khach-2-light-1`.
Tuy nhiên, bạn lại đang xem đường link của feed là `.../feeds/light-1`. Tức là Backend đang bắn lệnh vào một nơi, còn bạn lại đang đứng chờ ở một nơi khác.

**2. Đang ở sai màn hình:**
Hình ảnh bạn gửi là giao diện **Feeds** (chỉ dùng để vẽ biểu đồ và xem lịch sử dữ liệu thô). Để có nút bấm Bật/Tắt, bạn phải sang màn hình **Dashboards**.

Dưới đây là 3 bước bạn cần làm ngay để điều khiển thành công:

### Bước 1: Sửa Key trên Adafruit cho khớp Backend
1. Tại màn hình bạn đang chụp, nhìn sang cột bên phải (Feed Info), nhấn vào biểu tượng bánh răng ⚙️ (hoặc nhấn trực tiếp vào chữ `light-1` ở phần url/tên nếu nó cho sửa).
2. Tốt nhất là bạn vào lại tab **Feeds** -> tạo một Feed mới.
3. Ở ô **Name**, bạn nhập gì cũng được (ví dụ: `Đèn Phòng Khách 2`).
4. Ở ô **Key**, **bắt buộc** nhập chính xác: `phong-khach-2-light-1` -> Nhấn Create.

### Bước 2: Tạo nút bấm (Toggle) trên Dashboard
1. Nhìn lên menu màu đen trên cùng, chọn **Dashboards**.
2. Nhấn **+ New Dashboard** (Ví dụ đặt tên là `Smart Home`).
3. Click vào Dashboard vừa tạo -> Nhấn biểu tượng bánh răng ⚙️ góc phải trên -> Chọn **Create New Block**.
4. Chọn khối **Toggle** (hình công tắc).
5. Tick chọn cái Feed bạn vừa làm ở Bước 1 (`phong-khach-2-light-1`) -> Nhấn **Next step**.
6. Cấu hình giá trị:
   * Button On Text/Value: Điền `1`
   * Button Off Text/Value: Điền `0`
7. Nhấn **Create block**.

### Bước 3: Test trên Postman
Mở Postman, gọi API để điều khiển chính cái ID thiết bị bạn vừa tạo ở trên:
* **Method:** `POST`
* **URL:** `http://localhost:5189/api/devices/229f024e-804e-4bd7-830e-251611fbc5b4/control`
* **Body (JSON):**
```json
{
  "status": "ON"
}
```
* **Send!**
