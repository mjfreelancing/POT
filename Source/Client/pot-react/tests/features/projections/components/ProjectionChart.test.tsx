import { render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import { PROJECTION_METRICS } from '@/data/projection';
import ProjectionChart, {
  type ProjectionChartProps,
} from '@/features/projections/components/ProjectionChart';

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
  default: () => <div data-testid="no-projection-data" />,
}));

vi.mock('@/features/projections/components/ExpenseDetails', () => ({
  default: () => <div data-testid="expense-details" />,
}));

vi.mock('@/features/projections/components/IncomeDetails', () => ({
  default: () => <div data-testid="income-details" />,
}));

function renderChart(
  overrides: Partial<ProjectionChartProps> = {},
): ReturnType<typeof render> {
  const props: ProjectionChartProps = {
    data: createProjection(),
    startDate: new Date(2026, 3, 1),
    period: 1,
    selectedMetric: 'balance',
    hiddenSeries: [],
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
});
