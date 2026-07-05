# ☕ Hospital Cafe Ledger

A professional, offline desktop application built with **.NET 10**, **WPF (XAML)**, **SQLite**, **Entity Framework Core**, and **ML.NET** for managing hospital cafe operations. It helps track doctor accounts, billing, payments, ledger history, inventory, reports, and AI-powered business insights.

[![Download Latest Release](https://img.shields.io/badge/⬇-Download%20Latest%20Release-blue?style=for-the-badge)](https://github.com/chaudharyrizwan1876/HospitalCafeLedger/releases/latest/download/HospitalCafeLedger-v1.0.zip)

---

# 🔑 Demo Login Credentials

| Role | Email | Password |
|------|-------|----------|
| **Admin** | admin@cafeledger.com | Admin@cafe |

> ⚠️ These credentials are for demonstration purposes only.

---

# 📸 Features Overview

## 👨‍⚕️ Doctor / Member Management

- Add, edit and delete doctor accounts
- Opening balance management
- Department & phone information
- Live doctor search
- Active account tracking

---

## 🛒 Billing & Orders

- Fast billing interface
- Quick item buttons
- Custom item support
- Automatic balance deduction
- Real-time total calculation
- Order history recording

---

## 💰 Payments

- Deposit money into doctor accounts
- Automatic balance updates
- Outstanding balance tracking
- Complete payment history

---

## 📖 Ledger History

- Complete order history
- Date-wise grouping
- Daily totals
- Doctor-wise filtering

---

## 🍔 Items / Recipes

- Add new menu items
- Edit item prices
- Category management
- Quick billing integration

---

## 📊 Reports

- Monthly doctor reports
- Opening & closing balance
- Deposit summary
- Order summary
- CSV export
- Date filtering

---

## 💾 Backup & Restore

- Database backup
- Restore previous backups
- Safe data recovery

---

## 🤖 AI Insights (ML.NET)

Built using **ML.NET**

| Feature | Description |
|----------|-------------|
| **Sales Forecasting** | Predicts next month's expected sales |
| **Low Balance Prediction** | Identifies doctors likely to run out of balance |
| **Trending Items Analysis** | Shows fast-growing and declining menu items |
| **Peak Hour Analysis** | Predicts busiest days and hours for the cafe |

---

# 🛠 Technology Stack

| Layer | Technology |
|--------|------------|
| **Framework** | .NET 10 |
| **Desktop UI** | WPF (XAML) |
| **Database** | SQLite |
| **ORM** | Entity Framework Core 10 |
| **Machine Learning** | ML.NET |
| **Architecture** | Layered Architecture (Models → Data → Services → Desktop) |

---

# 🚀 Getting Started

## Option 1 — Download Ready-to-Use Application (Recommended)

Download the latest release from:

👉 **https://github.com/chaudharyrizwan1876/HospitalCafeLedger/releases/latest**

Steps:

1. Download the latest ZIP file.
2. Extract it.
3. Open the extracted folder.
4. Run:

```
HospitalCafeLedger.App.exe
```

No installation is required.

---

## Option 2 — Build From Source

### Prerequisites

- .NET 10 SDK
- Windows 10 / Windows 11
- Visual Studio 2022 or VS Code

Clone the repository:

```bash
git clone https://github.com/chaudharyrizwan1876/HospitalCafeLedger.git
```

Navigate to the project:

```bash
cd HospitalCafeLedger
```

Restore packages:

```bash
dotnet restore
```

Run the application:

```bash
dotnet run --project HospitalCafeLedger.App/HospitalCafeLedger.App.csproj
```

---

# 🗄 First Run

On the first launch the application automatically:

- Creates the SQLite database
- Creates the default Admin account
- Initializes required tables
- Loads sample data (if available)

---

# 📁 Project Structure

```
HospitalCafeLedger
│
├── HospitalCafeLedger.App
├── HospitalCafeLedger.Services
├── HospitalCafeLedger.Data
├── HospitalCafeLedger.Models
└── HospitalCafeLedger.slnx
```

---

# ✨ Highlights

- Modern WPF Desktop UI
- SQLite Database
- Entity Framework Core
- ML.NET Integration
- Offline First
- AI Sales Forecasting
- Doctor Wallet Management
- Inventory & Billing
- CSV Report Export
- Professional Layered Architecture

---

# 📄 License

This project is available for educational and portfolio purposes.

---

# 👨‍💻 Author

**Rizwan Chaudhary**

GitHub:
https://github.com/chaudharyrizwan1876
