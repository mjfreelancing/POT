import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiGetProjection } from '@/api/hooks/useProjections';
import type { ProjectionMetric } from '@/data/projection';
import ProjectionsPage from '@/features/projections/ProjectionsPage';
import useProjectionStorage from '@/features/projections/hooks/useProjectionStorage';
import { SuccessResult } from '@/lib';

import { createProjection } from '../shared/factories/projectionFactory';

vi.mock('@/api/hooks/useProjections', () => ({
  useApiGetProjection: vi.fn(),
}));

vi.mock('@/features/projections/hooks/useProjectionStorage', () => ({
  default: vi.fn(),
  projectionStorageDefaults: {
    metric: 'balance',
    period: 6,
    hiddenSeries: [],
  },
}));

vi.mock('@/features/projections/components', () => ({
  ProjectionsHeader: () => <h1>Projections</h1>,
  ProjectionChart: ({
    period,
    selectedMetric,
    hiddenSeries,
    onPeriodChange,
    onMetricChange,
    onHiddenSeriesChange,
  }: {
    period: number;
    selectedMetric: ProjectionMetric;
    hiddenSeries: string[];
    onPeriodChange: (period: number) => void;
    onMetricChange: (metric: ProjectionMetric) => void;
    onHiddenSeriesChange: (hiddenSeries: string[]) => void;
  }) => (
    <div>
      <div>{`chart period:${period}`}</div>
      <div>{`chart metric:${selectedMetric}`}</div>
      <div>{`chart hidden:${hiddenSeries.join('|')}`}</div>
      <button type="button" onClick={() => onPeriodChange(9)}>
        Set period
      </button>
      <button type="button" onClick={() => onMetricChange('available')}>
        Set metric
      </button>
      <button type="button" onClick={() => onHiddenSeriesChange(['account-1'])}>
        Set hidden series
      </button>
    </div>
  ),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

describe('Core Flow Integration - Projection', () => {
  const getProjectionStorageDataMock = vi.fn();
  const setProjectionStorageDataMock = vi.fn();
  const removeStorageStartDateMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    getProjectionStorageDataMock.mockReturnValue({
      period: 6,
      metric: 'balance',
      hiddenSeries: [],
    });

    vi.mocked(useProjectionStorage).mockReturnValue({
      getProjectionStorageData: getProjectionStorageDataMock,
      setProjectionStorageData: setProjectionStorageDataMock,
      removeStorageStartDate: removeStorageStartDateMock,
    });

    vi.mocked(useApiGetProjection).mockReturnValue({
      data: new SuccessResult(createProjection()),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetProjection>);
  });

  test('renders projection chart and persists control changes', async () => {
    // Step 1: Render projections page with successful API and storage state.
    render(<ProjectionsPage />);

    // Step 2: Verify baseline projection UI is rendered.
    expect(screen.getByText('Projections')).toBeInTheDocument();
    expect(screen.getByText('chart period:6')).toBeInTheDocument();
    expect(screen.getByText('chart metric:balance')).toBeInTheDocument();

    // Step 3: Simulate user control actions on period, metric, and series visibility.
    await userEvent.click(screen.getByRole('button', { name: 'Set period' }));
    await userEvent.click(screen.getByRole('button', { name: 'Set metric' }));
    await userEvent.click(
      screen.getByRole('button', { name: 'Set hidden series' }),
    );

    // Step 4: Verify each control interaction persists expected projection state.
    expect(setProjectionStorageDataMock).toHaveBeenCalledWith({
      period: 9,
      metric: 'balance',
      hiddenSeries: [],
    });

    expect(setProjectionStorageDataMock).toHaveBeenCalledWith({
      period: 6,
      metric: 'available',
      hiddenSeries: [],
    });

    expect(setProjectionStorageDataMock).toHaveBeenCalledWith({
      period: 6,
      metric: 'balance',
      hiddenSeries: ['account-1'],
    });
  });
});
