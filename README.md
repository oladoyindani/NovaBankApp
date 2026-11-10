# NovaBank Secure Console Terminal

NovaBank is an educational C# console banking application with customer onboarding, customer and administrator portals, and HTTP-based transfers between two running instances.

## Features

* Create a zero-balance account with name, state, phone number, password, and a generated unique account number.
* Log in as a customer to deposit, withdraw, make intra-bank or inter-bank transfers, pay bills, view forex values, and inspect account history.
* Confirm transfers with the sender's password and display a local recipient's details before an intra-bank transfer.
* Receive inter-bank transfer requests through `POST /api/transfers` using an interface-based HTTP client/server design.
* Provide simulated airtime and data purchases over HTTP, with provider-side history, duplicate detection, and transaction status lookup.
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

Account and payment data are intentionally stored in memory and reset when the app exits. The inter-bank and bill-payment endpoints have no production authentication or encryption. A production version should use persistent storage, identity management, TLS, signed requests, and transactional database operations.

## Bill-Payment Provider API

This app can act as a local HTTP provider for a duplicated/client app. Start this instance with a port, then send requests to that host and port. The listener binds to `localhost`, so both apps must run on the same machine.

### Buy airtime or data

Send a `POST` request to `/api/bills/airtime` or `/api/bills/data` with a caller-generated GUID transaction ID, destination phone number, and positive amount:

```bash
curl -X POST http://localhost:5000/api/bills/airtime \
  -H "Content-Type: application/json" \
  -d '{"transactionId":"8af59302-89af-4e20-9818-afb3ac7f2c51","phoneNumber":"08012345678","amount":100}'
```

The data route uses the same request shape. Successful purchases return HTTP `200` and a JSON response with `transactionId`, `paymentType`, `isSuccessful`, `isDuplicate`, `message`, and `processedAt`. Invalid requests return HTTP `400`; a reused transaction ID returns HTTP `409`. Use a unique ID for each purchase and reuse the same ID when retrying after a timeout.

### Check a transaction

After a duplicate response or timeout, retrieve the original result using:

```text
GET http://localhost:5000/api/bills/payments/{transactionId}
```

The response contains the original payment result; unknown IDs return HTTP `404`. The provider records valid transaction IDs, including rejected payment attempts, in memory, so a failed attempt cannot later be retried under the same ID. This is a demo provider: a valid request is recorded as successful but is not sent to a real airtime or data carrier.
