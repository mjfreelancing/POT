import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';

/**
 * Component displayed when there's no projection data available
 */
function NoProjectionData() {
  return (
    <Card className="flex flex-col h-full">
      <CardHeader className="flex-shrink-0">
        <CardTitle>Account Balance Projections</CardTitle>
        <CardDescription className="sr-only">
          No projection data available to display
        </CardDescription>
      </CardHeader>
      <CardContent className="flex-1 flex items-center justify-center">
        <div className="text-center text-muted-foreground">
          <div className="text-lg font-medium mb-2">No Data Available</div>
          <div className="text-sm">No projection data available to display</div>
        </div>
      </CardContent>
    </Card>
  );
}

export default NoProjectionData;
