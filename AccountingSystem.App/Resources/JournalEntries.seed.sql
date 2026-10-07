-- Journal Entries seed data: three balanced sample transactions
INSERT INTO JournalEntries (EntryDate, AccountId, VoucherNo, Description, Debit, Credit) VALUES ('2025-01-01', 'A001', 1, 'Initial cash capital', 100000, 0);
INSERT INTO JournalEntries (EntryDate, AccountId, VoucherNo, Description, Debit, Credit) VALUES ('2025-01-01', 'L0001', 1, 'Initial cash capital', 0, 100000);

INSERT INTO JournalEntries (EntryDate, AccountId, VoucherNo, Description, Debit, Credit) VALUES ('2025-01-02', 'E027', 2, 'Sample salary payment', 20000, 0);
INSERT INTO JournalEntries (EntryDate, AccountId, VoucherNo, Description, Debit, Credit) VALUES ('2025-01-02', 'A001', 2, 'Sample salary payment', 0, 20000);

INSERT INTO JournalEntries (EntryDate, AccountId, VoucherNo, Description, Debit, Credit) VALUES ('2025-01-03', 'A002', 3, 'Transfer to petty cash', 10000, 0);
INSERT INTO JournalEntries (EntryDate, AccountId, VoucherNo, Description, Debit, Credit) VALUES ('2025-01-03', 'A001', 3, 'Transfer to petty cash', 0, 10000);
