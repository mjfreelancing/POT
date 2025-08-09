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
    <div className="flex items-center justify-center flex-1 p-6 w-full h-full">
      <Card className="flex flex-col w-full h-full">
        <CardHeader className="flex-shrink-0">
          <CardTitle className="sr-only">No Data Available</CardTitle>
          <CardDescription className="sr-only">
            No projection data available to display
          </CardDescription>
        </CardHeader>
        <CardContent className="flex-1 flex items-center justify-center">
          <div className="text-center text-muted-foreground">
            <div className="text-lg font-medium mb-2">No Data Available</div>
            <div className="text-sm">
              No projection data available to display
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default NoProjectionData;
