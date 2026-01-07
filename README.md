# LAATAHealthCareAB

## Getting Started

### Backend

#### Dependencies
- **.NET 8 SDK**

#### Packages

**Runtime**
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.Tools
- Npgsql.EntityFrameworkCore.PostgreSQL
- Swashbuckle.AspNetCore
- System.IdentityModel.Tokens.Jwt

**Testing**
- Microsoft.NET.Test.Sdk (17.14.1)
- xunit
- xunit.runner.visualstudio (3.1.4)
- Moq
- coverlet.collector
- coverlet.msbuild

#### Installing

**API**

1. Clone the repository:
   ```bash
   git clone https://github.com/Aeezin/LAATAHealthCareAB.git
   ```

2. Navigate to the backend folder:
   ```bash
   cd HealthCareAB_v1
   ```
   
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
**Database**

1. Install Docker:
- Refer to the docker docs for the docker installation:
  - https://docs.docker.com/get-started/get-docker/

2. Setting Up PostgreSQL Container:
```
docker run --name [containername] \
  -e POSTGRES_PASSWORD=[secretpassword] \
  -e POSTGRES_USER=[username] \
  -e POSTGRES_DB=[database] \
  -p 5432:5432 \
  -d postgres:latest
```
[text] - signifies to use custom input.

3. Setting up database schema:
```
dotnet ef database update
```

#### Running the Program
**Starting the API**
   ```bash
   dotnet run
   ```
**Running the tests**
   ```bash
   dotnet test
   ```

