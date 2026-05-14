FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

RUN apt-get update && apt-get install -y locales && \
    sed -i -e 's/# ru_RU.UTF-8 UTF-8/ru_RU.UTF-8 UTF-8/' /etc/locale.gen && \
    locale-gen
ENV LANG ru_RU.UTF-8
ENV LC_ALL ru_RU.UTF-8

WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["Planeta35_Api/Planeta35_Api.csproj", "Planeta35_Api/"]
COPY ["CourseProgect_Planeta35/CourseProgect_Planeta35.csproj", "CourseProgect_Planeta35/"]

RUN dotnet restore "Planeta35_Api/Planeta35_Api.csproj"

COPY . .

WORKDIR "/src/Planeta35_Api"

RUN dotnet publish "Planeta35_Api.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Planeta35_Api.dll"]