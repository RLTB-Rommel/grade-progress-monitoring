# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GradeProgressMonitoring.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


# ---------- Run ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# 🔧 Install missing Kerberos library (fixes libgssapi_krb5.so.2 error)
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Render provides PORT; bind to 0.0.0.0
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

CMD ["dotnet", "GradeProgressMonitoring.dll"]