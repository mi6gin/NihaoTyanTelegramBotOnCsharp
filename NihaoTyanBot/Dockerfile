# Этап сборки: используем SDK для восстановления, сборки и публикации приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем файл проекта и восстанавливаем зависимости
COPY NihaoTyanBot.csproj .
RUN dotnet restore NihaoTyanBot.csproj

# Копируем все исходники и выполняем сборку
COPY . .
RUN dotnet build NihaoTyanBot.csproj -c Release -o /app/build
RUN chmod +x /app/DreamGirl/commandsList/userCommands/YtDlp/Downloader/Main/Utilities/yt-dlp_linux

# Этап публикации: формируем оптимизированную сборку
FROM build AS publish
RUN dotnet publish NihaoTyanBot.csproj -c Release -o /app/publish /p:UseAppHost=false

# Финальный этап: используем минимальный образ ASP.NET для выполнения приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Установка Python3 в финальном образе
RUN apt-get update && apt-get install -y python3 python3-pip && apt-get clean

# Копируем опубликованные файлы
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NihaoTyanBot.dll"]
