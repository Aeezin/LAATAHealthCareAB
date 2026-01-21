# LAATAHealthCareAB

## Getting Started

### Backend

#### Dependencies
- .NET 8 SDK (API)
- .NET 10 SDK (Tests - optional if only running API)
- Docker & Docker Compose

#### Packages

**Runtime**
- DotNetEnv
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.Identity
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.InMemory
- Microsoft.EntityFrameworkCore.Tools
- Npgsql.EntityFrameworkCore.PostgreSQL
- Swashbuckle.AspNetCore
- System.IdentityModel.Tokens.Jwt
**Testing**
- Microsoft.NET.Test.Sdk
- xunit
- xunit.runner.visualstudio
- Moq
- coverlet.collector
- coverlet.msbuild

#### Configuration
Create a `.env` file in the API project directory: `HealthCareAB_v1/HealthCareAB_v1/.env`
Add the following variables (adjust values as needed):
```env
# Auth Token
JWT_SECRET=your-super-secret-key-must-be-at-least-32-characters-long-for-security-reasons
# DB Connection
CONNECTION_STRING=Host=localhost;Port=5432;Database=HCAB01;Username=postgres;Password=change_me_to_a_secure_password
# Docker Database Config
DB_USER=postgres
DB_PASSWORD=change_me_to_a_secure_password
DB_NAME=HCAB01
DB_PORT=5432
```

#### Installing & Running
1. **Start the Database**
   Navigate to the Docker folder and start the PostgreSQL container:
```bash
   cd HealthCareAB_v1/Docker
   docker-compose up -d
```
2. **Setup the API**
   Navigate to the API project folder:
```bash
   cd ../HealthCareAB_v1
```
   Restore dependencies:
```bash
   dotnet restore
```
   Update the database schema:
```bash
   dotnet ef database update
```
3. **Run the Application**
   Start the API:
```bash
   dotnet run
```
4. **Run Tests**
   To run the unit tests:
```bash
   dotnet test
```
### Frontend

#### Dependencies

- Node.js (Latest LTS recommended)

#### Packages
**Core**
- React ^18.3.1
- React DOM ^18.3.1
- Vite ^7.1.12
- Axios ^1.7.7
- React Router DOM ^6.26.2
- Styled Components ^6.1.13

**UI Libraries**
- @mantine/core ^8.3.11
- @mantine/dates ^8.3.11
- @mantine/hooks ^8.3.11
- @tabler/icons-react ^3.36.1

**Development**
- ESLint ^9.11.1
- @vitejs/plugin-react ^4.3.2
- PostCSS ^8.5.6
- postcss-preset-mantine ^1.18.0
- postcss-simple-vars ^7.0.1

#### Installing & Running
1. **Navigate to the client folder**
```bash
   cd medical-app-client
```
2. **Install Dependencies**
```bash
   npm install
```
3. **Start the Development Server**
```bash
   npm run dev
```
   The application will typically start on http://localhost:5173.
### Additional Developer Notes
- **API Documentation & Testing**: A specific Bruno collection is available in the `HealthCareAB_v1/HealthCareAB.Bruno` directory for testing API endpoints.
- **EF Core Tools**: If `dotnet ef` commands fail, ensure you have the tools installed globally: `dotnet tool install --global dotnet-ef`.
