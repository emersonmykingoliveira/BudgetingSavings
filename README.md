# Budgeting and Savings

A .NET 10 backend project Web API designed to help users manage their finances, track saving goals, set budgets, and earn rewards for saving.

## Project Structure

- **BudgetingSavings.API**: The core Web API project containing controllers, services, and infrastructure.
- **BudgetingSavings.Tests**: A comprehensive suite of unit tests using xUnit and NSubstitute.

## Technology Stack

- **Framework**: .NET 10
- **Language**: C# 14
- **ORM**: Entity Framework Core
- **Validation**: FluentValidation, ExceptionHandlingMiddleware
- **Tests**: xUnit, NSubstitute, FluentAssertions

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
- `POST /api/transactions`: Create a new transaction (Debit/Credit/Transfer).

### Saving Goals (`/api/saving-goals`)
- `GET /api/saving-goals/{id}`: Retrieve a specific saving goal.
- `GET /api/saving-goals/customer/{customerId}`: Retrieve all saving goals for a customer.
- `GET /api/saving-goals/{id}/status`: Get progress and status of a saving goal.
- `GET /api/saving-goals/customer/{customerId}/suggestions`: Get saving suggestions based on spending patterns.
- `POST /api/saving-goals`: Create a saving goal.
- `PUT /api/saving-goals`: Update a saving goal.

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

## Getting Started

1.  Clone the repository.
2.  Ensure you have the .NET 10 SDK installed.
3.  Configure your connection string in `appsettings.json`. The default is set to use Local SQLite DB, but you can switch to SQL Server or another provider if needed.
4.  Run the application: `dotnet run --project BudgetingSavings.API`.
5.  Access the Swagger UI (usually at `https://localhost:{port}/swagger`) to explore the API.
