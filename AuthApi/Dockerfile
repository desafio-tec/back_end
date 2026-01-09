# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências
# Usamos caminhos relativos para o Docker encontrar o arquivo na mesma pasta
COPY AuthApi.csproj .
RUN dotnet restore AuthApi.csproj

# Copia todo o resto do código da pasta AuthApi e compila
COPY . .
RUN dotnet publish AuthApi.csproj -c Release -o /app/publish

# Estágio Final (Imagem menor para produção)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Variável de ambiente para expor a porta correta no Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# O nome da DLL deve ser o mesmo do seu projeto
ENTRYPOINT ["dotnet", "AuthApi.dll"]