# Cafe Management System

A desktop application built for hospital cafe management. It keeps track of doctor accounts, daily orders, payments, and balances — so cafe staff always know who ordered what and whether their account has enough credit.

---

## Download

[![Download Latest](https://img.shields.io/badge/Download-v1.0.0-blue?style=for-the-badge)](https://github.com/chaudharyrizwan1876/HospitalCafeLedger/releases/latest/download/HospitalCafeLedger.App.exe)

No installation required. Just download the `.exe` file and run it. The database is created automatically on first launch.

**Login credentials**
Email: admin@cafeledger.com
Password: Admin@cafe

---

## How It Works

Every doctor has an account in the system. When they open an account they deposit an opening balance — this is their prepaid credit. Every time they order food from the cafe, the order amount is deducted from that balance. They can add more money to their account at any time. If their balance runs out and they keep ordering, the system tracks the outstanding amount separately.

The staff uses the Billing screen to record orders in real time. They select the doctor, pick items from the quick menu or add a custom item, and save the order. The doctor's balance updates instantly.

---

## Screens

**AI Insights** shows four machine learning predictions: next month sales forecast, which doctors are about to run out of balance, which items are trending up or down, and what days and hours the cafe is busiest.

**Dashboard** shows today's sales, total pending amount across all doctors, total active doctors, today's order count, a monthly sales line chart, top ordered items this month, recent transactions, and a pending summary — all from live database data.

**Doctors / Members** is where you manage doctor accounts. You can add a new doctor with their name, department, phone number, and opening balance. You can edit any detail or delete a doctor. There is a live search bar to filter by name, ID, or department.

**Items / Recipes** is the menu management screen. Add any food or drink item with its category and price. Items added here automatically appear as quick buttons in the Billing screen.

**Billing / New Order** is the main screen for daily use. Select a doctor from the list, tap the quick items to add them to the order, or use the Custom Item button for anything not on the menu. The total updates as you add items. Press Save Order to record it.

**Payments** is the wallet screen. Every doctor's account shows their opening balance, total cash deposits, total orders placed, and current available balance. Staff can add a new deposit here when a doctor tops up their account. If the balance is negative the system shows the outstanding amount in red.

**Ledger / History** shows a complete order history for any doctor, grouped by date with a day total at the bottom of each date. Useful for checking what a specific doctor had over any period.

**Reports** generates a monthly summary for all doctors in one table — opening balance, total deposited, orders that month, and available balance. The report can be exported as a CSV file and opened in Excel.

**Backup / Restore** lets you save a copy of the database to any folder on your computer and restore from a previous backup if needed.

---

## Tech Stack

Built with C# and WPF on .NET 10. Database is SQLite managed through Entity Framework Core — the database file sits next to the application, no server or internet needed. Machine learning predictions use ML.NET. The entire application runs offline on any Windows machine.

---

## Requirements

Windows 10 or later. No additional software or runtime installation needed — everything is bundled in the single exe file.
