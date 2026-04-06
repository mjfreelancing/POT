import { EnrichedDatePicker } from '@/components/picker';
import { Button } from '@/components/ui/button';
import { FORM_SHEET_STYLES } from '@/lib';

type FormDateActionRowProps = {
  selectedDate: Date | undefined;
  onDateAccepted: (date: Date | undefined) => void;
  triggerId: string;
  actionLabel: string;
  onActionClick: () => void;
  disabled?: boolean;
};

function FormDateActionRow({
  selectedDate,
  onDateAccepted,
  triggerId,
  actionLabel,
  onActionClick,
  disabled = false,
}: FormDateActionRowProps) {
  return (
    <div className={FORM_SHEET_STYLES.DATE_ROW}>
      <EnrichedDatePicker
        selectedDate={selectedDate}
        onDateAccepted={onDateAccepted}
        triggerClassName={FORM_SHEET_STYLES.DATE_TRIGGER}
        triggerId={triggerId}
        disabled={disabled}
      />
      <div className={FORM_SHEET_STYLES.DATE_ACTION_ROW}>
        <Button
          type="button"
          variant="ghost"
          size="sm"
          className={FORM_SHEET_STYLES.DATE_ACTION_BUTTON}
          onClick={onActionClick}
          disabled={disabled}
        >
          {actionLabel}
        </Button>
      </div>
    </div>
  );
}

export default FormDateActionRow;
