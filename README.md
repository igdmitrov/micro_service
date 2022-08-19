# Microservice Architecture

### Stack:

ASP .NET Web API | RabbitMQ | .NET worker | Supabase Storage | FLUTTER | Docker


#### Deploy and build containers:
```
docker-compose up --build -d
```

#### Stop and remove containers:
```
docker-compose down
```

#### Generate dev cert
```
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p your_password
dotnet dev-certs https --trust
```

add in docker-compose:
```
- ASPNETCORE_Kestrel__Certificates__Default__Password=your_password
- ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
```

### Video on YouTube:
[![My video](https://img.youtube.com/vi/ldJv6K__n3c/0.jpg)](https://youtu.be/ldJv6K__n3c)
