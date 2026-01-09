# --- ESTÁGIO 1: COMPILAÇÃO (BUILD) ---
# Usa a imagem do SDK do .NET 9 (pesada, contém todas as ferramentas de build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Define a pasta de trabalho dentro do container onde o código será compilado
WORKDIR /src

# Copia apenas o arquivo de projeto (.csproj) para o container
# Fazemos isso primeiro para aproveitar o cache do Docker e acelerar builds futuros
COPY AuthApi.csproj .

# Baixa todas as bibliotecas e dependências (NuGet) necessárias para o projeto
RUN dotnet restore AuthApi.csproj

# Copia todos os outros arquivos da sua pasta AuthApi para dentro do container
COPY . .

# Compila o projeto em modo "Release" (otimizado) e joga os arquivos prontos na pasta /app/publish
RUN dotnet publish AuthApi.csproj -c Release -o /app/publish

# --- ESTÁGIO 2: EXECUÇÃO (RUNTIME) ---
# Usa uma imagem muito mais leve que contém apenas o necessário para rodar o .NET (sem ferramentas de build)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
# Define a pasta onde a aplicação vai morar em produção
WORKDIR /app

# Copia apenas os arquivos compilados (o resultado final) do estágio anterior (build)
COPY --from=build /app/publish .

# --- CONFIGURAÇÕES DE REDE PARA O RENDER ---
# Informa ao .NET que ele deve escutar na porta 8080 (padrão esperado pelo Render)
ENV ASPNETCORE_URLS=http://+:8080
# Documenta que o container escuta na porta 8080
EXPOSE 8080

# Comando que inicia a aplicação. 
# Importante: "AuthApi.dll" deve ser o nome exato do arquivo gerado pelo seu projeto.
ENTRYPOINT ["dotnet", "AuthApi.dll"]