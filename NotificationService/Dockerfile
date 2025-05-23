FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY UniversityCourseManagementSystem.sln .
COPY NotificationService/NotificationService.csproj NotificationService/
COPY Shared/Shared.csproj Shared/

RUN dotnet restore NotificationService/NotificationService.csproj

COPY . .

WORKDIR /src/NotificationService
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80

ENTRYPOINT [ "dotnet", "NotificationService.dll" ]