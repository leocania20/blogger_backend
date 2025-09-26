# Imagem SDK .NET 8.0 para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar csproj e restaurar dependências
COPY *.csproj ./
RUN dotnet restore

# Copiar todo o código e publicar
COPY . ./
RUN dotnet publish -c Release -o out

# Imagem runtime .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expor portas
EXPOSE 5000
EXPOSE 5001

# Comando para rodar a aplicação
ENTRYPOINT ["dotnet", "blogger_backend.dll"]
