import { fireEvent, render, screen } from '@testing-library/react';
import React from 'react';
import { describe, expect, test, vi } from 'vitest';

import type { Account } from '@/data';

import AccountFilter from '@/components/filters/AccountFilter';

vi.mock('@/components/ui/select', () => {
  const SelectTrigger = ({
    children,
    className,
  }: React.PropsWithChildren<{ className?: string }>) => {
    return (
      <div data-testid="account-filter-trigger" className={className}>
        {children}
      </div>
    );
  };

  const SelectValue = ({ placeholder }: { placeholder?: string }) => {
    return <span>{placeholder}</span>;
  };

  const SelectContent = ({ children }: React.PropsWithChildren) => {
    return <>{children}</>;
  };

  const SelectItem = ({
    children,
  }: React.PropsWithChildren<{ value: string }>) => {
    return <>{children}</>;
  };

  const Select = ({
    value,
    onValueChange,
    name,
    children,
  }: React.PropsWithChildren<{
    value: string;
    onValueChange: (value: string) => void;
    name?: string;
  }>) => {
    const optionElements: Array<{ value: string; label: string }> = [];

    const walk = (nodeChildren: React.ReactNode) => {
      React.Children.forEach(nodeChildren, node => {
        if (
          !React.isValidElement<{
            value?: string;
            children?: React.ReactNode;
          }>(node)
        ) {
          return;
        }

        if (node.type === SelectItem) {
          optionElements.push({
            value: node.props.value as string,
            label: String(node.props.children),
          });
          return;
        }

        walk(node.props.children);
      });
    };

    walk(children);

    return (
      <div>
        {children}
        <select
          aria-label="Filter by account"
          name={name}
          value={value}
          onChange={event => {
            onValueChange(event.target.value);
          }}
        >
          {optionElements.map(option => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>
    );
  };

  return {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
  };
});

const createAccount = (rowId: string, description: string): Account => ({
  rowId,
  etag: 0n,
  bsb: '111-111',
  number: '12345678',
  description,
  balance: 1000,
  reserved: 100,
  totalExpenseAccrued: 10,
  dailyExpenseAccrual: 1,
  stableExpenseAccrual: 1,
  available: 900,
  linkedExpenses: 1,
  linkedIncomes: 1,
});

describe('AccountFilter', () => {
  test('renders all accounts option and account descriptions', () => {
    const accounts = [
      createAccount('account-1', 'Primary Account'),
      createAccount('account-2', 'Bills Account'),
    ];

    render(
      <AccountFilter
        accounts={accounts}
        selectedAccountId={null}
        onAccountChange={vi.fn()}
      />,
    );

    expect(
      screen.getByRole('option', { name: 'All Accounts' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('option', { name: 'Primary Account' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('option', { name: 'Bills Account' }),
    ).toBeInTheDocument();
  });

  test('calls onAccountChange with account id when selecting a specific account', () => {
    const onAccountChange = vi.fn();

    render(
      <AccountFilter
        accounts={[createAccount('account-1', 'Primary Account')]}
        selectedAccountId={null}
        onAccountChange={onAccountChange}
      />,
    );

    fireEvent.change(
      screen.getByRole('combobox', { name: 'Filter by account' }),
      {
        target: { value: 'account-1' },
      },
    );

    expect(onAccountChange).toHaveBeenCalledWith('account-1');
  });

  test('calls onAccountChange with null when selecting all', () => {
    const onAccountChange = vi.fn();

    render(
      <AccountFilter
        accounts={[createAccount('account-1', 'Primary Account')]}
        selectedAccountId={'account-1'}
        onAccountChange={onAccountChange}
      />,
    );

    fireEvent.change(
      screen.getByRole('combobox', { name: 'Filter by account' }),
      {
        target: { value: 'all' },
      },
    );

    expect(onAccountChange).toHaveBeenCalledWith(null);
  });

  test('applies active style when an account is selected', () => {
    render(
      <AccountFilter
        accounts={[createAccount('account-1', 'Primary Account')]}
        selectedAccountId={'account-1'}
        onAccountChange={vi.fn()}
      />,
    );

    expect(screen.getByTestId('account-filter-trigger')).toHaveClass(
      'ring-[2px]',
      'ring-primary/60',
      'bg-primary/10',
    );
  });
});
