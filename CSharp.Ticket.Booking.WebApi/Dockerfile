#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CSharp.Ticket.Booking.WebApi/CSharp.Ticket.Booking.WebApi.csproj", "CSharp.Ticket.Booking.WebApi/"]
COPY ["CSharp.Ticket.Booking.DataAccess.Database/CSharp.Ticket.Booking.DataAccess.Database.csproj", "CSharp.Ticket.Booking.DataAccess.Database/"]
COPY ["CSharp.Ticket.Booking.DataAccess.InMemoryDatabase/CSharp.Ticket.Booking.DataAccess.InMemoryDatabase.csproj", "CSharp.Ticket.Booking.DataAccess.InMemoryDatabase/"]
COPY ["CSharp.Ticket.Booking.DataAccess/CSharp.Ticket.Booking.DataAccess.csproj", "CSharp.Ticket.Booking.DataAccess/"]
RUN dotnet restore "CSharp.Ticket.Booking.WebApi/CSharp.Ticket.Booking.WebApi.csproj"
COPY . .
WORKDIR "/src/CSharp.Ticket.Booking.WebApi"
RUN dotnet build "CSharp.Ticket.Booking.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CSharp.Ticket.Booking.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CSharp.Ticket.Booking.WebApi.dll"]