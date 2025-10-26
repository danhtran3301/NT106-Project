# NT106 - Lập trình mạng căn bản
## 👥 Danh sách thành viên nhóm 10    
	- 24521726	Lương Thị Anh Thư  
	- 24521674	Lê Văn Thiết  
	- 24521655	Phùng Ngọc Thi  
	- 24521654	Nguyễn Thị Trang Thi  
	- 22520200	Trần Ngọc Danh  

**Bài tập tuần 5 - Viết ứng dụng quản lý người dùng với tính năng đăng nhập, đăng ký với TCP socket**  
  
## 📝 Mô tả
Ứng dụng C# Windows Forms gồm hai chức năng chính:
- **Đăng ký người dùng**: thêm thông tin vào cơ sở dữ liệu SQL Server
- **Đăng nhập**: kiểm tra thông tin đăng nhập thông qua dữ liệu đã lưu trong bảng `Users`

## 🛠️ Cơ sở dữ liệu

Ứng dụng sử dụng SQL Server. Để thuận tiện cho quá trình chấm bài, nhóm chúng em đã chuẩn bị sẵn file:

- 📁 `database.sql`  
  → File này chứa toàn bộ lệnh tạo cơ sở dữ liệu `UserDB`, tạo bảng `Users`, tạo sẵn user tên myuser để phục vụ cho việc kết nối tới database.
- Cách sử dụng:  
  1. Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio và kết nối tới SQL Server instance của bạn (có thể sẽ yêu cầu đăng nhập bằng tài khoản 'sa').
  2. Mở file `database.sql` trong SSMS.
  3. Chạy toàn bộ script để tạo cơ sở dữ liệu, user và bảng cần thiết.

## 🔐 Thông tin kết nối mặc định

Trong code có sử dụng chuỗi kết nối:

Server=localhost;Database=UserDB;User Id=myuser;Password=YourStrong@Passw0rd;


