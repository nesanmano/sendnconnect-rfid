# SendNConnect RFID API

SendNConnect — a production-grade, multi-tenant ASP.NET Core 8 Web API for RFID/NFC card lifecycle, stored-value wallet, and transaction management. Designed as a **horizontal SaaS platform** — the same tables serve arcades, universities, hospitals, corporate offices, transit systems, theme parks, and financial simulations.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    API Gateway / CORS                     │
├─────────────────────────────────────────────────────────┤
│  ExceptionHandlingMiddleware                             │
│  Authentication (Zitadel JWT / Dev Bypass)                │
│  TenantMiddleware (org_id extraction)                    │
├─────────────────────────────────────────────────────────┤
│  Controllers                                             │
│  ┌──────────┬──────────┬──────────┬──────────────────┐  │
│  │  Cards   │ Wallets  │  Txns    │ CardHolders      │  │
│  │          │          │          │ Merchants         │  │
│  │          │          │          │ CustomFields      │  │
│  │          │          │          │ TenantSettings    │  │
│  └──────────┴──────────┴──────────┴──────────────────┘  │
├─────────────────────────────────────────────────────────┤
│  Services (business logic + validation)                  │
├─────────────────────────────────────────────────────────┤
│  EF Core DbContext (global tenant query filters)         │
├──────────────────┬──────────────────────────────────────┤
│  MySQL 8.0       │  Kafka (CloudEvents 1.0)              │
│  (15 tables)     │  (card-events, wallet-events)         │
└──────────────────┴──────────────────────────────────────┘
```

## Key Features

- **Multi-tenant isolation** via `org_id` on every table with EF Core global query filters
- **Zitadel JWT authentication** with dev bypass for local testing
- **Card lifecycle**: Register, Block, Unblock, Wipe with blacklist enforcement
- **Wallet operations**: Load, Spend (with daily/monthly limits), Refund with idempotency
- **Optimistic concurrency** on wallet balance via version column
- **Immutable transaction ledger** with idempotency keys
- **Custom data**: Every entity supports a `custom_data` JSON column — tenants store whatever they want
- **Custom field definitions**: Tenants define their own schema per entity type
- **Kafka event publishing** with CloudEvents 1.0 envelope
- **Azure App Configuration** integration for centralized config management
- **Docker Compose** with MySQL 8.0 and Kafka (KRaft mode, no Zookeeper)
- **Swagger UI** with JWT bearer auth support

## 15-Table Schema

| # | Table | Purpose |
|---|-------|---------|
| 1 | `tenant_settings` | Per-org configuration |
| 2 | `custom_field_definitions` | Tenant-defined schema |
| 3 | `card_holders` | People + custom_data |
| 4 | `card_types` | Card categories with limits |
| 5 | `cards` | RFID card registry |
| 6 | `wallets` | Stored-value balances |
| 7 | `merchants` | POS terminals |
| 8 | `transactions` | Immutable ledger |
| 9 | `audit_logs` | Compliance trail |
| 10 | `blacklisted_uids` | Fraud prevention |
| 11 | `batch_operations` | Bulk action tracking |
| 12 | `auto_topup_rules` | Auto-reload config |
| 13 | `webhook_subscriptions` | Event subscriptions |
| 14 | `notification_logs` | Delivery tracking |
| 15 | `custom_field_index` | Searchable JSON index |

## Quick Start (Docker)

```bash
docker-compose up --build
```

This starts the API on `http://localhost:8080`, MySQL on port 3306, and Kafka on port 9092. The `init-db.sql` automatically creates all 15 tables.

**Swagger UI:** http://localhost:8080/swagger

Dev mode is enabled by default in Docker — no JWT token needed.

## Local Development (Visual Studio)

### Prerequisites
- .NET 8 SDK
- MySQL 8.0 (local or Azure)
- Visual Studio 2022 or VS Code

### Setup

1. Clone and open `CardManagement.Api.csproj` in Visual Studio
2. Update `appsettings.Development.json` with your MySQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=card_management;User=root;Password=yourpass;"
     }
   }
   ```
3. Run the `init-db.sql` against your MySQL instance
4. Press F5 — Swagger opens at https://localhost:7143/swagger

`BypassAuth: true` is set in dev mode, so all requests auto-authenticate as `dev-org-001` / `dev-user-001`.

## Azure Deployment

### Azure App Configuration

Store your connection string in Azure App Configuration:
- Key: `ConnectionStrings:DefaultConnection`
- Value: Your Azure MySQL connection string

Set the App Configuration connection string in your App Service settings:
```
AzureAppConfig__ConnectionString = <your-azure-app-config-connection-string>
```

Or use Managed Identity:
```
AzureAppConfig__Endpoint = https://<your-config>.azconfig.io
```

### Container Deployment

```bash
docker build -t sendnconnect-rfid-api .
# Push to Azure Container Registry and deploy to App Service / AKS
```

### Environment Variables (Production)

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | MySQL connection string |
| `AzureAppConfig__ConnectionString` | Azure App Config connection string |
| `Zitadel__Authority` | Zitadel issuer URL |
| `Zitadel__Audience` | Zitadel project audience |
| `Kafka__BootstrapServers` | Kafka broker addresses |
| `BypassAuth` | Set to `false` in production! |

## API Endpoints

### Cards
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/cards` | Register a new card |
| GET | `/api/cards` | List cards (paginated) |
| GET | `/api/cards/{id}` | Get card by ID |
| PUT | `/api/cards/{id}` | Update card metadata |
| PUT | `/api/cards/{id}/block` | Block a card |
| PUT | `/api/cards/{id}/unblock` | Unblock a card |
| PUT | `/api/cards/{id}/wipe` | Wipe card (zero balance) |

### Wallets
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/cards/{cardId}/wallet` | Get wallet |
| POST | `/api/cards/{cardId}/wallet/load` | Load funds |
| POST | `/api/cards/{cardId}/wallet/spend` | Spend funds |
| POST | `/api/cards/{cardId}/wallet/refund` | Refund transaction |

### Transactions
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/transactions` | List transactions |
| GET | `/api/transactions/{id}` | Get transaction |

### Card Holders, Merchants, Custom Fields, Tenant Settings
Full CRUD endpoints available — see Swagger for details.

## Project Structure

```
CardManagement.Api/
├── Configuration/          # Settings classes (Zitadel, Kafka)
├── Controllers/            # 7 API controllers
├── Data/                   # EF Core DbContext
├── Infrastructure/         # Middleware, tenant context, Kafka
├── Models/
│   ├── DTOs/               # Request/Response models
│   ├── Entities/           # 15 entity models + ITenantEntity
│   └── Enums/              # 9 enum definitions
├── Services/               # Business logic (7 service pairs)
├── Properties/             # launchSettings.json
├── Program.cs              # App entry point
├── Dockerfile              # Multi-stage Docker build
├── docker-compose.yml      # Full dev stack
├── init-db.sql             # 15-table MySQL DDL
└── appsettings.json        # Configuration
```

## License

Proprietary — All rights reserved.
