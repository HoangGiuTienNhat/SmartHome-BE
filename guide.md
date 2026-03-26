# Thứ tự chạy migration
## 1. ở root (backend) : dotnet ef migrations add SeedData --project src/SmartHome.Infrastructure --startup-project src/SmartHome.API


| Cách                | Khi dùng        |
| ------------------- | --------------- |
| `--startup-project` | Dev nhanh       | -> của file ở chỗ data/SmartHomeDbContext.cs
| Factory             | Production / CI |

ở chỗ data/SmartHomeDbContext.cs là dev nhanh 
ở chỗ data/SmartHomeDbContextFactory.cs là factory ( chưa kiểm chứng nhưng cứ tạo file mới này v, có thể xóa file này , còn nếu dùng nó để chạy seed thì : dotnet ef migrations add SeedData --project src/SmartHome.Infrastructure )

## 2 . ở root chạy ni để update seed: 
dotnet ef database update --project src/SmartHome.Infrastructure --startup-project src/SmartHome.API

# chạy API
cd src/SmartHome.API
dotnet run


Now listening on: https://localhost:7096
Now listening on: http://localhost:5189
cái nào thì + swagger cái đó


Chạy HTTPS:
dotnet run --launch-profile https

👉 Swagger:

https://localhost:7096/swagger
🔹 Chạy HTTP:
dotnet run --launch-profile http

👉 Swagger:

http://localhost:5189/swagger



cập nhật seed data mới:
PS E:\bku\252\DADN\backend> dotnet ef migrations add SeedData --project src/SmartHome.Infrastructure --startup-project src/SmartHome.API
PS E:\bku\252\DADN\backend\src\SmartHome.Infrastructure> dotnet ef database update --startup-project ../SmartHome.API