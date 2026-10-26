# NovaBank Secure Console Terminal

NovaBank is an educational C# console banking application with customer onboarding, customer and administrator portals, and HTTP-based transfers between two running instances.

## Features

* Create a zero-balance account with name, state, phone number, password, and a generated unique account number.
* Log in as a customer to deposit, withdraw, make intra-bank or inter-bank transfers, pay bills, view forex values, and inspect account history.
* Confirm transfers with the sender's password and display a local recipient's details before an intra-bank transfer.
* Receive inter-bank transfer requests through `POST /api/transfers` using an interface-based HTTP client/server design.
* Log in as an administrator and freeze or unfreeze accounts. Frozen accounts cannot initiate transactions, but can still receive funds.

## Project Structure

* `Application/` coordinates console workflows without containing banking rules.
* `Models/` contains account, transaction, bill, and result data.
* `Data/` contains the in-memory account repository abstraction and implementation.
* `Services/` contains account creation, authentication, banking, administration, wallet, and transfer rules.
* `Contracts/` contains the request/response objects and interface used for HTTP transfers.
* `Network/` contains the HTTP client and local HTTP listener.
* `Ui/` contains all console prompts and display formatting.

## Run One Instance

```bash
dotnet run --project NovaBankApp/NovaBankApp.csproj -- --port 5000
```

The `--port` argument is optional; the default is `5000`.

## Run Two Instances

Open two terminals from the project root:

```bash
dotnet run --project NovaBankApp/NovaBankApp.csproj -- --port 5000
dotnet run --project NovaBankApp/NovaBankApp.csproj -- --port 5001
```

Create or log in to a customer on each instance. On the sending instance select **Transfer Funds → Inter-Bank Transfer**, enter the other app's URL (for example `http://localhost:5001`), then enter the receiving account number and confirm with your password.

Each app has its own in-memory data, so a customer must exist in the receiving app before it can receive an inter-bank transfer. Data resets when that app exits.

## Demo Accounts

Each newly launched app seeds these local accounts for intra-bank transfer testing:

| Name | Account number | Password |
| --- | --- | --- |
| Brian Naheemat | `1000000001` | `test123` |
| Francis Chocboy | `1000000002` | `test123` |

## Administrator Login

Use the following preset credentials at the normal login screen:

| User name | Password |
| --- | --- |
| `admin` | `Admin@123` |

## Educational Scope

Accounts are intentionally stored in memory and the inter-bank endpoint has no production authentication or encryption. A production version should use persistent storage, identity management, TLS, signed requests, and transactional database operations.
