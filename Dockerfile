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

# ✅ Fix missing native libs (Kerberos / GSSAPI)
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# (Optional but helps Render detect the port)
EXPOSE 10000

# Render provides PORT; bind to 0.0.0.0
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

CMD ["dotnet", "GradeProgressMonitoring.dll"]