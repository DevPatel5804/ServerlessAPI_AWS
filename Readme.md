# ASP.NET Core Web API Serverless Application

This project shows how to run an ASP.NET Core Web API project as an AWS Lambda exposed through Amazon API Gateway. The NuGet package [Amazon.Lambda.AspNetCoreServer](https://www.nuget.org/packages/Amazon.Lambda.AspNetCoreServer) contains a Lambda function that is used to translate requests from API Gateway into the ASP.NET Core framework and then the responses from ASP.NET Core back to API Gateway.


For more information about how the Amazon.Lambda.AspNetCoreServer package works and how to extend its behavior view its [README](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.AspNetCoreServer/README.md) file in GitHub.


### Configuring for API Gateway HTTP API ###

API Gateway supports the original REST API and the new HTTP API. In addition HTTP API supports 2 different
payload formats. When using the 2.0 format the base class of `LambdaEntryPoint` must be `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`.
For the 1.0 payload format the base class is the same as REST API which is `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction`.
**Note:** when using the `AWS::Serverless::Function` CloudFormation resource with an event type of `HttpApi` the default payload
format is 2.0 so the base class of `LambdaEntryPoint` must be `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`.


### Configuring for Application Load Balancer ###

To configure this project to handle requests from an Application Load Balancer instead of API Gateway change
the base class of `LambdaEntryPoint` from `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction` to 
`Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction`.

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "serverless-auth/test/serverless-auth.Tests"
    dotnet test
```

Deploy application
```
    cd "serverless-auth/src/serverless-auth"
    dotnet lambda deploy-serverless
```

# Serverless Auth API

A secure, serverless authentication API built with **ASP.NET Core Web API** and deployed on **AWS Lambda** using **Amazon API Gateway** and **DynamoDB**. This project provides JWT-based authentication, user management, and API key protection for modern serverless applications.

---

## Features

- **Serverless Deployment**: Runs on AWS Lambda, managed via CloudFormation ([serverless.template](serverless.template)).
- **User Authentication**: Secure login with JWT access and refresh tokens.
- **User Management**: Add or update users with hashed passwords.
- **API Key Protection**: All endpoints require a valid `X-API-KEY` header.
- **DynamoDB Integration**: User data is stored in DynamoDB for scalability and reliability.
- **Configurable Security**: Supports max failed login attempts, account lockout, and more.
- **CORS Support**: Configured for cross-origin requests.
- **Environment-based Configuration**: Uses `appsettings.json` for secrets and environment settings.
- **Extensible**: Easily add more endpoints or integrate with other AWS services.

---

## Architecture

```mermaid
graph TD
    Client-->|HTTP/HTTPS|APIGateway
    APIGateway-->|Lambda Proxy|Lambda[Lambda (ASP.NET Core)]
    Lambda-->|DynamoDBContext|DynamoDB[(DynamoDB Table)]
```

- **API Gateway**: Exposes REST endpoints and handles API key validation.
- **AWS Lambda**: Hosts the ASP.NET Core Web API, scaling automatically.
- **DynamoDB**: Stores user credentials and metadata.

---

## Project Structure

- [`Controllers/`](Controllers/)  
  - [`AuthController.cs`](Controllers/AuthController.cs): Handles login/authentication.
  - [`UserController.cs`](Controllers/UserController.cs): Handles user creation and updates.
- [`BusinessObjects/`](BusinessObjects/)  
  - [`UserBusinessObject.cs`](BusinessObjects/UserBusinessObject.cs): Core business logic for users.
- [`Utilities/`](Utilities/)  
  - [`JWTServiceUtilities.cs`](Utilities/JWTServiceUtilities.cs): JWT token generation and validation.
  - [`XApiKeyUtilities.cs`](Utilities/XApiKeyUtilities.cs): API key validation filter.
- [`ViewModels/`](ViewModels/)  
  - Data models for requests and responses.
- [`Startup.cs`](Startup.cs): Service and middleware configuration.
- [`LambdaEntryPoint.cs`](LambdaEntryPoint.cs): Lambda bootstrapper.
- [`LocalEntryPoint.cs`](LocalEntryPoint.cs): Local Kestrel bootstrapper.
- [`appsettings.json`](appsettings.json): Configuration for AWS, JWT, API keys, etc.

---

## API Endpoints

All endpoints require the `X-API-KEY` header.

### `POST /jwt-auth/api/auth/login`

Authenticate a user and receive JWT tokens.

**Request Body:**
```json
{
  "ApplicationID": "string",
  "Email": "user@example.com",
  "Password": "password"
}
```

**Response:**
```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "email": "user@example.com",
  "applicationID": "string",
  "expiresIn": 900
}
```

---

### `POST /jwt-auth/api/user/add`

Add or update a user.

**Request Body:**
```json
{
  "ApplicationID": "string",
  "Email": "user@example.com",
  "Password": "password",
  "IsActive": true,
  "IsLocked": false,
  "IsEnabled": true
}
```

**Response:**
```json
{
  "message": "User added/updated successfully.",
  "email": "user@example.com",
  "applicationId": "string"
}
```

---

## Environment Variables & Configuration

All sensitive settings are managed via `appsettings.json` or environment variables:

| Key                                 | Description                        |
|--------------------------------------|------------------------------------|
| `JwtSettings:Secret`                 | JWT signing secret                 |
| `JwtSettings:Issuer`                 | JWT issuer                         |
| `JwtSettings:Audience`               | JWT audience                       |
| `ApiKeys:XApiKey`                    | API key for endpoint protection    |
| `SecuritySettings:MaxFailedLoginAttempts` | Max failed login attempts before lockout |
| `AWS Region`                         | AWS region for DynamoDB            |

---

## Error Handling

- **401 Unauthorized**: Invalid credentials or API key.
- **403 Forbidden**: Account locked or disabled.
- **400 Bad Request**: Validation errors.
- **500 Internal Server Error**: Unexpected server errors.

All error responses include a `Message` field for clarity.

---

## Extensibility

- Add new endpoints by creating controllers in `Controllers/`.
- Add new business logic in `BusinessObjects/`.
- Integrate with other AWS services (SNS, SQS, etc.) as needed.
- Customize JWT claims or token lifetimes in `JWTServiceUtilities.cs`.

---

## Security

- Passwords are hashed using BCrypt.
- JWT tokens are signed with a strong secret.
- API endpoints are protected by API key and JWT authentication.
- Account lockout after configurable failed login attempts.
- Sensitive configuration is never hardcoded.

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- AWS CLI configured
- DynamoDB table `auth-users` created

### Local Development

```sh
dotnet restore
dotnet build
dotnet run
```

### Deploy to AWS Lambda

```sh
dotnet tool install -g Amazon.Lambda.Tools
dotnet lambda deploy-serverless
```

---

## Contribution Guidelines

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/your-feature`).
3. Commit your changes.
4. Push to your branch.
5. Open a Pull Request.

---

## License

MIT

---

## Author

[Dev Patel]
[devp9137@gmail.com OR www.linkedin.com/in/devpatel9137]