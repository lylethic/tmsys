# Sử dụng image cơ bản của .NET SDK để build dự án
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Sao chép tệp .csproj và khôi phục các gói NuGet
COPY *.csproj ./
RUN dotnet restore

# Sao chép toàn bộ mã nguồn và build dự án
COPY . ./
RUN dotnet publish -c Release -o out

# Sử dụng image runtime để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
COPY --from=build /app/wwwroot ./wwwroot

# Mở cổng 80
EXPOSE 80
# Đảm bảo biến môi trường được đọc từ .env khi chạy
ENV ASPNETCORE_ENVIRONMENT=Development

# Lệnh khởi chạy ứng dụng
ENTRYPOINT ["dotnet", "server.dll"]