import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import type { ChartConfig } from '@/components/ui/chart';
import type { ProjectionInclude } from '@/data/projection';
import { DEFAULT_PROJECTION_INCLUDE } from '@/data/projection';
import ChartControls, {
  type ChartControlsProps,
} from '@/features/projections/components/ChartControls';
import { useIsMobile } from '@/hooks/use-mobile';

vi.mock('@/hooks/use-mobile', () => ({
  useIsMobile: vi.fn(),
}));

const chartConfig: ChartConfig = {
  'account-1': { label: 'Bills Account', color: '#8884d8' },
  global: { label: 'Total (All Accounts)', color: '#2563eb' },
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
    seriesKeys: ['account-1', 'global'],
    seriesVisibility: { 'account-1': true, global: true },
    onToggleSeries: vi.fn(),
    chartConfig,
    include: DEFAULT_PROJECTION_INCLUDE,
    onIncludeChange: vi.fn(),
    ...overrides,
  };
}

const ALL_INCLUDES_OFF: ProjectionInclude = {
  reserved: false,
  accruals: false,
  arrears: false,
};

describe('ChartControls', () => {
  beforeEach(() => {
    vi.mocked(useIsMobile).mockReturnValue(false);
  });

  test('relabels the series toggle group from Show to Series', () => {
    render(<ChartControls {...createProps()} />);

    expect(screen.getByText('Series:')).toBeInTheDocument();
    expect(screen.queryByText('Show:')).not.toBeInTheDocument();
  });

  describe('include switches', () => {
    test('renders the three switches with accessible names and the default state', () => {
      render(<ChartControls {...createProps()} />);

      expect(screen.getByText('Include:')).toBeInTheDocument();
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

    test('hands the whole include record back when one switch is toggled', async () => {
      const user = userEvent.setup();
      const props = createProps();

      render(<ChartControls {...props} />);

      await user.click(
        screen.getByRole('switch', { name: 'Include Reserved' }),
      );

      expect(props.onIncludeChange).toHaveBeenCalledWith({
        reserved: true,
        accruals: false,
        arrears: true,
      });
    });

    test.each(['dailyAccrual', 'incomeReceived', 'expensesPaid'] as const)(
      'does not render the include group under the %s metric',
      metric => {
        render(<ChartControls {...createProps({ selectedMetric: metric })} />);

        expect(screen.queryByText('Include:')).not.toBeInTheDocument();
        expect(screen.queryByRole('switch')).not.toBeInTheDocument();
      },
    );

    test('reapplies the controlled state after being hidden under another metric', () => {
      const include: ProjectionInclude = {
        reserved: true,
        accruals: false,
        arrears: true,
      };

      const { rerender } = render(
        <ChartControls {...createProps({ include })} />,
      );

      expect(
        screen.getByRole('switch', { name: 'Include Reserved' }),
      ).toBeChecked();

      rerender(
        <ChartControls
          {...createProps({ include, selectedMetric: 'expensesPaid' })}
        />,
      );

      expect(screen.queryByRole('switch')).not.toBeInTheDocument();

      rerender(<ChartControls {...createProps({ include })} />);

      expect(
        screen.getByRole('switch', { name: 'Include Reserved' }),
      ).toBeChecked();
      expect(
        screen.getByRole('switch', { name: 'Include Arrears' }),
      ).toBeChecked();
    });

    test('shows the switches on mobile without expanding the filters', () => {
      vi.mocked(useIsMobile).mockReturnValue(true);

      render(<ChartControls {...createProps()} />);

      // The filters stay collapsed behind the toggle...
      expect(
        screen.getByRole('button', { name: 'Show filters' }),
      ).toBeInTheDocument();

      // ...while the include switches are already visible.
      expect(
        screen.getByRole('switch', { name: 'Include Reserved' }),
      ).toBeVisible();
      expect(
        screen.getByRole('switch', { name: 'Include Accruals' }),
      ).toBeVisible();
      expect(
        screen.getByRole('switch', { name: 'Include Arrears' }),
      ).toBeVisible();
    });
  });

  describe('touch caption', () => {
    test('names the default deduction and stays hidden at md and above', () => {
      render(<ChartControls {...createProps()} />);

      expect(screen.getByText('Balance less: Arrears')).toHaveClass(
        'md:hidden',
      );
    });

    test('reads Balance when all three switches are off', () => {
      render(<ChartControls {...createProps({ include: ALL_INCLUDES_OFF })} />);

      expect(screen.getByText('Balance')).toHaveClass('md:hidden');
    });

    test('lists every active deduction in switch order', () => {
      const include: ProjectionInclude = {
        reserved: true,
        accruals: true,
        arrears: false,
      };

      render(<ChartControls {...createProps({ include })} />);

      expect(
        screen.getByText('Balance less: Reserved, Accruals'),
      ).toBeInTheDocument();
    });
  });
});
