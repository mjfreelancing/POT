import { Account } from './account';

export const compareAccountBsbNumber = (lhs: Account, rhs: Account): number => {
  const bsbCompare = lhs.bsb.localeCompare(rhs.bsb);
  return bsbCompare !== 0 ? bsbCompare : lhs.number.localeCompare(rhs.number);
};
