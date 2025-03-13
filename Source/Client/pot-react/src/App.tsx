import { Button } from "@/components/ui/button";

const App = () => {
  return (
    <div className="flex flex-col items-center justify-center min-h-svh gap-1">
      <Button variant="default">Default</Button>
      <Button variant="destructive">Destructive</Button>
      <Button variant="ghost">Ghost</Button>
      <Button variant="link">Link</Button>
      <Button variant="outline">Outline</Button>
      <Button variant="secondary">Secondary</Button>
    </div>
  );
};

export default App;
