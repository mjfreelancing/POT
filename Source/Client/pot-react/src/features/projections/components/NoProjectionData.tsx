import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';

/**
 * Props for the empty chart state. Both default to the no-data wording, so the
 * component can also be reused when there is data but every series is hidden.
 */
type NoProjectionDataProps = {
  title?: string;
  description?: string;
};

/**
 * Component displayed when there's nothing to plot: no projection data is
 * available, or every series has been hidden.
 */
function NoProjectionData({
  title = 'No Data Available',
  description = 'No projection data available to display',
}: NoProjectionDataProps) {
  return (
    <div className="flex items-center justify-center flex-1 p-6 w-full h-full">
      <Card className="flex flex-col w-full h-full">
        <CardHeader className="flex-shrink-0">
          <CardTitle className="sr-only">{title}</CardTitle>
          <CardDescription className="sr-only">{description}</CardDescription>
        </CardHeader>
        <CardContent className="flex-1 flex items-center justify-center">
          <div className="text-center text-muted-foreground">
            <div className="text-lg font-medium mb-2">{title}</div>
            <div className="text-sm">{description}</div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default NoProjectionData;
export type { NoProjectionDataProps };
