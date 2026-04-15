# Budgeting and Savings

A .NET 10 backend project Web API designed to help users manage their finances, track saving goals, set budgets, and earn rewards for saving.

## Saving Goals and Rewards Handling

The API employs an intelligent approach to encourage better financial habits through saving goals and a gamified reward system:

- **Saving Goals Tracking**: Users can set specific saving targets with a name, target amount, and deadline. The system monitors transactions tagged with `TransactionCategory.Savings`. The goal's progress is dynamically calculated by summarizing all relevant savings transactions within the goal's timeframe.
- **Automated "Round-Up" Savings**: Every time a user makes a `Debit` (expense) transaction from a Checking account, the API automatically rounds the amount up to the nearest whole unit. The difference is then moved from the Checking account to the user's Savings account as an automated saving transaction.
- **Gamified Rewards**: Rewards are automatically handled after transactions to incentivize positive financial behavior. Users earn points for their first transaction (welcome bonus) and for their first savings contribution each month. Accumulated points can be redeemed for cashback.

## Project Structure

- **BudgetingSavings.API**: The main project containing the Web API.
    - `Controllers/`: Handles incoming HTTP requests and directs them to the appropriate services.
    - `Services/`: Contains the business logic for the application, such as calculations and processing data.
    - `Interfaces/`: Defines the service abstractions for loose coupling and easier testing.
    - `Infrastructure/`:
        - `Data/`: Manages database connections, context, and data initialization.
        - `Migrations/`: Contains Entity Framework Core migration history.
        - `Entities/`: Defines the core data models used by Entity Framework.
        - `Security/`: Custom authentication logic, including API Key handling.
    - `Models/`: Contains Data Transfer Objects (DTOs) for requests and responses, as well as enums.
    - `Validators/`: Contains FluentValidation rules to ensure incoming data is correct.
    - `Middleware/`: Custom logic injected into the ASP.NET Core request pipeline (e.g., exception handling, rate limiting).
- **BudgetingSavings.Tests**: The testing project ensuring high reliability.
    - `UnitTests/`: Contains isolated tests for all business services, including 90+ test cases covering valid flows, validation failures, and complex edge cases (like round-up logic and reward bonuses).

## Technology Stack

- **Framework**: .NET 10
- **Language**: C# 14
- **ORM**: Entity Framework Core (In-Memory for development/testing)
- **Validation**: FluentValidation
- **Patterns**: Result Pattern for functional-style error handling and flow control.
- **Error Handling**: Custom `ExceptionHandlingMiddleware` using `ProblemDetails`
- **Tests**: xUnit, NSubstitute (90+ Unit Tests)

## Result Pattern

The application uses a centralized `Result` and `Result<T>` pattern for business logic flow control. This ensures that:
- Service methods explicitly return success or failure states.
- Failure reasons are clearly propagated without relying solely on exceptions.
- Domain logic is kept clean and predictable.

## Error Handling

The API features a centralized error-handling mechanism via `ExceptionHandlingMiddleware`. This ensures that all errors are returned in a consistent, machine-readable format using the `ProblemDetails` standard (RFC 7807).

Key features:
- **Validation Errors**: Catching `ValidationException` from `FluentValidation` and returning a `400 Bad Request` with a detailed list of validation failures.
- **Unexpected Failures**: Catching all other exceptions and returning a `500 Internal Server Error` with a generic message to avoid leaking sensitive system information.

## Rate Limiting

The API implements rate limiting to ensure stability and prevent abuse:

- **Policy**: `fixedRateLimiter` (Fixed Window).
- **Limit**: 20 requests per minute.
- **Queueing**: Disabled (requests are rejected immediately when the limit is reached).

## Security & Authentication

The API uses API Key authentication to secure its endpoints:
- **Development key**: `@MySuperSecretDevKey123`
- **Header Name**: `X-API-Key`
- **Configuration**: The API key is stored in `appsettings.json` under the `Security:ApiKey` path.
- **Local Development Override**: You can override the default API key by using .NET User Secrets.
- **Example Usage**: Include the key in your HTTP request headers: `X-API-Key: @MySuperSecretDevKey123`.

## Services
1.  **CustomerService**: Manages customer profiles and information.
2.  **AccountService**: Handles account creation, retrieval, and management.
3.  **TransactionService**: Manages transactions (debits, credits, transfers).
4.  **SavingGoalService**: Manages saving goals, progress tracking, and suggestions.
5.  **BudgetService**: Manages budgets, spending tracking, and notifications.
6.  **RewardService**: Manages rewards, points calculation, and redemption.

## Entities and Relationships

### Entities

1.  **Customer**: The primary user of the application.
    - Properties: `Name`, `Email`, `PhoneNumber`, `DateOfBirth`.
2.  **Account**: A financial account (Checking or Savings) owned by a customer.
    - Properties: `AccountNumber`, `AccountType`, `Balance`, `Currency`, `CreatedDate`.
3.  **Transaction**: A record of monetary movement (Debit/Credit).
    - Properties: `TransactionDateTime`, `Amount`, `Currency`, `TransactionType`, `TransactionCategory`.
4.  **SavingGoal**: A target for saving money towards a specific purpose.
    - Properties: `Name`, `TargetAmount`, `StartDate`, `TargetDate`.
5.  **Budget**: A spending limit set for a specific timeframe.
    - Properties: `StartTime`, `EndTime`, `LimitAmount`, `Currency`.
6.  **Reward**: Points earned through positive financial habits (like saving).
    - Properties: `Date`, `Amount`, `Points`, `Redeemed`, `CashBack`, `RedeemedDate`.

### Relationships

- A **Customer** has a one-to-many relationship with **Accounts**, **Transactions**, **SavingGoals**, **Budgets**, and **Rewards**.
- An **Account** is owned by one **Customer** and contains many **Transactions**.
- A **Transaction** is linked to both a **Customer** and an **Account**.
- **SavingGoals**, **Budgets**, and **Rewards** are directly associated with a **Customer**.

## API Endpoints

### Customers (`/api/customers`)
- `GET /api/customers`: Retrieve all customers.
- `GET /api/customers/{id}`: Retrieve a specific customer.
- `POST /api/customers`: Create a new customer.
- `PUT /api/customers`: Update an existing customer.
- `DELETE /api/customers/{id}`: Delete a customer.

### Accounts (`/api/accounts`)
- `GET /api/accounts`: Retrieve all accounts.
- `GET /api/accounts/{id}`: Retrieve a specific account.
- `GET /api/accounts/customer/{customerId}`: Retrieve all accounts for a specific customer.
- `POST /api/accounts`: Create a new account.
- `DELETE /api/accounts/{id}`: Delete an account.

### Transactions (`/api/transactions`)
- `GET /api/transactions/{id}`: Retrieve a specific transaction.
- `GET /api/transactions/account/{accountId}`: Retrieve all transactions for an account.
- `POST /api/transactions`: Create a new transaction (Debit/Credit).
- `POST /api/transactions/Transfer`: Create a new transfer between two accounts.

### Saving Goals (`/api/saving-goals`)
- `GET /api/saving-goals/{id}`: Retrieve a specific saving goal.
- `GET /api/saving-goals/customer/{customerId}`: Retrieve all saving goals for a customer.
- `GET /api/saving-goals/{id}/status`: Get progress and status of a saving goal.
- `GET /api/saving-goals/customer/{customerId}/suggestions`: Get saving suggestions based on spending patterns.
- `POST /api/saving-goals`: Create a saving goal.
- `PUT /api/saving-goals`: Update a saving goal.
- `DELETE /api/saving-goals/{id}`: Delete a saving goal.

### Budgets (`/api/budgets`)
- `GET /api/budgets/{id}`: Retrieve a specific budget.
- `GET /api/budgets/customer/{customerId}`: Retrieve all budgets for a customer.
- `GET /api/budgets/{id}/status`: Get current spending status against the budget.
- `POST /api/budgets`: Create a new budget.
- `PUT /api/budgets`: Update a budget.
- `DELETE /api/budgets/{id}`: Delete a budget.

### Rewards (`/api/rewards`)
- `GET /api/rewards/{id}`: Retrieve a specific reward.
- `GET /api/rewards/customer/{customerId}`: Retrieve all rewards for a customer.
- `POST /api/rewards/redeem`: Redeem earned points for cashback.

## Database Initialization

The project includes a `DbInitializer` that automatically seeds the database with sample data if it's empty when the application starts. This includes customers, accounts, initial transactions, goals, budgets, and rewards.

## Future Improvements

- Add JWT-based authentication and authorization using Azure Entra ID, deriving customer identity from the authenticated user instead of request payloads.
- Expand business validation and integrate with external APIs for currency conversion.
- Add integration tests and end-to-end (E2E) tests.
- Integrate with an open banking provider to import real transactions and balances.
- Refine the reward system into a more explicit points ledger and reward history model.
- Improve production readiness with structured logging, health checks, and a production-grade database.
- Use Azure Key Vault or similar secret vaults to securely store application settings and keys in Azure.
- Eventually integrate a message bus (e.g., Azure Service Bus) to queue requests and handle background processing.

## Getting Started

1.  Clone the repository.
2.  Ensure you have the .NET 10 SDK installed.
3.  Run the application: `dotnet run --project BudgetingSavings.API`.
4.  Access the Swagger UI (usually at `https://localhost:{port}/swagger`) to explore the API.
5.  Use the development key in Swagger: 
    - Click the **Authorize** button.
    - Enter `@MySuperSecretDevKey123` in the value field for the `ApiKey` (X-API-Key) definition.
    - Click **Authorize** and then **Close**.
    - You can now test the endpoints.

## Running Tests

The solution includes a comprehensive set of unit tests (90+) covering all business services.

1.  To run all tests: `dotnet test`.
2.  To run specific project tests: `dotnet test BudgetingSavings.Tests/BudgetingSavings.Tests.csproj`.
3.  Tests use `InMemoryDatabase` to avoid dependencies on external databases during the run.
