type AccrueAccountExpensesInput = {
  rowIds: string[];
};

type AccrualsStatusInput = {
  accountRowIds: string[];
};

type AccrualsStatus = {
  expenseRenewalsRequired: string[];
  incomeRenewalsRequired: string[];
  accountAccrualsRequired: string[];
};

export type { AccrualsStatus, AccrualsStatusInput, AccrueAccountExpensesInput };
