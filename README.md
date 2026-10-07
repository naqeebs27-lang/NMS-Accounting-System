# Naqeebs Accounting System

**Double-Entry Accounting Software**  
Developed by **Naqeebs Multi Services, Rawalpindi, Pakistan**

Naqeebs Accounting System is designed for double-entry bookkeeping, journal vouchers, ledger review, trial balance, basic Income Statement and Balance Sheet reporting, and petty cash handling.

> Every complete transaction must balance: **Total Debit = Total Credit**.

## Main Modules

- Chart of Accounts
- Journal Entries
- Ledger
- Trial Balance
- Income Statement
- Balance Sheet
- Petty Cash

## Basic Accounting Rules

| Account Type | Increase | Decrease | Normal Balance |
|---|---|---|---|
| Assets | Debit | Credit | Debit |
| Expenses | Debit | Credit | Debit |
| Liabilities | Credit | Debit | Credit |
| Capital / Equity | Credit | Debit | Credit |
| Income / Revenue | Credit | Debit | Credit |

## Recommended Workflow

1. Create and review the **Chart of Accounts**.
2. Identify the business transaction and supporting document.
3. Identify the accounts affected.
4. Decide the debit and credit treatment.
5. Enter all lines of the journal voucher.
6. Confirm **Journal out-of-balance total = 0.00**.
7. Review the Ledger.
8. Review the Trial Balance.
9. Reconcile Cash, Bank, and Petty Cash.
10. Review the Income Statement and Balance Sheet.

## Chart of Accounts

The supplied screen includes **Account ID, Account Name, Category, Type**, and controls for **Add New, Update Selected, Delete Selected,** and **Clear**. Account classifications should be internally consistent because a journal may balance even when an account is classified incorrectly.

## Journal Entries

The Journal screen includes **Date, Voucher No., Account, Description, Debit, Credit, Add Line,** and **Start New Voucher**. All lines belonging to one transaction should collectively balance.

Example: salary paid in cash for Rs. 50,000:

| Account | Debit | Credit |
|---|---:|---:|
| Salaries Expense | 50,000 | 0 |
| Cash | 0 | 50,000 |

After corrections or deletions, recheck the journal out-of-balance amount, ledger, and trial balance.

## Ledger and Trial Balance

The **Ledger** groups postings by account and is useful for investigating unusual balances. The **Trial Balance** summarizes ledger balances into Debit and Credit columns. Total debits should equal total credits, but equality does not prove that every account classification is correct.

## Income Statement

The basic relationship is:

**Income - Expenses = Profit or Loss**

## Balance Sheet

The basic accounting equation is:

**Assets = Liabilities + Equity**

## Petty Cash

The supplied Petty Cash screen includes period controls plus **Refresh, Generate Monthly Total, Post Summary to Journal**, and entry fields for **Date, Description, Account, Type, Receipt,** and **Payment**.

- **Receipt**: money received into petty cash.
- **Payment**: money paid out of petty cash.
- Review detailed transactions and the Monthly Petty Cash Expenses summary before posting.
- Do **not** manually post the same petty cash expense to the Journal and then post the same summary again.

Example: Rs. 300 petrol paid from petty cash:

| Account | Debit | Credit |
|---|---:|---:|
| Petrol Expense | 300 | 0 |
| Petty Cash | 0 | 300 |

## Common Entries

| Transaction | Debit | Credit |
|---|---|---|
| Owner introduces cash | Cash | Capital |
| Deposit cash into bank | Bank | Cash |
| Pay rent | Rent Expense | Cash/Bank |
| Earn cash service income | Cash | Service Revenue |
| Fund petty cash | Petty Cash | Cash/Bank |
| Buy equipment for cash | Equipment | Cash/Bank |
| Purchase/incur expense on credit | Relevant Asset/Expense | Accounts Payable |
| Pay creditor | Accounts Payable | Cash/Bank |

## Important Controls

- Do not use unexplained entries merely to force a balance.
- Transfers among Bank, Cash, and Petty Cash are normally asset transfers, not income or expenses.
- Investigate unexpected or negative balances through the relevant ledger and source documents.
- Maintain regular backups of accounting data.
- Keep voucher descriptions meaningful and retain supporting documents where applicable.

## User Manual

See **`Naqeebs_Accounting_System_User_Manual.pdf`** for the combined detailed manual.

## Disclaimer

The software assists with bookkeeping and basic accounting reports. Users remain responsible for transaction accuracy, account classification, backups, and compliance with applicable accounting, tax, and legal requirements. For complex matters, obtain professional advice.
