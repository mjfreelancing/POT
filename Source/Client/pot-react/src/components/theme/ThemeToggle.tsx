import { Moon, Sun } from "lucide-react";
import { useTheme } from "./ThemeProvider";

const ThemeToggle = () => {
  const { setTheme } = useTheme();

  return (
    // TODO: Originally contained bg-background, leave off for now until it's decided where the theme switching will be performed from
    <div className="relative inline-flex h-10 w-10 items-center justify-center rounded-md border border-input cursor-pointer hover:bg-gray-200 dark:hover:bg-gray-700">
      <Sun
        onClick={() => setTheme("dark")}
        className="h-[1.2rem] w-[1.2rem] rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0"
      />
      <Moon
        onClick={() => setTheme("light")}
        className="absolute h-[1.2rem] w-[1.2rem] rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100"
      />
    </div>
  );
};

export default ThemeToggle;
