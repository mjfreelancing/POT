import { ReactNode } from 'react';

import { ActionCard } from '@/components/cards';

/**
 * Props for a single summary card in the grid.
 */
type SummaryCardConfig = {
  title: string;
  icon: ReactNode;
  value: ReactNode;
  className?: string;
};

/**
 * Props for the summary cards grid.
 */
type SummaryCardsGridProps = {
  cards: SummaryCardConfig[];
  gridClassName?: string;
};

function SummaryCardsGrid({ cards, gridClassName }: SummaryCardsGridProps) {
  return (
    <div
      className={`grid grid-cols-1 md:grid-cols-2 gap-4 max-w-2xl w-full ${gridClassName || ''}`}
    >
      {cards.map((card, idx) => (
        <ActionCard key={card.title + idx} title={card.title} icon={card.icon}>
          <div className={card.className || ''}>{card.value}</div>
        </ActionCard>
      ))}
    </div>
  );
}

export default SummaryCardsGrid;
export type { SummaryCardConfig,SummaryCardsGridProps };
