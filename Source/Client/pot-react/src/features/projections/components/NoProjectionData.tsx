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
    <Card>
      <CardHeader>
        <CardTitle>Account Balance Projections</CardTitle>
        <CardDescription className="sr-only">
          No projection data available to display
        </CardDescription>
      </CardHeader>
      <CardContent className="flex items-center justify-center h-64">
        <div className="text-center text-muted-foreground">
          <div className="text-lg font-medium mb-2">No Data Available</div>
          <div className="text-sm">No projection data available to display</div>
        </div>
      </CardContent>
    </Card>
  );
}

export default NoProjectionData;
