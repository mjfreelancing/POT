type AccrualsStatusInput = {
  accountRowIds: string[];
};

type AccrualsStatus = {
  expenseRenewalsRequired: string[];
  incomeRenewalsRequired: string[];
};

export type { AccrualsStatus, AccrualsStatusInput };
