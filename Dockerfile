FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ./JumpenoWebassembly/Client/JumpenoWebassembly.Client.csproj ./JumpenoWebassembly/Client/
COPY ./JumpenoWebassembly/Server/JumpenoWebassembly.Server.csproj ./JumpenoWebassembly/Server/
COPY ./JumpenoWebassembly/Shared/JumpenoWebassembly.Shared.csproj ./JumpenoWebassembly/Shared/
COPY ./JumpenoWebassembly.sln ./
RUN dotnet restore ./JumpenoWebassembly.sln
COPY ./JumpenoWebassembly/Shared ./JumpenoWebassembly/Shared
COPY ./JumpenoWebassembly/Server ./JumpenoWebassembly/Server
COPY ./JumpenoWebassembly/Client ./JumpenoWebassembly/Client
RUN dotnet publish JumpenoWebassembly/Server/JumpenoWebassembly.Server.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "JumpenoWebassembly.Server.dll"]