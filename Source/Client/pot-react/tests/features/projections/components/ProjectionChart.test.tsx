import { render, screen } from '@testing-library/react';
import { afterAll, beforeAll, describe, expect, test, vi } from 'vitest';

import {
  DEFAULT_PROJECTION_INCLUDE,
  PROJECTION_METRICS,
} from '@/data/projection';
import ProjectionChart, {
  type ProjectionChartProps,
} from '@/features/projections/components/ProjectionChart';
import { formatMoneyValue } from '@/lib';

import { createProjection } from '../../../shared/factories/projectionFactory';

// The short-viewport hook reads window.matchMedia + window.innerHeight, which
// jsdom does not model; the desktop (non-short) layout is the chart path under
// test, so stub it to false.
vi.mock('@/hooks/use-short-viewport', () => ({
  useIsShortViewport: () => false,
}));

// Stub the leaf feature controls/sheets: this smoke exercises the chart
// pipeline (data hook + ChartContainer + real recharts primitives), not the
// controls/sheets themselves.
vi.mock('@/features/projections/components/ChartControls', () => ({
  default: () => <div data-testid="chart-controls" />,
}));

vi.mock('@/features/projections/components/NoProjectionData', () => ({
  default: ({ title }: { title?: string }) => (
    <div data-testid="no-projection-data">{title ?? 'no-data'}</div>
  ),
}));

vi.mock('@/features/projections/components/ExpenseDetails', () => ({
  default: () => <div data-testid="expense-details" />,
}));

vi.mock('@/features/projections/components/IncomeDetails', () => ({
  default: () => <div data-testid="income-details" />,
}));

// Recharts' ResponsiveContainer measures its container through ResizeObserver.
// The shared test mock never reports a size, so the chart falls back to 0x0 and
// renders no plot. Removing the global makes ResponsiveContainer keep its
// initialDimension (320x200), so the real plot renders under jsdom.
beforeAll(() => {
  vi.stubGlobal('ResizeObserver', undefined);
});

afterAll(() => {
  vi.unstubAllGlobals();
});

function renderChart(
  overrides: Partial<ProjectionChartProps> = {},
): ReturnType<typeof render> {
  const props: ProjectionChartProps = {
    data: createProjection(),
    startDate: new Date(2026, 3, 1),
    period: 1,
    selectedMetric: 'balance',
    hiddenSeries: [],
    include: DEFAULT_PROJECTION_INCLUDE,
    onIncludeChange: vi.fn(),
    onStartDateChange: vi.fn(),
    onPeriodChange: vi.fn(),
    onMetricChange: vi.fn(),
    onHiddenSeriesChange: vi.fn(),
    isDetailsOpen: false,
    selectedDate: null,
    onToggleDetails: vi.fn(),
    ...overrides,
  };

  return render(<ProjectionChart {...props} />);
}

describe('ProjectionChart', () => {
  test('renders a line-chart card for a line metric', () => {
    renderChart({ selectedMetric: 'balance' });

    expect(
      screen.getByText(PROJECTION_METRICS.balance.title),
    ).toBeInTheDocument();
    expect(screen.getByTestId('chart-controls')).toBeInTheDocument();
    expect(document.querySelector('[data-slot="chart"]')).not.toBeNull();
  });

  test('renders a bar-chart card for a bar metric', () => {
    renderChart({ selectedMetric: 'expensesPaid' });

    expect(
      screen.getByText(PROJECTION_METRICS.expensesPaid.title),
    ).toBeInTheDocument();
    expect(document.querySelector('[data-slot="chart"]')).not.toBeNull();
  });

  test('shows the empty state when there is no chart data', () => {
    renderChart({
      data: createProjection({
        accounts: [],
        global: [],
      }),
    });

    expect(screen.getByTestId('no-projection-data')).toBeInTheDocument();
    expect(document.querySelector('[data-slot="chart"]')).toBeNull();
  });

  test('shows a no-series message when every series is hidden', () => {
    renderChart({
      selectedMetric: 'balance',
      hiddenSeries: ['account-1', 'account-2', 'global'],
    });

    expect(screen.getByTestId('no-projection-data')).toHaveTextContent(
      'No account selected',
    );
    expect(document.querySelector('[data-slot="chart"]')).toBeNull();
  });

  test('renders the income detail sheet for the income metric when a date is selected', () => {
    renderChart({
      selectedMetric: 'incomeReceived',
      selectedDate: new Date(2026, 3, 2),
      isDetailsOpen: true,
    });

    expect(screen.getByTestId('income-details')).toBeInTheDocument();
  });

  test('renders the expense detail sheet for the expense metric when a date is selected', () => {
    renderChart({
      selectedMetric: 'expensesPaid',
      selectedDate: new Date(2026, 3, 1),
      isDetailsOpen: true,
    });

    expect(screen.getByTestId('expense-details')).toBeInTheDocument();
  });

  test('renders min/max values at the visible low and high for the balance metric', () => {
    renderChart({ selectedMetric: 'balance' });

    // The default include subtracts arrears only, so the factory's two days
    // compose to a visible low of 80 and a high of 235.
    expect(screen.getByTestId('projection-extreme-low')).toHaveTextContent(
      formatMoneyValue(80),
    );
    expect(screen.getByTestId('projection-extreme-high')).toHaveTextContent(
      formatMoneyValue(235),
    );
    expect(screen.getByTestId('projection-extremes-summary')).toHaveTextContent(
      `Projected period low ${formatMoneyValue(80)}, high ${formatMoneyValue(235)}`,
    );
  });

  test('excludes hidden series from the read-out', () => {
    renderChart({ selectedMetric: 'balance', hiddenSeries: ['global'] });

    // Hiding the combined total leaves the account extremes: 80 and 145.
    expect(screen.getByTestId('projection-extreme-low')).toHaveTextContent(
      formatMoneyValue(80),
    );
    expect(screen.getByTestId('projection-extreme-high')).toHaveTextContent(
      formatMoneyValue(145),
    );
  });

  test('renders no read-out when every series is hidden', () => {
    renderChart({
      selectedMetric: 'balance',
      hiddenSeries: ['account-1', 'account-2', 'global'],
    });

    expect(screen.queryByTestId('projection-extreme-high')).toBeNull();
    expect(screen.queryByTestId('projection-extreme-low')).toBeNull();
    expect(screen.queryByTestId('projection-extremes-summary')).toBeNull();
  });

  test('renders no read-out under a bar metric', () => {
    renderChart({ selectedMetric: 'expensesPaid' });

    expect(screen.queryByTestId('projection-extreme-high')).toBeNull();
    expect(screen.queryByTestId('projection-extremes-summary')).toBeNull();
  });

  test('renders the read-out under Projection Accruals', () => {
    renderChart({ selectedMetric: 'dailyAccrual' });

    // Daily accrual values across the factory range from 5 to 18.
    expect(screen.getByTestId('projection-extreme-low')).toHaveTextContent(
      formatMoneyValue(5),
    );
    expect(screen.getByTestId('projection-extreme-high')).toHaveTextContent(
      formatMoneyValue(18),
    );
  });

  test('follows the include switches', () => {
    renderChart({
      selectedMetric: 'balance',
      include: { reserved: true, accruals: false, arrears: false },
    });

    // Reserved only: the account low/high become 75 and 225.
    expect(screen.getByTestId('projection-extreme-low')).toHaveTextContent(
      formatMoneyValue(75),
    );
    expect(screen.getByTestId('projection-extreme-high')).toHaveTextContent(
      formatMoneyValue(225),
    );
  });

  test('renders one line and one value when the low equals the high', () => {
    const flatProjection = createProjection({
      accounts: [
        {
          rowId: 'account-1',
          description: 'Bills Account',
          dates: [
            {
              date: '2026-04-01',
              balance: 100,
              reserved: 0,
              arrears: 10,
              unpaidAccrual: 0,
              dailyAccrual: 0,
              incomeReceived: 0,
              expensesPaid: 0,
              expenseItems: [],
              incomeItems: [],
            },
          ],
        },
      ],
      global: [
        {
          date: '2026-04-01',
          balance: 100,
          reserved: 0,
          arrears: 10,
          unpaidAccrual: 0,
          dailyAccrual: 0,
          incomeReceived: 0,
          expensesPaid: 0,
          expenseItems: [],
          incomeItems: [],
        },
      ],
    });

    renderChart({ data: flatProjection, selectedMetric: 'balance' });

    // The arrears-only default composes 100 - 10 = 90 on the single day for
    // both the account and the combined total, so low and high are equal.
    expect(screen.getByTestId('projection-extreme')).toHaveTextContent(
      formatMoneyValue(90),
    );
    expect(screen.queryByTestId('projection-extreme-high')).toBeNull();
    expect(screen.queryByTestId('projection-extreme-low')).toBeNull();
  });
});
