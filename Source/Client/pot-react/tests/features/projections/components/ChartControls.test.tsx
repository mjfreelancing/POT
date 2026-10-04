import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';

import type { ChartConfig } from '@/components/ui/chart';
import type { ProjectionInclude } from '@/data/projection';
import { DEFAULT_PROJECTION_INCLUDE } from '@/data/projection';
import ChartControls, {
  type ChartControlsProps,
} from '@/features/projections/components/ChartControls';

const chartConfig: ChartConfig = {
  'account-1': { label: 'Bills Account', color: '#8884d8' },
  total: { label: 'Total (Selected Accounts)', color: '#2563eb' },
};

function createProps(
  overrides: Partial<ChartControlsProps> = {},
): ChartControlsProps {
  return {
    selectedMetric: 'balance',
    onMetricChange: vi.fn(),
    startDate: new Date(2026, 0, 1),
    onStartDateChange: vi.fn(),
    period: 6,
    onPeriodChange: vi.fn(),
    seriesKeys: ['account-1', 'total'],
    seriesVisibility: { 'account-1': true, total: true },
    onToggleSeries: vi.fn(),
    chartConfig,
    include: DEFAULT_PROJECTION_INCLUDE,
    onIncludeChange: vi.fn(),
    ...overrides,
  };
}

function includeTrigger(): HTMLElement {
  return screen.getByRole('button', { name: /^Include:/ });
}

describe('ChartControls', () => {
  test('collapses the facets and the legend behind an Options disclosure on small screens', async () => {
    const user = userEvent.setup();

    render(<ChartControls {...createProps()} />);

    const wrapper = screen.getByRole('group', { name: 'View' }).parentElement!
      .parentElement!;

    expect(wrapper).toHaveClass('hidden', 'md:flex');
    expect(wrapper).toContainElement(screen.getByText('Accounts'));

    await user.click(screen.getByRole('button', { name: 'Show options' }));

    expect(wrapper).not.toHaveClass('hidden');
    expect(
      screen.getByRole('button', { name: 'Hide options' }),
    ).toBeInTheDocument();
  });

  describe('period', () => {
    test('shows the selected period in a months dropdown', () => {
      render(<ChartControls {...createProps()} />);

      expect(
        screen.getByRole('combobox', { name: 'Select chart period in months' }),
      ).toHaveTextContent('6 months');
    });

    test('speaks the singular for a one-month period', () => {
      render(<ChartControls {...createProps({ period: 1 })} />);

      expect(
        screen.getByRole('combobox', { name: 'Select chart period in months' }),
      ).toHaveTextContent('1 month');
    });
  });

  describe('include', () => {
    test('states the default deduction on the trigger', () => {
      render(<ChartControls {...createProps()} />);

      expect(includeTrigger()).toHaveAccessibleName(
        'Include: Balance less: Arrears',
      );
    });

    test('lists every active deduction in switch order', () => {
      const include: ProjectionInclude = {
        reserved: true,
        accruals: true,
        arrears: false,
      };

      render(<ChartControls {...createProps({ include })} />);

      expect(includeTrigger()).toHaveAccessibleName(
        'Include: Balance less: Reserved, Accruals',
      );
    });

    test('reads Balance when all three switches are off', () => {
      const include: ProjectionInclude = {
        reserved: false,
        accruals: false,
        arrears: false,
      };

      render(<ChartControls {...createProps({ include })} />);

      expect(includeTrigger()).toHaveAccessibleName('Include: Balance');
    });

    test('is absent under the other metrics', () => {
      render(
        <ChartControls {...createProps({ selectedMetric: 'expensesPaid' })} />,
      );

      expect(
        screen.queryByRole('button', { name: /^Include:/ }),
      ).not.toBeInTheDocument();
    });

    test('reveals the three switches with accessible names and the default state', async () => {
      const user = userEvent.setup();

      render(<ChartControls {...createProps()} />);

      await user.click(includeTrigger());

      expect(
        screen.getByRole('switch', { name: 'Include Reserved' }),
      ).not.toBeChecked();
      expect(
        screen.getByRole('switch', { name: 'Include Accruals' }),
      ).not.toBeChecked();
      expect(
        screen.getByRole('switch', { name: 'Include Arrears' }),
      ).toBeChecked();
    });

    test('reflects the controlled state when reopened', async () => {
      const user = userEvent.setup();
      const include: ProjectionInclude = {
        reserved: true,
        accruals: false,
        arrears: false,
      };

      const { rerender } = render(
        <ChartControls {...createProps({ include })} />,
      );

      await user.click(includeTrigger());

      expect(
        screen.getByRole('switch', { name: 'Include Reserved' }),
      ).toBeChecked();
      expect(
        screen.getByRole('switch', { name: 'Include Arrears' }),
      ).not.toBeChecked();

      // Hidden under another metric, then shown again, the same state applies.
      rerender(
        <ChartControls
          {...createProps({ include, selectedMetric: 'expensesPaid' })}
        />,
      );

      expect(
        screen.queryByRole('button', { name: /^Include:/ }),
      ).not.toBeInTheDocument();
    });

    test('hands the whole include record back when one switch is toggled', async () => {
      const user = userEvent.setup();
      const props = createProps();

      render(<ChartControls {...props} />);

      await user.click(includeTrigger());
      await user.click(
        screen.getByRole('switch', { name: 'Include Reserved' }),
      );

      expect(props.onIncludeChange).toHaveBeenCalledWith({
        reserved: true,
        accruals: false,
        arrears: true,
      });
    });
  });

  describe('accounts', () => {
    test('renders the legend toggles with accessible names', () => {
      render(<ChartControls {...createProps()} />);

      expect(screen.getByText('Accounts')).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Hide Bills Account account on chart',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Hide Total (Selected Accounts) account on chart',
        }),
      ).toBeInTheDocument();
    });

    test('keeps the total toggle outside the Accounts group', () => {
      render(<ChartControls {...createProps()} />);

      const accountsGroup = screen.getByRole('group', { name: 'Accounts' });

      expect(
        within(accountsGroup).getByRole('button', {
          name: 'Hide Bills Account account on chart',
        }),
      ).toBeInTheDocument();

      expect(
        within(accountsGroup).queryByRole('button', {
          name: /Total \(Selected Accounts\)/,
        }),
      ).toBeNull();

      expect(
        screen.getByRole('button', {
          name: 'Hide Total (Selected Accounts) account on chart',
        }),
      ).toBeInTheDocument();
    });

    test('hands the toggled series key back to the caller', async () => {
      const user = userEvent.setup();
      const props = createProps();

      render(<ChartControls {...props} />);

      await user.click(
        screen.getByRole('button', {
          name: 'Hide Bills Account account on chart',
        }),
      );
      await user.click(
        screen.getByRole('button', {
          name: 'Hide Total (Selected Accounts) account on chart',
        }),
      );

      expect(props.onToggleSeries).toHaveBeenNthCalledWith(1, 'account-1');
      expect(props.onToggleSeries).toHaveBeenNthCalledWith(2, 'total');
    });

    test('reports the hidden state for a hidden series', () => {
      render(
        <ChartControls
          {...createProps({
            seriesVisibility: { 'account-1': false, total: true },
          })}
        />,
      );

      expect(
        screen.getByRole('button', {
          name: 'Show Bills Account account on chart',
        }),
      ).toHaveAttribute('aria-pressed', 'false');
    });
  });
});
